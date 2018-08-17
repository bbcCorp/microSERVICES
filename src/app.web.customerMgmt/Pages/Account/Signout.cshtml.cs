using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;

namespace app.web.customerMgmt.Pages.Account
{
    public class SignoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            return SignOut(new AuthenticationProperties{
                RedirectUri = "/Index"
            }, "Cookies");
        }
    }
}