using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using app.model;
using app.model.entities;
using app.common;
using app.common.notification;

using app.common.messaging;
using app.common.messaging.generic;

namespace app.services.email
{
    public class AppEmailServer
    {        
        private readonly string KafkaServerAddress;

        private readonly EmailService mailService;

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<AppEmailServer> _logger;

        private readonly string _notificationTopic;
        private readonly string _notificationGroup;

        public AppEmailServer(ILoggerFactory loggerFactory, IConfigurationRoot Configuration)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException("LoggerFactory needed for object initialization");

            this._loggerFactory = loggerFactory;

            this._logger = this._loggerFactory.CreateLogger<AppEmailServer>();   

            
            // Get Mail Service related configuration
            string host = Configuration["SmtpService:Host"];
            int port = Convert.ToInt32( Configuration["SmtpService:Port"]);
            string userid = Configuration["SmtpService:UserID"];
            string pwd = Configuration["SmtpService:Password"];
            string mailboxName = Configuration["SmtpService:MailboxName"];
            string mailboxAddress = Configuration["SmtpService:MailboxAddress"];
            bool usessl = Convert.ToBoolean(Configuration["SmtpService:EnableSSL"]);
            
            this.mailService = new EmailService(this._loggerFactory);
            this.mailService.Setup(host, port, userid, pwd, mailboxName, mailboxAddress, usessl);    

            // Get Kafka Service related configuration
            this.KafkaServerAddress = Configuration["KafkaService:Server"]; 
            this._notificationTopic = Configuration["KafkaService:Topic"];
            this._notificationGroup =  Configuration["KafkaService:Group"];

        }

        public void StartServer(CancellationToken cancellationToken)
        {
            this._logger.LogDebug(LoggingEvents.Debug, "Started Email Notification Server");
            Console.WriteLine(" *** Started App Email Notification Server ***"); 

            var kConsumer = new KafkaConsumer<EmailEventArgs>(this._loggerFactory);            
            var customerConfig = new Dictionary<string, object>
            {
                { "bootstrap.servers", this.KafkaServerAddress },
                { "group.id", this._notificationGroup },      
                
                { "auto.commit.interval.ms", 5000 },
                { "auto.offset.reset", "earliest" }
      
            };

            var consumeTopics = new List<string>(){ this._notificationTopic };

            kConsumer.Setup(customerConfig, consumeTopics);

            Func<KMessage<EmailEventArgs>, Task> notificationHandler = async (message) =>
            {
                this._logger.LogTrace(LoggingEvents.Trace, $"Response: Partition:{message.Partition}, Offset:{message.Offset} :: {message.Message}");

                var evt = message.Message;

                if(evt.notifyTo == null || evt.notifyTo.Count == 0)
                {
                    this._logger.LogError(LoggingEvents.Critical, $"notifyTo list is not populated for the message: {message.Message}");
                    return;                         
                }

                try {

                    await this.mailService.SendEmailAsync(evt.notifyTo , evt.subject, evt.textMsg, evt.htmlMsg, evt.notifyCC, evt.notifyBCC);

                    this._logger.LogDebug(LoggingEvents.Trace, $"Processed notification event {evt.id}");

                }
                catch(Exception ex){
                    this._logger.LogError(LoggingEvents.Error, ex, $"Error processing notification event {evt.id}");
                }

            };

            kConsumer.Consume(cancellationToken,notificationHandler, null, null);

            kConsumer.Dispose();
            Console.WriteLine(" *** Stopped App Email Notification Server ***");    

            this._logger.LogDebug(LoggingEvents.Debug, "Stopped App Email Notification Server");
        }
    }
}