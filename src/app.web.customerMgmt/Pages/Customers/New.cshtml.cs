using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using app.model;
using app.model.entities;
using Microsoft.AspNetCore.Authorization;

namespace app.web.customerMgmt.Pages.Customers
{
    [Authorize]
    public class NewModel : PageModel
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

        public NewModel(LoggerFactory loggerFactory, IConfiguration configuration)
        {
            this._logger = loggerFactory.CreateLogger<IndexModel>();

            this._config = configuration;
            apiPath = configuration["API_URL"];            
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if(!ModelState.IsValid){
                // Rerender the current page
                ActionMessage = "Invalid/Incomplete data for Customer";
                return Page();
            }      

            try{

                HttpClient client = new HttpClient();

                var path = $"{apiPath}";

                this._logger.LogTrace(LoggingEvents.Trace, $"Calling API: {path}");
                HttpResponseMessage response = await client.PostAsJsonAsync(path, Customer);
                if (response.IsSuccessStatusCode)
                {
                    var entityid = response.Content.ReadAsStringAsync().Result;
                    ActionMessage = $"Created new customer: {entityid}"; 
                }
                else
                {
                    var msg = "Could not create new customer";
                    ActionMessage = msg;
                   
                    this._logger.LogError(LoggingEvents.Error, $"{msg}. {response.StatusCode}::{response.Content}");
                }

                return RedirectToPage("Index");
            }
            catch(Exception ex){
                ActionMessage = $"Could not add new customer.";
                this._logger.LogError(LoggingEvents.Error, ex, $"Could not add customer record. {ex.Message}");
                return Page();
            }

        }
    }
}