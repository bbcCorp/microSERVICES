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

using app.common.serialization;

namespace app.common.messaging.generic
{
    // Kafka Consumer using POCO based messaging using Wire serializer
    public class KafkaConsumer<T> : IAppKafkaMessageConsumer<T> , IDisposable
    {
        private readonly ILogger<KafkaConsumer<T>> _logger;
        private Dictionary<string, object> _config;

        private List<string> _topics;

        public KafkaConsumer(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException("LoggerFactory needed for object initialization");

            this._logger = loggerFactory.CreateLogger<KafkaConsumer<T>>();
        }

        public void Setup(Dictionary<string, object> config, List<string> topics){
            
            _logger.LogTrace(LoggingEvents.Trace, $"Setting up Kafka streams.");

            this._config = config; 
            this._topics = topics;            
        }

        // Implement Consumer interface
        public IEnumerable<KMessage<T>> Consume(CancellationToken cancellationToken)
        {
            if(this._config == null){
                throw new InvalidOperationException("You need Setup Kafka config");
            }

            _logger.LogDebug(LoggingEvents.Debug, $"Reading from Kafka streams.");

            var msgbuffer = new List<KMessage<T>>();

            using (var consumer = new Consumer<Null, T>(this._config, null, new AppWireSerializer<T>() ))
            {
                consumer.Subscribe(this._topics);

                consumer.OnMessage += (_, msg) =>
                {

                    // _logger.LogTrace(LoggingEvents.Trace,$"Read '{msg.Value}' from: {msg.TopicPartitionOffset}");
                    // _logger.LogTrace(LoggingEvents.Trace,$"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} {msg.Value}");
                    
                    consumer.CommitAsync(msg);

                    var payload = new KMessage<T>(){
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

        public void Consume(CancellationToken cancellationToken, 
            Func<KMessage<T>, Task> messageHandler,
            Func<Error, Task> errorHandler,
            Func<Message, Task> consumptionErrorHandler
        )
        {
            if(this._config == null){
                throw new InvalidOperationException("You need Setup Kafka config");
            }

            _logger.LogDebug(LoggingEvents.Debug, $"Reading from Kafka streams.");

            using (var consumer = new Consumer<Null, T>(this._config, null, new AppWireSerializer<T>() ))
            {
                consumer.Subscribe(this._topics);

                consumer.OnMessage += async (_, msg) =>
                {

                    // _logger.LogTrace(LoggingEvents.Trace,$"Read '{msg.Value}' from: {msg.TopicPartitionOffset}");
                    // _logger.LogTrace(LoggingEvents.Trace,$"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} {msg.Value}");
                                        
                    var payload = new KMessage<T>(){
                        Topic = msg.Topic,
                        Partition = msg.Partition,
                        Offset = msg.Offset.Value,
                        Message = msg.Value
                    };

                    try {
                        await messageHandler(payload);
                    }
                    catch(Exception ex){
                        _logger.LogError(LoggingEvents.Error,ex,$"Message handler for topic:{msg.Topic} Offset:{msg.Offset.Value} resullted in error");
                    }
                    
                    await consumer.CommitAsync(msg);
                };

                consumer.OnError += (_, error) => {
                    _logger.LogError(LoggingEvents.Error,$"Error: {error}");

                    if(errorHandler != null){
                        errorHandler(error);
                    }

                };

                consumer.OnConsumeError += (_, msg) => {
                    _logger.LogError(LoggingEvents.Error,$"Consume error ({msg.TopicPartitionOffset}): {msg.Error}");
                
                    if(consumptionErrorHandler !=null){
                        consumptionErrorHandler(msg);
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
