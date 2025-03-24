using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WorkFinder.Web.Areas.Employer.Views.Company
{
    public class SetupOrganization : PageModel
    {
        private readonly ILogger<SetupOrganization> _logger;

        public SetupOrganization(ILogger<SetupOrganization> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}