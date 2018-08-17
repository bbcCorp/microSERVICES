using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using app.model;
using app.model.entities;
using Microsoft.AspNetCore.Authorization;

namespace app.web.customerMgmt.Pages.Customers
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly string apiPath;
        private readonly IConfiguration _config;
        protected ILogger _logger;

        public IList<Customer> Customers { get; set; }
        public bool HasCustomers => (Customers != null) && (Customers.Count() > 0);

        [TempData]
        public string ActionMessage { get; set; }
        public bool ShowActionMessage => (!String.IsNullOrEmpty(ActionMessage));

        public string Message { get; set; }

        public IndexModel(LoggerFactory loggerFactory, IConfiguration configuration)
        {            
            this._logger = loggerFactory.CreateLogger<IndexModel>();

            this._config = configuration;
            apiPath = configuration["API_URL"];            
        }

        public async Task OnGetAsync()
        {
            try
            {
                HttpClient client = new HttpClient();
                var path = apiPath;

                this._logger.LogTrace(LoggingEvents.Trace, $"Calling API: {path}");
                HttpResponseMessage response = await client.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    Customers = await response.Content.ReadAsAsync<List<Customer>>();
                }
                
                if (Customers.Count == 0)
                {
                    Message = $"There are no customer records";
                }
                else
                {
                    Message = $"Showing {Customers.Count} records";
                }   
            }
            catch(Exception ex)
            {
                Message = $"Error in fetching Customer information.";
                this._logger.LogError(LoggingEvents.Error, ex, $"Error in fetching Customer information. {ex.Message}");
            }            

        }

        // Pages routing looks for method names with "On", followed by HTTP verb, handler name and Async (optional)        
        public async Task<IActionResult> OnPostDeleteAsync(string id){
            
            try
            {
                HttpClient client = new HttpClient();

                var path = $"{apiPath}/{id}";

                this._logger.LogTrace(LoggingEvents.Trace, $"Calling API: {path}");
                HttpResponseMessage response = await client.DeleteAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    ActionMessage = "Deleted customer"; 

                    return RedirectToPage("Index");
                }
                else
                {
                    if(response.StatusCode == HttpStatusCode.NotFound){
                        ActionMessage = "Customer does not exist!";
                    }
                    else {
                        ActionMessage = $"Customer: {id} could not be deleted.";
                    }
                    
                    this._logger.LogError(LoggingEvents.Error, $"Could not delete customer:{id}. {response.StatusCode}::{response.Content}");
                }

            }
            catch(Exception ex)
            {
                ActionMessage = $"Customer could not be deleted.";
                this._logger.LogError(LoggingEvents.Error, ex, $"Could not delete customer record:{id}. {ex.Message}");
            }

            return Page();
        }

    }
}
