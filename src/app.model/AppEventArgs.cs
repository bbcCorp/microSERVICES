using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace app.model
{
    public enum AppEventType {

        // There should not be any reason to use generic events. Handlers cannot be assigned for them
        Any = 0,
        Insert = 100,
        Update = 200,
        Delete = 300
    }

    public class AppEventArgs<T> : EventArgs, IAppSerializer<AppEventArgs<T>>
    {
        [JsonProperty]
        public Guid id { get; private set; } = Guid.NewGuid();

        [JsonProperty]
        public DateTime event_ts { get; private set; } = DateTime.UtcNow;

        [JsonProperty]
        public AppEventType appEventType { get; set;}   = AppEventType.Any;

        [JsonProperty]
        public T beforeChange { get; set; }
        
        [JsonProperty]
        public T afterChange { get; set; }

        public AppEventArgs() { 
                    
        }

        [JsonConstructor]
        public AppEventArgs(Guid id, DateTime event_ts, AppEventType appEventType, T beforeChange, T afterChange)        
        {
            this.id = id;
            this.event_ts = event_ts;
            this.appEventType = appEventType;

            this.beforeChange = beforeChange;
            this.afterChange = afterChange;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static AppEventArgs<T> FromJson(string serializedJSON){
           return JsonConvert.DeserializeObject<AppEventArgs<T>>(serializedJSON); 
        }


        AppEventArgs<T> IAppSerializer<AppEventArgs<T>>.FromJson(string serializedJSON){
           return AppEventArgs<T>.FromJson(serializedJSON); 
        }
    }

    
}    