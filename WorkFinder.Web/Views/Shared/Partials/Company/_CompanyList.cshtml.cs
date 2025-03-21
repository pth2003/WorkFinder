using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WorkFinder.Web.Views.Shared.Partials.Company
{
    public class _CompanyList : PageModel
    {
        private readonly ILogger<_CompanyList> _logger;

        public _CompanyList(ILogger<_CompanyList> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}