using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace app.model
{

    public class EmailEventArgs : EventArgs, IAppSerializer<EmailEventArgs>
    {
        [JsonProperty]
        public Guid id { get; private set; } = Guid.NewGuid();

        [JsonProperty]
        public DateTime event_ts { get; private set; } = DateTime.UtcNow;

        [JsonProperty]
        public AppEventType appEventType { get; set;}   = AppEventType.Any;

        [JsonProperty]
        public string subject { get; set;}

        [JsonProperty]
        public string textMsg { get; set;}

        [JsonProperty]
        public string htmlMsg { get; set;}

        [JsonProperty]
        public List<string> notifyTo { get; set;}

        [JsonProperty]
        public List<string> notifyCC { get; set;}

        [JsonProperty]
        public List<string> notifyBCC { get; set;}

        public EmailEventArgs() { 
                    
        }

        [JsonConstructor]
        public EmailEventArgs(Guid id, DateTime event_ts, AppEventType appEventType, 
            string subject, string textMsg, string htmlMsg,
            List<string> notifyTo, List<string> notifyCC, List<string> notifyBCC
            )        
        {
            this.id = id;
            this.event_ts = event_ts;
            this.appEventType = appEventType;

            this.subject = subject;
            this.textMsg = textMsg;
            this.htmlMsg = htmlMsg;
            this.notifyTo = notifyTo;
            this.notifyCC = notifyCC;
            this.notifyBCC = notifyBCC;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static EmailEventArgs FromJson(string serializedJSON){
           return JsonConvert.DeserializeObject<EmailEventArgs>(serializedJSON); 
        }

        EmailEventArgs IAppSerializer<EmailEventArgs>.FromJson(string serializedJSON){
           return EmailEventArgs.FromJson(serializedJSON); 
        }

    }

}