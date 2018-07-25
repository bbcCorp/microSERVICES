using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using Confluent.Kafka;
using Confluent.Kafka.Serialization;

using app.model;
using System.Text;
using System.Threading;

namespace app.common.messaging
{
    // Kafka Consumer using simple string based messaging
    public class KafkaConsumer : IAppKafkaMessageConsumer , IDisposable
    {
        private readonly ILogger<KafkaConsumer> _logger;
        private Dictionary<string, object> _config;

        private List<string> _topics;

        public KafkaConsumer(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException("LoggerFactory needed for object initialization");

            this._logger = loggerFactory.CreateLogger<KafkaConsumer>();
        }

        public void Setup(Dictionary<string, object> config, List<string> topics){
            
            _logger.LogTrace(LoggingEvents.Trace, $"Setting up Kafka streams.");

            this._config = config; 
            this._topics = topics;            
        }

        // Implement Consumer interface
        public IEnumerable<KMessage> Consume(CancellationToken cancellationToken)
        {
            _logger.LogDebug(LoggingEvents.Debug, $"Reading from Kafka streams.");

            List<KMessage> msgbuffer = new List<KMessage>();

            using (var consumer = new Consumer<Null, string>(this._config, null, new StringDeserializer(Encoding.UTF8)))
            {
                consumer.Subscribe(this._topics);

                consumer.OnMessage += (_, msg) =>
                {

                    // _logger.LogTrace(LoggingEvents.Trace,$"Read '{msg.Value}' from: {msg.TopicPartitionOffset}");
                    // _logger.LogTrace(LoggingEvents.Trace,$"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} {msg.Value}");
                    
                    consumer.CommitAsync(msg);

                    var payload = new KMessage(){
                        Topic = msg.Topic,
                        Partition = msg.Partition,
                        Offset = msg.Offset.Value,
                        Message = msg.Value
                    };
                    msgbuffer.Add(payload);
                };

                consumer.OnError += (_, error)
                    => _logger.LogError(LoggingEvents.Error,$"Error: {error}");

                consumer.OnConsumeError += (_, msg)
                    => _logger.LogError(LoggingEvents.Error,$"Consume error ({msg.TopicPartitionOffset}): {msg.Error}");

                while ((cancellationToken == null) || (!cancellationToken.IsCancellationRequested))
                {
                    consumer.Poll(TimeSpan.FromMilliseconds(100));
                    
                    if(msgbuffer.Count > 0){
                        foreach(var msg in msgbuffer){
                            yield return msg;
                        }                        
                        msgbuffer.Clear();
                    }
                }
            }

        }

        // Implement Consumer interface
        public void Consume(CancellationToken cancellationToken, 
            Func<KMessage, Task> messageHandler,
            Func<Error, Task> errorHandler,
            Func<Message, Task> consumptionErrorHandler
        )
        {
            _logger.LogDebug(LoggingEvents.Debug, $"Reading from Kafka streams.");

            using (var consumer = new Consumer<Null, string>(this._config, null, new StringDeserializer(Encoding.UTF8)))
            {
                consumer.Subscribe(this._topics);

                consumer.OnMessage += async (_, msg) =>
                {

                    // _logger.LogTrace(LoggingEvents.Trace,$"Read '{msg.Value}' from: {msg.TopicPartitionOffset}");
                    // _logger.LogTrace(LoggingEvents.Trace,$"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} {msg.Value}");
                    
                    var payload = new KMessage(){
                        Topic = msg.Topic,
                        Partition = msg.Partition,
                        Offset = msg.Offset.Value,
                        Message = msg.Value
                    };
                    await messageHandler(payload);
                    await consumer.CommitAsync(msg);

                };

                consumer.OnError += async (_, error)  => 
                {
                    _logger.LogError(LoggingEvents.Error,$"Error: {error}");
                    if(errorHandler != null){
                        await errorHandler(error);
                    }
                };

                consumer.OnConsumeError += async (_, msg) => {
                    _logger.LogError(LoggingEvents.Error,$"Consume error ({msg.TopicPartitionOffset}): {msg.Error}");

                    if(consumptionErrorHandler !=null){
                        await consumptionErrorHandler(msg);
                    }
                };

                while ((cancellationToken == null) || (!cancellationToken.IsCancellationRequested))
                {
                    consumer.Poll(TimeSpan.FromMilliseconds(100));
                }
            }

        }

        public void Dispose()
        {
            _logger.LogDebug(LoggingEvents.Debug, $"Disposing KafkaConsumer.");
        }

    }
}
