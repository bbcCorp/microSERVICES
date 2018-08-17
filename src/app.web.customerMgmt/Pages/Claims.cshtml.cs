using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace app.web.customerMgmt.Pages
{
    [Route("identity")]
    [Authorize]
    public class IdentityModel : PageModel
    {
        public void OnGet()
        {
            // display User.Claims

        }
    }
}