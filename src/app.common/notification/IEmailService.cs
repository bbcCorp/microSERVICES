using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

namespace app.common.notification
{
    public interface IEmailService
    {
        void Setup(string host, int port, string userid, string pwd, bool enableSSL = true);
        Task SendEmailAsync(string email, string subject, string textMessage, string htmlMessage);
        Task SendEmailAsync(List<string> emails, string subject, string textMessage, string htmlMessage); 

        Task SendEmailAsync(List<string> emailsTO, string subject, string textMessage, string htmlMessage, List<string> emailsCC = null, List<string> emailsBCC = null);       
    }
}    