using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


using app.model;
using app.model.entities;

using app.data;
using app.data.mongo;
using app.data.elastic;

using app.common;
using app.common.messaging;
using app.common.messaging.generic;


namespace app.services.replication
{
    public class AppReplicationServer
    {        
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfigurationRoot _config;
        private readonly ILogger<AppReplicationServer> _logger;

        private readonly string _kafkaServerAddress;
        private readonly string _crudMsgQueueTopic;
        private readonly string _notificationGroup;
        private readonly string _notificationMsgQueueTopic;

        private readonly app.common.messaging.generic.KafkaProducer<EmailEventArgs> _notificationProducer;
        private readonly app.common.messaging.generic.KafkaConsumer<AppEventArgs<Customer>> _appEventMsgConsumer;

        private MongoRepository<Customer> _custrepo = null;

        private readonly ElasticRepository<Customer> _searchrepo;

        public AppReplicationServer(ILoggerFactory loggerFactory, IConfigurationRoot Configuration)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException("LoggerFactory needed for AppReplicationServer initialization");

            if (Configuration == null)
                throw new ArgumentNullException("Configuration needed for AppReplicationServer initialization");

            this._loggerFactory = loggerFactory;
            this._logger = this._loggerFactory.CreateLogger<AppReplicationServer>();   
            this._config = Configuration;

            // Get Kafka Service related configuration
            this._kafkaServerAddress = Configuration["KafkaService:Server"]; 
          
            // ---------------------  Setup Kafka Consumer ------------------------- //
            this._crudMsgQueueTopic = Configuration["KafkaService:Topic"];
            this._notificationGroup =  Configuration["KafkaService:Group"];
            this._appEventMsgConsumer = new app.common.messaging.generic.KafkaConsumer<AppEventArgs<Customer>>(loggerFactory);
            var customerConfig = new Dictionary<string, object>
            {
                { "bootstrap.servers", this._kafkaServerAddress },
                { "group.id", this._notificationGroup },      
                
                { "auto.commit.interval.ms", 5000 },
                { "auto.offset.reset", "earliest" }
      
            };
            var consumeTopics = new List<string>(){ this._crudMsgQueueTopic };
            this._appEventMsgConsumer.Setup(customerConfig, consumeTopics);


            // -------------- Setup Kafka Notification Producer ----------------------- //
            this._notificationMsgQueueTopic = Configuration["KafkaService:NotificationTopic"];
            this._notificationProducer = new app.common.messaging.generic.KafkaProducer<EmailEventArgs>(loggerFactory);
            this._notificationProducer.Setup(new Dictionary<string, object>
            {
                { "bootstrap.servers", this._kafkaServerAddress }          
            });

            // -------------------- Configure data replication repository ---------------------------- //
            this._custrepo = new MongoRepository<Customer>(this._loggerFactory);
            this._custrepo.Setup(
                Configuration["ConnectionStrings:CustomerDb:url"], 
                Configuration["ConnectionStrings:CustomerDb:db"], 
                Configuration["ConnectionStrings:CustomerDb:collection"]);


            // ------------- Configure ElasticSearch repository -------------------------------- //
            this._searchrepo = new ElasticRepository<Customer>(loggerFactory, 
                Configuration["ElasticService:ServerUrl"], null, 
                "customer", 
                Configuration["ElasticService:AppIndex"]);    
        }

        public void StartServer(CancellationToken cancellationToken)
        {
            this._logger.LogDebug(LoggingEvents.Debug, "Started Data Replication Server");
            Console.WriteLine(" *** Started App Data Replication Server ***"); 

           
            // This is the event handler for emailNotification queue. Make sure this does not throw exception
            // Kafka message handling block would not be responsible to handle any exceptions 
            Func<KMessage<AppEventArgs<Customer>>, Task> appEventHandler = async (message) =>
            {
                this._logger.LogTrace(LoggingEvents.Trace, $"Response: Partition:{message.Partition}, Offset:{message.Offset} :: {message.Message}");

                AppEventArgs<Customer> evt = message.Message;

                try {
                    Customer customer = evt.afterChange;

                    switch(evt.appEventType){
                        case AppEventType.Insert:
                                _logger.LogTrace(LoggingEvents.Trace, String.Format("Adding new Customer:{0}", customer.name));
                                await _custrepo.AddAsync(customer);    
                                await this._searchrepo.AddAsync(customer);                    
                            break; 
                            
                        case AppEventType.Delete:
                            var cust = await _custrepo.GetByEntityIdAsync(customer.entityid);
                            if (cust == null)
                            {
                                _logger.LogTrace(LoggingEvents.Trace, $"Trying to delete Customer {customer.name} with EmtityID: {customer.entityid} that does not exist");
                            }
                            else {
                                await _custrepo.DeleteAsync(customer.entityid);
                            }

                            if(this._searchrepo.Exists(customer.id)){
                                await this._searchrepo.DeleteAsync(customer.id);
                            }
                            
                            break;   
                        
                        case AppEventType.Update:
                            _logger.LogTrace(LoggingEvents.Trace, $"Processing request to update customer:{customer.entityid}");
                            await _custrepo.UpdateAsync(customer);
                            await _searchrepo.UpdateAsync(customer);
                            break; 
                        
                        default:
                            _logger.LogTrace(LoggingEvents.Trace, $"No action required for event:{evt.id} of type:{evt.appEventType}");
                            break;
                            
                    }

                    this._logger.LogDebug(LoggingEvents.Trace, $"Processed Customer CRUD event {evt.id}");

                }
                catch(Exception ex){
                    var msg = $"Event:{evt.id} - Error:{ex.Message}"; 

                    this._logger.LogError(LoggingEvents.Error, ex, msg);

                    // We will send out a notification for every update
                    var notifyEvt = new EmailEventArgs{
                        subject = "Data Replication Error",
                        textMsg = $"Error replicating customer information. {msg}",
                        htmlMsg = $"<p> Error replicating customer information.  </p><p> <b>Message Details: </b> {msg}  <p>",
                        notifyTo= new List<string>() { "bedabrata.chatterjee@gmail.com" },
                        notifyCC= new List<string>(),
                        notifyBCC=new List<string>()
                    };

                    await this._notificationProducer.ProduceAsync(this._notificationMsgQueueTopic, notifyEvt);
                            
                }

            };

            this._appEventMsgConsumer.Consume(cancellationToken,appEventHandler, null, null);

            this._appEventMsgConsumer.Dispose();
            Console.WriteLine(" *** Stopped App Data Replication Server ***");    

            this._logger.LogDebug(LoggingEvents.Debug, "Stopped App Data Replication Server");
        }
    }
}