using System;
using System.Text;

using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using Xunit;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

using app.model;
using app.tests.model;
using app.common.messaging;


// dotnet test --filter FullyQualifiedName~KafkaMessagingTest
namespace app.tests.kafka
{
    public class KafkaMessagingTest : AppUnitTest
    {
        private readonly string TestTopic = "KafkaMessagingTest";

        private string serverAddress;

        private CancellationTokenSource cts;

        public KafkaMessagingTest()
        {
            this.serverAddress = this.Configuration["ConnectionStrings:KafkaConnection"];

            this.cts = new CancellationTokenSource();
        }        

        private AppEventArgs<Customer> __getEvent(){
            AppEventArgs<Customer> evt = new AppEventArgs<Customer>();
            evt.beforeChange = new Customer("testentity", "1-800-PHONE") ;
            evt.afterChange = new Customer("testentity", "1-800-UPDATED");
            evt.appEventType = AppEventType.Update;

            return evt;
        }

        [Fact]
        public async Task KafkaMessagingTest001_PublishConsumeUsingStringMessage_ExpectNoExceptions()
        {
            var topic = $"{TestTopic}_Test001_{Guid.NewGuid()}";

            AppEventArgs<Customer> evt = this.__getEvent();

            var kProducer = new KafkaProducer<AppEventArgs<Customer>>(this.loggerFactory);
            var producerConfig = new Dictionary<string, object>
            {
                { "bootstrap.servers", this.serverAddress }          
            };

            kProducer.Setup(producerConfig);
            await kProducer.ProduceAsync(topic, evt);
            kProducer.Dispose();

            var kConsumer = new KafkaConsumer(this.loggerFactory);            
            var customerConfig = new Dictionary<string, object>
            {
                { "bootstrap.servers", this.serverAddress },
                { "auto.commit.interval.ms", 5000 },

                { "auto.offset.reset", "earliest" },
                { "group.id", "test-consumer-group" }            
            };

            var consumeTopics = new List<string>(){ topic };
            kConsumer.Setup(customerConfig, consumeTopics);

            //Consume yield returns a list of messages
            foreach (var message in kConsumer.Consume(this.cts.Token))
            {
                this.testLogger.LogDebug($"Response: Partition:{message.Partition}, Offset:{message.Offset} :: {message.Message}"); 
                var payload = AppEventArgs<Customer>.FromJson(message.Message);

                if(payload.id == evt.id){
                    this.testLogger.LogDebug($"Received EventID:{evt.id} from Kafka");                    
                    // break;
                    this.cts.Cancel();
                }                     
            }

            kConsumer.Dispose();

        }


        [Fact]
        public async Task KafkaMessagingTest002_PublishConsumeBulkMessages_ExpectNoExceptions()
        {
            var topic = $"{TestTopic}_Test002_{Guid.NewGuid()}";

            var kProducer = new KafkaProducer<AppEventArgs<Customer>>(this.loggerFactory);

            // Config for fast synchronous write without buffering
            var producerConfig = new Dictionary<string, object>
            {
                { "bootstrap.servers", this.serverAddress },
                
                { "retries", 0 },
                { "queue.buffering.max.ms", 0 },
                { "batch.num.messages", 1 },
                { "socket.nagle.disable", true }
            };

            kProducer.Setup(producerConfig);

            // Generate 100 events
            var opTimer = Stopwatch.StartNew();
            for(int i=0; i<100;i++){
                AppEventArgs<Customer> evt = this.__getEvent();

                // We want these events going off as soon as possible
                await kProducer.ProduceAsync(topic, evt);
                // kProducer.ProduceAsync(topic, evt);
            }
            opTimer.Stop();
            this.testLogger.LogInformation($"Took {opTimer.Elapsed.TotalSeconds} sec to send 100 events"); 
            kProducer.Dispose();
           
            var kConsumer = new KafkaConsumer(this.loggerFactory);            
            var customerConfig = new Dictionary<string, object>
            {
                { "bootstrap.servers", this.serverAddress },
                { "auto.commit.interval.ms", 5000 },

                { "auto.offset.reset", "earliest" },
                { "group.id", "test-consumer-group" }            
            };

            var consumeTopics = new List<string>(){ topic };
            kConsumer.Setup(customerConfig, consumeTopics);

            List<AppEventArgs<Customer>> events = new List<AppEventArgs<Customer>>();
            //Consume yield returns a list of messages
            
            opTimer = Stopwatch.StartNew();
            foreach (var message in kConsumer.Consume(this.cts.Token))
            {
                this.testLogger.LogTrace(LoggingEvents.Trace, $"Response: Partition:{message.Partition}, Offset:{message.Offset} :: {message.Message}"); 
                events.Add(AppEventArgs<Customer>.FromJson(message.Message));

                if(events.Count == 100){
                    opTimer.Stop();

                    this.testLogger.LogDebug($"Received 1000 events from Kafka"); 
                    this.testLogger.LogInformation($"Took {opTimer.Elapsed.TotalSeconds} sec to receive 100 events");
                   
                    // break;
                    this.cts.Cancel();
                }                     
            }

            kConsumer.Dispose();

        }

