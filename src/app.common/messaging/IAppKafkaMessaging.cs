using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using Confluent.Kafka;
using app.model;
using System.Threading;

namespace app.common.messaging
{
    public interface IAppKafkaMessageProducer<T> where T : IAppSerializer<T>
    {
        void Setup(Dictionary<string, object> config);

        // Producer interface
        void Produce(string topic, T val);
        Task ProduceAsync(string topic, T val);
    }

    // String based messaging
    public interface IAppKafkaMessageConsumer
    {
        void Setup(Dictionary<string, object> config, List<string> topics);

        IEnumerable<KMessage> Consume(CancellationToken cancellationToken);

        void Consume(CancellationToken cancellationToken, 
            Func<KMessage, Task> messageHandler,
            Func<Error, Task> errorHandler,
            Func<Message, Task> consumptionErrorHandler
        );
    }

    // POCO based messaging using Wire serialization
    public interface IAppKafkaMessageConsumer<T>
    {
        void Setup(Dictionary<string, object> config, List<string> topics);

        IEnumerable<KMessage<T>> Consume(CancellationToken cancellationToken);

        void Consume(CancellationToken cancellationToken, 
            Func<KMessage<T>, Task> messageHandler,
            Func<Error, Task> errorHandler,
            Func<Message, Task> consumptionErrorHandler
        );
    }

}
