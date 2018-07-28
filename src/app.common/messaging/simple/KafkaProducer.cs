using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using Confluent.Kafka;
using Confluent.Kafka.Serialization;

using app.model;
using System.Text;

namespace app.common.messaging.simple
{
    // Kafka Producer using simple string based messaging
    public class KafkaProducer<T> : IAppKafkaMessageProducer<T>, IDisposable
                where T : IAppSerializer<T>
    {
        private readonly ILogger<KafkaProducer<T>> _logger;
        private Dictionary<string, object> _config;

        private Producer<Null, string> _producer;

        public KafkaProducer(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException("LoggerFactory needed for object initialization");

            this._logger = loggerFactory.CreateLogger<KafkaProducer<T>>();

        }

        public void Setup(Dictionary<string, object> config)
        {
            if (config == null)
            {
                throw new ArgumentException("Config is required to setup Kafka Producer");
            }

            _logger.LogTrace(LoggingEvents.Trace, $"Setting up Kafka streams.");
            this._config = config;

            this._producer = new Producer<Null, string>(this._config, null, new StringSerializer(Encoding.UTF8));
        }

        // Producer interface
        public void Produce(string topic, T val)
        {
            ProduceAsync(topic, val).Wait();
        }

        public async Task ProduceAsync(string topic, T val)
        {
            if(this._producer == null){
                throw new InvalidOperationException("You need to setup Kafka Producer before you can publish messages");
            }

            string text = val.ToJson();

            var dr = await this._producer.ProduceAsync(topic, null, text);

            _logger.LogTrace(LoggingEvents.Trace, $"Delivered '{dr.Value}' to: {dr.TopicPartitionOffset}");

            this._producer.Flush(100);

        }

        public void Produce(string topic, T val, int partition)
        {
            ProduceAsync(topic, val, partition).Wait();
        }

        public async Task ProduceAsync(string topic, T val, int partition)
        {
            if(this._producer == null){
                throw new InvalidOperationException("You need to setup Kafka Producer before you can publish messages");
            }

            string text = val.ToJson();

            var dr = await this._producer.ProduceAsync(topic, null, text, partition);

            _logger.LogTrace(LoggingEvents.Trace, $"Delivered '{dr.Value}' to: {dr.TopicPartitionOffset}");

            this._producer.Flush(100);

        }

        public void Dispose()
        {
            _logger.LogDebug(LoggingEvents.Debug, $"Disposing KafkaProducer Producer.");

            this._producer.Flush(100);
            this._producer.Dispose();
        }
    }
}
