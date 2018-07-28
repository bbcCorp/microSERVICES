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
    public class KafkaConsumer<T> : IAppKafkaMessageConsumer<T>, IDisposable
    {
        private readonly ILogger<KafkaConsumer<T>> _logger;
        private Dictionary<string, object> _config;

        private List<string> _topics;
        private Consumer<Null, T> _consumer;

        public KafkaConsumer(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException("LoggerFactory needed for object initialization");

            this._logger = loggerFactory.CreateLogger<KafkaConsumer<T>>();
        }

        public void Setup(Dictionary<string, object> config, List<string> topics)
        {
            if (config == null)
            {
                throw new ArgumentException("Config is required to setup Kafka Consumer");
            }

            _logger.LogTrace(LoggingEvents.Trace, $"Setting up Kafka streams.");

            this._config = config;
            this._topics = topics;

            this._consumer = new Consumer<Null, T>(this._config, null, new AppWireSerializer<T>());
        }

        // Implement Consumer interface
        public IEnumerable<KMessage<T>> Consume(CancellationToken cancellationToken)
        {
            if (this._consumer == null)
            {
                throw new InvalidOperationException("You need to setup Kafka Consumer before you can consume messages");
            }

            _logger.LogDebug(LoggingEvents.Debug, $"Reading from Kafka streams.");

            var msgbuffer = new List<KMessage<T>>();


            this._consumer.Subscribe(this._topics);

            this._consumer.OnMessage += (_, msg) =>
            {

                    // _logger.LogTrace(LoggingEvents.Trace,$"Read '{msg.Value}' from: {msg.TopicPartitionOffset}");
                    // _logger.LogTrace(LoggingEvents.Trace,$"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} {msg.Value}");

                    this._consumer.CommitAsync(msg);

                var payload = new KMessage<T>()
                {
                    Topic = msg.Topic,
                    Partition = msg.Partition,
                    Offset = msg.Offset.Value,
                    Message = msg.Value
                };
                msgbuffer.Add(payload);
            };

            this._consumer.OnError += (_, error)
                => _logger.LogError(LoggingEvents.Error, $"Error: {error}");

            this._consumer.OnConsumeError += (_, msg)
                => _logger.LogError(LoggingEvents.Error, $"Consume error ({msg.TopicPartitionOffset}): {msg.Error}");

            while ((cancellationToken == null) || (!cancellationToken.IsCancellationRequested))
            {
                this._consumer.Poll(TimeSpan.FromMilliseconds(100));

                if (msgbuffer.Count > 0)
                {
                    foreach (var msg in msgbuffer)
                    {
                        yield return msg;
                    }
                    msgbuffer.Clear();
                }
            }


        }

        public void Consume(CancellationToken cancellationToken,
            Func<KMessage<T>, Task> messageHandler,
            Func<Error, Task> errorHandler,
            Func<Message, Task> consumptionErrorHandler
        )
        {
            if (this._consumer == null)
            {
                throw new InvalidOperationException("You need to setup Kafka Consumer before you can consume messages");
            }

            _logger.LogDebug(LoggingEvents.Debug, $"Reading from Kafka streams.");


            this._consumer.Subscribe(this._topics);

            this._consumer.OnMessage += async (_, msg) =>
            {

                    // _logger.LogTrace(LoggingEvents.Trace,$"Read '{msg.Value}' from: {msg.TopicPartitionOffset}");
                    // _logger.LogTrace(LoggingEvents.Trace,$"Topic: {msg.Topic} Partition: {msg.Partition} Offset: {msg.Offset} {msg.Value}");

                    var payload = new KMessage<T>()
                {
                    Topic = msg.Topic,
                    Partition = msg.Partition,
                    Offset = msg.Offset.Value,
                    Message = msg.Value
                };

                try
                {
                    await messageHandler(payload);
                }
                catch (Exception ex)
                {
                    _logger.LogError(LoggingEvents.Error, ex, $"Message handler for topic:{msg.Topic} Offset:{msg.Offset.Value} resullted in error");
                }

                await this._consumer.CommitAsync(msg);
            };

            this._consumer.OnError += (_, error) =>
            {
                _logger.LogError(LoggingEvents.Error, $"Error: {error}");

                if (errorHandler != null)
                {
                    errorHandler(error);
                }

            };

            this._consumer.OnConsumeError += (_, msg) =>
            {
                _logger.LogError(LoggingEvents.Error, $"Consume error ({msg.TopicPartitionOffset}): {msg.Error}");

                if (consumptionErrorHandler != null)
                {
                    consumptionErrorHandler(msg);
                }
            };

            while ((cancellationToken == null) || (!cancellationToken.IsCancellationRequested))
            {
                this._consumer.Poll(TimeSpan.FromMilliseconds(100));
            }


        }

        public void Dispose()
        {
            _logger.LogDebug(LoggingEvents.Debug, $"Disposing KafkaConsumer.");

            this._consumer.Dispose();
        }

    }
}