        [Fact]
        public async Task KafkaMessagingTest003_PublishNotificationMessages_ExpectNoExceptions()
        {
            //var topic = $"{TestTopic}_Test003_{Guid.NewGuid()}";
            var topic = "MICROSERVICE-CUSTOMER-EMAIL-NOTIFICATION";

            var kProducer = new KafkaProducer<EmailEventArgs>(this.loggerFactory);

            // Config for fast synchronous write without buffering
            var producerConfig = new Dictionary<string, object>
            {
                { "bootstrap.servers", this.serverAddress },
                
                { "retries", 0 },
                { "queue.buffering.max.ms", 0 },
                { "batch.num.messages", 1 },
                { "socket.nagle.disable", true }
            };

            kProducer.Setup(producerConfig);

            // Generate 10 notification events
            var opTimer = Stopwatch.StartNew();
            for(int i=0; i<10;i++){
                var evt = new EmailEventArgs{
                    subject= $"Notification Test:{i+1}",
                    textMsg= $"Notification Test:{i+1} :: BODY",
                    htmlMsg= $"<p>Notification Test: <b>{i+1}</b> :: BODY</p>",
                    notifyTo= new List<string>() { "bedabratachatterjee@yahoo.com", "bedabrata.chatterjee@gmail.com" },
                    notifyCC= new List<string>(),
                    notifyBCC=new List<string>()
                };

                // We want these events going off as soon as possible
                await kProducer.ProduceAsync(topic, evt);
                // kProducer.ProduceAsync(topic, evt);
            }
            opTimer.Stop();
            this.testLogger.LogInformation($"Took {opTimer.Elapsed.TotalSeconds} sec to send 100 events"); 
            
            kProducer.Dispose();
        }


        // [Fact]
        public async Task KafkaMessagingTest004_PublishConsumeBulkMessagesUsingWire_ExpectNoExceptions()
        {
            var topic = $"{TestTopic}_Test002_{Guid.NewGuid()}";

            var kProducer = new KafkaProducer<AppEventArgs<Customer>>(this.loggerFactory);
            

            // Config for fast synchronous write without buffering
            var producerConfig = new Dictionary<string, object>
            {
                { "bootstrap.servers", this.serverAddress },
                
                { "retries", 0 },
                { "queue.buffering.max.ms", 0 },
                { "batch.num.messages", 1 },
                { "socket.nagle.disable", true }
            };

            kProducer.Setup(producerConfig);

            // Generate 100 events
            var opTimer = Stopwatch.StartNew();
            for(int i=0; i<100;i++){
                AppEventArgs<Customer> evt = this.__getEvent();

                // We want these events going off as soon as possible
                await kProducer.ProduceAsync(topic, evt);
                // kProducer.ProduceAsync(topic, evt);
            }
            opTimer.Stop();
            this.testLogger.LogInformation($"KafkaProducer ::Took {opTimer.Elapsed.TotalSeconds} sec to send 100 events"); 
            kProducer.Dispose();

            // Test Wire based Kafka Producer
            var kProducer2 = new app.common.messaging.generic.KafkaProducer<AppEventArgs<Customer>>(this.loggerFactory);
            kProducer2.Setup(producerConfig);

            // Generate 1000 events
            opTimer = Stopwatch.StartNew();
            for(int i=0; i<100;i++){
                AppEventArgs<Customer> evt = this.__getEvent();

                // We want these events going off as soon as possible
                await kProducer2.ProduceAsync(topic, evt);
                // kProducer.ProduceAsync(topic, evt);
            }
            opTimer.Stop();
            this.testLogger.LogInformation($"KafkaProducer2::Took {opTimer.Elapsed.TotalSeconds} sec to send 100 events"); 
            kProducer2.Dispose();

        }

        [Fact]
        public async Task KafkaMessagingTest005_PublishConsumeUsingPOCOMessage_ExpectNoExceptions()
        {
            var topic = $"{TestTopic}_Test001_{Guid.NewGuid()}";

            AppEventArgs<Customer> evt = this.__getEvent();

            var kProducer = new app.common.messaging.generic.KafkaProducer<AppEventArgs<Customer>>(this.loggerFactory);
            var producerConfig = new Dictionary<string, object>
            {
                { "bootstrap.servers", this.serverAddress }          
            };

            kProducer.Setup(producerConfig);
            await kProducer.ProduceAsync(topic, evt);
            kProducer.Dispose();

            var kConsumer = new app.common.messaging.generic.KafkaConsumer<AppEventArgs<Customer>>(this.loggerFactory);            
            var customerConfig = new Dictionary<string, object>
            {
                { "bootstrap.servers", this.serverAddress },
                { "auto.commit.interval.ms", 5000 },

                { "auto.offset.reset", "earliest" },
                { "group.id", "test-consumer-group" }            
            };

            var consumeTopics = new List<string>(){ topic };
            kConsumer.Setup(customerConfig, consumeTopics);

            //Consume yield returns a list of messages
            foreach (var message in kConsumer.Consume(this.cts.Token))
            {
                this.testLogger.LogDebug($"Response: Partition:{message.Partition}, Offset:{message.Offset} :: {message.Message}"); 

                if(message.Message.id == evt.id){
                    this.testLogger.LogDebug($"Received EventID:{evt.id} from Kafka");                    
                    // break;
                    this.cts.Cancel();
                }                     
            }

            kConsumer.Dispose();

        }


    }
}