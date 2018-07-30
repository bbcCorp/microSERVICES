using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using AutoMapper;

using app.model;
using app.model.entities;
using app.data;
using app.data.mongo;
using app.api.customers.Model;

using app.common.messaging.generic;

namespace app.api.customers.Controllers
{
    [Route("api/[controller]")]
    public class CustomersController : Controller
    {
        private readonly IConfiguration _config;
        private MongoRepository<Customer> _custrepo = null;
        private readonly ILogger<CustomersController> _logger;
        private IMapper _mapper;

        private LoggerFactory _loggerFactory;

        private List<Guid> _evtHandlerLst;

        private readonly app.common.messaging.generic.KafkaProducer<AppEventArgs<Customer>> _appEventProducer;
        private readonly app.common.messaging.generic.KafkaProducer<EmailEventArgs> _notificationProducer;
    
        private readonly string _notificationMsgQueueTopic;
        private readonly string _crudMsgQueueTopic;
        public CustomersController(IConfiguration Configuration, LoggerFactory loggerFactory, IMapper mapper)
        {
            this._config = Configuration;
            this._mapper = mapper;
            this._loggerFactory = loggerFactory;


            this._logger = loggerFactory.CreateLogger<CustomersController>();
                    
            // -------------- Setup Kafka AppEvent Producer ----------------------- //
            this._crudMsgQueueTopic = Configuration["KafkaService:Topic"];
            this._appEventProducer = new app.common.messaging.generic.KafkaProducer<AppEventArgs<Customer>>(loggerFactory);
            this._appEventProducer.Setup(new Dictionary<string, object>
            {
                { "bootstrap.servers", this._config["KafkaService:Server"] }          
            });

            this._notificationMsgQueueTopic = Configuration["KafkaService:NotificationTopic"];
            // -------------- Setup Kafka Notification Producer ----------------------- //
            this._notificationProducer = new app.common.messaging.generic.KafkaProducer<EmailEventArgs>(loggerFactory);
            this._notificationProducer.Setup(new Dictionary<string, object>
            {
                { "bootstrap.servers", this._config["KafkaService:Server"] }          
            });

            // Configure repository
            this._evtHandlerLst = new List<Guid>();
            __setupRepo();
        }

        private void __setupRepo(){

            // Configure repository           
            this._custrepo = new MongoRepository<Customer>(this._loggerFactory);
            this._custrepo.Setup(
                this._config["ConnectionStrings:CustomerDb:url"], 
                this._config["ConnectionStrings:CustomerDb:db"], 
                this._config["ConnectionStrings:CustomerDb:collection"]);

            Func<AppEventArgs<Customer>, Task> evtStreamHandler = (evt) =>
            {
                // Queue application event
                return Task.Run( async () =>
                {
                    this._logger.LogTrace(LoggingEvents.Trace, $"Creating update stream message for evt:{evt.appEventType} for Customer:{evt.afterChange.entityid}");
                    
                    // Required to fix serialization error during Insert op
                    if(evt.appEventType==AppEventType.Insert){
                        evt.beforeChange = new Customer();
                    }
                    await this._appEventProducer.ProduceAsync(this._crudMsgQueueTopic, evt);                    
                });
            };

            Func<AppEventArgs<Customer>, Task> evtMailHandler = (evt) =>
            {

                // We will send out a notification for every update
                var notifyEvt = new EmailEventArgs{
                
                    notifyTo= new List<string>() { evt.afterChange.email },
                    notifyCC= new List<string>(),
                    notifyBCC=new List<string>()
                };

                switch(evt.appEventType){
                    case AppEventType.Insert:
                        notifyEvt.subject= $"Your record has been created";
                        notifyEvt.textMsg= $"Dear {evt.afterChange.name}, Your record has been created. Please verify the details \n Phone:{evt.afterChange.phone}";
                        notifyEvt.htmlMsg= $"<p>Dear {evt.afterChange.name}, <br/> Your record has been created. <br/> Please verify the details:- <br/> Phone: <b>{evt.afterChange.phone}</b></p>";
                        break; 
                        
                    case AppEventType.Delete:
                        notifyEvt.subject= $"Your record has been deleted";
                        notifyEvt.textMsg= $"Dear {evt.afterChange.name}, We are sorry to see you leave!";
                        notifyEvt.htmlMsg= $"<p>Dear {evt.afterChange.name}, <br/> We are sorry to see you leave! </p>";
                        break;   
                    
                    case AppEventType.Update:
                    default:
                        notifyEvt.subject= $"Your information has been updated";
                        notifyEvt.textMsg= $"Dear {evt.afterChange.name}, Your information has been updated. Please verify the details \n Phone:{evt.afterChange.phone}";
                        notifyEvt.htmlMsg= $"<p>Dear {evt.afterChange.name}, <br/> Your information has been updated. <br/> Please verify the details:- <br/> Phone: <b>{evt.afterChange.phone}</b></p>";
                        break; 
                           
                }

                // Queue notification event
                return Task.Run( async () =>
                {
                    this._logger.LogTrace(LoggingEvents.Trace, $"Creating notification message for evt:{evt.appEventType} for Customer:{evt.afterChange.entityid}");
                    
                    try{
                        await this._notificationProducer.ProduceAsync(this._notificationMsgQueueTopic, notifyEvt);
                    }
                    catch(Exception ex){
                       this._logger.LogError(LoggingEvents.Critical, ex, $"Error in steaming evt:{evt.appEventType} for Customer:{evt.afterChange.entityid}"); 
                    }
                                        
                });

            };
            
            this._evtHandlerLst.Add(this._custrepo.Subscribe(AppEventType.Any, evtStreamHandler));
            this._evtHandlerLst.Add(this._custrepo.Subscribe(AppEventType.Any, evtMailHandler));

        }

