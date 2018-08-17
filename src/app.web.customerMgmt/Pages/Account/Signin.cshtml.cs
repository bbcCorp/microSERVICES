using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace app.web.customerMgmt.Pages.Account
{
    public class SigninModel : PageModel
    {
        public IActionResult OnGet()
        {
            return Challenge(new AuthenticationProperties {
                // Once login succeeds, we get back to the Index page
                RedirectUri = "/Index",                
            }, "oidc");
        }
    }
}