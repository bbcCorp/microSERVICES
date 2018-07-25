using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using Confluent.Kafka;
using Confluent.Kafka.Serialization;

using app.model;
using System.Text;

namespace app.common.messaging
{
    // Kafka Producer using simple string based messaging
    public class KafkaProducer<T> : IAppKafkaMessageProducer<T>, IDisposable 
                where T: IAppSerializer<T>
    {
        private readonly ILogger<KafkaProducer<T>> _logger;
        private Dictionary<string, object> _config;

        public KafkaProducer(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException("LoggerFactory needed for object initialization");

            this._logger = loggerFactory.CreateLogger<KafkaProducer<T>>();
        }

        public void Setup(Dictionary<string, object> config){
            
            _logger.LogTrace(LoggingEvents.Trace, $"Setting up Kafka streams.");
            this._config = config; 
        }

        // Producer interface
        public void Produce(string topic, T val)
        {
            ProduceAsync(topic, val).Wait();
        }

        public async Task ProduceAsync(string topic, T val)
        {   
            using (var producer = new Producer<Null, string>(this._config, null, new StringSerializer(Encoding.UTF8)))
            {
                string text = val.ToJson();

                var dr = await producer.ProduceAsync(topic, null, text);
                
                _logger.LogTrace(LoggingEvents.Trace,$"Delivered '{dr.Value}' to: {dr.TopicPartitionOffset}");                    
                
                producer.Flush(100);

            }
        }

        public void Produce(string topic, T val, int partition)
        {
            ProduceAsync(topic, val,partition).Wait();
        }

        public async Task ProduceAsync(string topic, T val, int partition)
        {
            using (var producer = new Producer<Null, string>(this._config, null, new StringSerializer(Encoding.UTF8)))            
            {
                string text = val.ToJson();

                var dr = await producer.ProduceAsync(topic, null, text,partition);
                
                _logger.LogTrace(LoggingEvents.Trace,$"Delivered '{dr.Value}' to: {dr.TopicPartitionOffset}");                    
                
                producer.Flush(100);

            }
        }

        public void Dispose()
        {
            _logger.LogDebug(LoggingEvents.Debug, $"Disposing KafkaProducer Producer.");
        }
    }
}
