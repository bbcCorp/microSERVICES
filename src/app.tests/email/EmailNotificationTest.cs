using System;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

using Xunit;
using Microsoft.Extensions.Logging;

using app.common.notification;


// dotnet test --filter FullyQualifiedName~EmailNotificationTest
namespace app.tests.email
{
    public class EmailNotificationTest : AppUnitTest
    {
        private readonly EmailService mailService;
        public EmailNotificationTest()
        {
            this.mailService = new EmailService(this.loggerFactory);

            string host = this.Configuration["SmtpServer:Host"];
            int port = Convert.ToInt32(this.Configuration["SmtpServer:Port"]);
            string userid = this.Configuration["SmtpServer:UserID"];
            string pwd = this.Configuration["SmtpServer:Password"];
            string mailboxName = this.Configuration["SmtpServer:MailboxName"];
            string mailboxAddress = this.Configuration["SmtpServer:MailboxAddress"];
            bool usessl = Convert.ToBoolean(this.Configuration["SmtpServer:EnableSSL"]);
            
            this.mailService.Setup(host, port, userid, pwd, mailboxName, mailboxAddress, usessl);
        }

        [Fact]
        public async Task EmailNotificationTest001_SendSingleTextMessage_ExpectNoExceptions()
        {
            var sendTo ="bedabratachatterjee@yahoo.com";
            await this.mailService.SendEmailAsync(sendTo, "test subject", "text msg", null);
        }

        [Fact]
        public async Task EmailNotificationTest002_SendSingleHTMLMessage_ExpectNoExceptions()
        {
            var sendTo ="bedabratachatterjee@yahoo.com";
            await this.mailService.SendEmailAsync(sendTo, "test subject", "text msg", "<b>Test Message</b>");
        }

        [Fact]
        public async Task EmailNotificationTest003_SendTextMessageToMultipleRecipient_ExpectNoExceptions()
        {
            var sendTo = new List<string>() { "bedabratachatterjee@yahoo.com", "bedabrata.chatterjee@gmail.com" };
            await this.mailService.SendEmailAsync(sendTo, "test subject", "text msg", null);
        }

    }
}