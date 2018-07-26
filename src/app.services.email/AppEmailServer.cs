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
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfigurationRoot _config;
        private readonly ILogger<AppEmailServer> _logger;

        private readonly string _kafkaServerAddress;
        private readonly string _notificationTopic;
        private readonly string _notificationGroup;

        private readonly string _notificationFailureTopic;        
        private readonly app.common.messaging.generic.KafkaProducer<EmailEventArgs> _notificationProducer;
        
        
        private readonly EmailService mailService;

        public AppEmailServer(ILoggerFactory loggerFactory, IConfigurationRoot Configuration)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException("LoggerFactory needed for AppEmailServer initialization");
            }


            if (Configuration == null)
            {
                throw new ArgumentNullException("Configuration needed for AppReplicationServer initialization");
            }

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
            this._kafkaServerAddress = Configuration["KafkaService:Server"]; 
            this._notificationTopic = Configuration["KafkaService:Topic"];
            this._notificationGroup =  Configuration["KafkaService:Group"];
            this._notificationFailureTopic = Configuration["KafkaService:FailedTopic"];

            this._notificationProducer = new app.common.messaging.generic.KafkaProducer<EmailEventArgs>(loggerFactory);
            this._notificationProducer.Setup(new Dictionary<string, object>
            {
                { "bootstrap.servers", this._kafkaServerAddress }          
            });
        }

        public void StartServer(CancellationToken cancellationToken)
        {
            this._logger.LogDebug(LoggingEvents.Debug, "Started Email Notification Server");
            Console.WriteLine(" *** Started App Email Notification Server ***"); 

            var kConsumer = new KafkaConsumer<EmailEventArgs>(this._loggerFactory);            
            var customerConfig = new Dictionary<string, object>
            {
                { "bootstrap.servers", this._kafkaServerAddress },
                { "group.id", this._notificationGroup },      
                
                { "auto.commit.interval.ms", 5000 },
                { "auto.offset.reset", "earliest" }
      
            };

            var consumeTopics = new List<string>(){ this._notificationTopic };

            kConsumer.Setup(customerConfig, consumeTopics);

            // This is the event handler for emailNotification queue. Make sure this does not throw exception
            // Kafka message handling block would not be responsible to handle any exceptions 
            Func<KMessage<EmailEventArgs>, Task> notificationHandler = async (message) =>
            {
                this._logger.LogTrace(LoggingEvents.Trace, $"Response: Partition:{message.Partition}, Offset:{message.Offset} :: {message.Message}");

                var evt = message.Message;
                
                this._logger.LogDebug(LoggingEvents.Trace, $"Processing notification event {evt.id} with subject:{evt.subject}");

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
                    var msg = $"Event:{evt.id} - Retry:{evt.retries +1} - Error:{ex.Message}"; 

                    this._logger.LogError(LoggingEvents.Error, ex, msg);

                    try {
                        evt.retries += 1; 
                        if(evt.retryLog == null){
                            evt.retryLog = new List<string>();                        
                        }
                        evt.retryLog.Add(msg);

                        if(evt.retries > 3){
                            // Give up 
                            await this._notificationProducer.ProduceAsync(this._notificationFailureTopic, evt);
                            this._logger.LogInformation(LoggingEvents.Critical, $"Stopping notification attempt for {evt.id} after {evt.retries} retries"); 
                        }
                        else {
                            // Put the message back for retries
                            await this._notificationProducer.ProduceAsync(this._notificationTopic, evt); 
                        }
                    }
                    catch(Exception ex2){
                        
                        this._logger.LogCritical(LoggingEvents.Critical, ex2, $"Event:{evt.id} - Retry:{evt.retries +1} - Error:{ex2.Message}");
                    }              
                    
                }

            };

            kConsumer.Consume(cancellationToken,notificationHandler, null, null);

            kConsumer.Dispose();
            Console.WriteLine(" *** Stopped App Email Notification Server ***");    

            this._logger.LogDebug(LoggingEvents.Debug, "Stopped App Email Notification Server");
        }
    }
}