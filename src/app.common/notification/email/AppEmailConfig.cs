using System;

namespace app.common.notification
{       
    public class AppEmailConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserID { get; set; }
        public string Password { get; set; }
        public bool EnableSSL = true;

        // Required to send emails
        public string MailboxName { get; set;}
        public string MailboxAddress { get; set;}   

        public AppEmailConfig(){}
        public AppEmailConfig(string host, int port, string userid, string pwd, string mailboxName, string mailboxAddress, bool enableSSL = true)
        {
            if( String.IsNullOrEmpty(host) || String.IsNullOrEmpty(userid) || String.IsNullOrEmpty(pwd) || String.IsNullOrEmpty(mailboxName) || String.IsNullOrEmpty(mailboxAddress) )
            { 
                throw new ArgumentNullException("All parameters are required for EMailConfig setup");
            }

            this.Host = host;
            this.Port = port;

            this.UserID = userid;
            this.Password = pwd;
            this.EnableSSL = enableSSL;

            this.MailboxName = mailboxName;
            this.MailboxAddress = mailboxAddress;
        }     
    }
}    