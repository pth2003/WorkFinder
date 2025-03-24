using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WorkFinder.Web.Areas.Employer.Views.Company
{
    public class SetupBasic : PageModel
    {
        private readonly ILogger<SetupBasic> _logger;

        public SetupBasic(ILogger<SetupBasic> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}