        // GET api/customers
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {                
                var records = _mapper.Map<List<Customer>,List<CustomerVM>>( await _custrepo.GetAsync() );
                return Ok(records);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.Error, ex, $"ERROR: Could not create a retrieve information from customer repository");
            }

            return BadRequest("Internal Error: Could not retrieve customer information");
        }

        // GET api/customers/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return BadRequest("Customer ID is required to fetch customer record");
            }

            _logger.LogInformation (LoggingEvents.Trace, String.Format("Retrieving information for CustomerID:{0}", id));

            try
            {

                var result = _mapper.Map<Customer,CustomerVM>( await _custrepo.GetByEntityIdAsync(id) );

                _logger.LogInformation (LoggingEvents.Trace, String.Format("Retrieved information for CustomerID:{0}", id));

                return Ok(result);
            }
            catch(Exception ex){
                _logger.LogError(LoggingEvents.Error, ex, $"ERROR: Unable to retrieve information for customer {id}");
            }
            
            return BadRequest($"Error: Unable to retrieve for customer ID:{id}");
        }

        // POST api/customers
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerVM cust)
        {
            if (! ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                Customer customer = _mapper.Map<CustomerVM, Customer>(cust);

                _logger.LogTrace(LoggingEvents.Trace, String.Format("Adding new Customer:{0}", customer.name));

                await _custrepo.AddAsync(customer);

                _logger.LogInformation(LoggingEvents.Critical, String.Format("Added new CustomerID:{0}-{1}", customer.entityid , customer.name));
             
                return Ok(customer.entityid);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.Error, ex, String.Format("ERROR: Unable to add customer"));
            }

            return BadRequest("Error: Unable to add customer");            

        }

        // PUT api/customers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody]CustomerVM cust)
        {
            if (String.IsNullOrEmpty(id))
            {
                return BadRequest("Customer ID is required to update customer record");
            }

            if(cust == null){
                return BadRequest("Request could not be processed. Parameter is not in the desired format. Please refer to the API documentation");
            }

            try{
                _logger.LogTrace(LoggingEvents.Trace, $"Processing request to update customer:{cust.entityid}");

                var entity = await _custrepo.GetByEntityIdAsync(id);
                if (entity == null)
                {
                    return NotFound();
                }

                entity.name = cust.name;
                entity.phone = cust.phone;                

                await _custrepo.UpdateAsync(entity);
                return Ok();
            }
            catch(Exception ex){
                _logger.LogError(LoggingEvents.Error, ex, String.Format("ERROR: Unable to update customer"));
            }
            return BadRequest($"ERROR: Unable to update customer: {cust.entityid}"); 
        }

        // DELETE api/customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return BadRequest("Customer ID is required to delete customer record");
            }

            try
            {
                var cust = await _custrepo.GetByEntityIdAsync(id);
                if (cust == null)
                {
                    return NotFound();
                }

                await _custrepo.DeleteAsync(id);

                return Ok();
            }
            catch(Exception ex){
                _logger.LogError(LoggingEvents.Error, ex, $"ERROR: Unable to delete customer: {id}");
            }

            return BadRequest($"ERROR: Unable to delete customer: {id}"); 
        }
    }
}
