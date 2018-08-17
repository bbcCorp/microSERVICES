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
    public class EditModel : PageModel
    {
        private readonly string apiPath;
        private readonly IConfiguration _config;
        protected ILogger _logger;

        // On POST, we want to bind this property
        [BindProperty]
        public Customer Customer { get; set; }

        // This will be used to pass action message
        [TempData]
        public string ActionMessage { get; set; }


        public EditModel(LoggerFactory loggerFactory, IConfiguration configuration)
        {
            this._logger = loggerFactory.CreateLogger<EditModel>();

            this._config = configuration;
            apiPath = configuration["API_URL"];            
        }

        public async Task OnGet(string id)
        {
            if(String.IsNullOrEmpty(id)){
                ActionMessage = $"Customer ID could not be determined.";
                return ;
            }

            try
            {

                HttpClient client = new HttpClient();

                var path = $"{apiPath}/{id}";

                this._logger.LogTrace(LoggingEvents.Trace, $"Calling API: {path}");
                HttpResponseMessage response = await client.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    Customer = await response.Content.ReadAsAsync<Customer>();
                    ActionMessage = $"Retrieved customer: {id}"; 
                }
                else
                {
                    if(response.StatusCode == HttpStatusCode.NotFound){
                        ActionMessage = "Customer does not exist!";
                    }
                    else {
                        ActionMessage = $"Customer: {id} could not be retrieved.";
                    }
                    
                    this._logger.LogError(LoggingEvents.Error, $"Could not delete customer:{id}. {response.StatusCode}::{response.Content}");
                }

            }
            catch(Exception ex)
            {
                ActionMessage = $"Customer:{id} could not be retrieved.";
                this._logger.LogError(LoggingEvents.Error, ex, $"Could not read customer record:{id}. {ex.Message}");
            }

            
        }

        // This is a Create New page, so we have no onGet method. We will handle onPost
        public async Task<IActionResult> OnPostAsync()
        {
            if(!ModelState.IsValid){
                // Rerender the current page
                ActionMessage = "Invalid/Incomplete data for Customer";
                return Page();
            }      

            try {

                HttpClient client = new HttpClient();

                var path = $"{apiPath}/{Customer.entityid}";

                this._logger.LogTrace(LoggingEvents.Trace, $"Calling API: {path}");
                HttpResponseMessage response = await client.PutAsJsonAsync(path, Customer);
                if (response.IsSuccessStatusCode)
                {
                    ActionMessage = $"Updated customer:{Customer.entityid}"; 
                }
                else
                {
                    if(response.StatusCode == HttpStatusCode.NotFound){
                        ActionMessage = "Customer does not exist!";
                    }
                    else {
                        ActionMessage = $"Customer: {Customer.entityid} could not be updated.";
                    }
                    
                    this._logger.LogError(LoggingEvents.Error, $"Could not update customer:{Customer.entityid}. {response.StatusCode}::{response.Content}");
                }

                return RedirectToPage("Index");
            }
            catch(Exception ex){
                ActionMessage = "We could not update Customer record";

                this._logger.LogError(LoggingEvents.Error, ex, $"Could not update customer record. {ex.Message}");
                return Page();
            }

        }
    }
}