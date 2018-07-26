using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;


using MailKit;
using MailKit.Net.Smtp;

using app.model;
using MimeKit;

namespace app.common.notification
{
    public class EmailService
    {
        private ILogger<EmailService> _logger;
        public AppEmailConfig _mailConfig { get; set; }

        public EmailService(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException("LoggerFactory needed for object initialization");

            this._logger = loggerFactory.CreateLogger<EmailService>();
        }     

        public void Setup(string host, int port, string userid, string pwd, string mailboxName, string mailboxAddress, bool enableSSL = true)
        {
            _logger.LogTrace(LoggingEvents.Trace, $"Setting up EmailService.");

            if( String.IsNullOrEmpty(host) || String.IsNullOrEmpty(userid) || String.IsNullOrEmpty(pwd) || String.IsNullOrEmpty(mailboxName) || String.IsNullOrEmpty(mailboxAddress) )
            { 
                throw new ArgumentNullException("All parameters are required for MailService setup");
            }

            this._mailConfig = new AppEmailConfig
            {
                Host = host,
                Port = port,

                UserID = userid,
                Password = pwd,
                EnableSSL = enableSSL,

                MailboxName = mailboxName,
                MailboxAddress = mailboxAddress
            };            
        }

        public void Setup(AppEmailConfig emailConfig)
        {
            _logger.LogTrace(LoggingEvents.Trace, $"Setting up EmailService.");

            if( emailConfig == null )
            { 
                throw new ArgumentNullException("EmailConfig parameter is are required for MailService setup");
            }

            this._mailConfig = emailConfig;            
        }        

        public async Task SendEmailAsync(string email, string subject, string textMessage, string htmlMessage)
        {
            await this.SendEmailAsync(new List<string>(){ email }, subject, textMessage, htmlMessage, null, null);
        }

        public async Task SendEmailAsync(List<string> emails, string subject, string textMessage, string htmlMessage)
        {
            await this.SendEmailAsync(emails, subject, textMessage, htmlMessage, null, null);
        }

        public async Task SendEmailAsync(List<string> emailsTO, string subject, string textMessage, string htmlMessage, List<string> emailsCC = null, List<string> emailsBCC = null)
        {
            _logger.LogTrace(LoggingEvents.Trace, $"Sending out email with subject:{subject}");
            try
            {
                var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress(_mailConfig.MailboxName, _mailConfig.MailboxAddress));

                emailMessage.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = htmlMessage;
                bodyBuilder.TextBody = textMessage;
                emailMessage.Body = bodyBuilder.ToMessageBody();


                emailsTO.ForEach(email => emailMessage.To.Add(new MailboxAddress(email)) );

                if(emailsCC !=null && emailsCC.Count>0){
                    emailsCC.ForEach(email => emailMessage.Cc.Add(new MailboxAddress(email)) );
                }

                if(emailsBCC !=null && emailsBCC.Count>0){
                    emailsBCC.ForEach(email => emailMessage.Bcc.Add(new MailboxAddress(email)) );
                }
             

                using (var client = new SmtpClient())
                {
                    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    client.Connect(_mailConfig.Host, _mailConfig.Port, _mailConfig.EnableSSL);

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate(_mailConfig.UserID, _mailConfig.Password);


                    await client.SendAsync(emailMessage);

                    _logger.LogTrace(LoggingEvents.Trace, $"Sent out email with subject:{subject}");

                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.Error, ex, "Error in sending out Emails");
                throw;
            }            
        }

    }
}
