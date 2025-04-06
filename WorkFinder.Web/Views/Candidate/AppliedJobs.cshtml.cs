using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WorkFinder.Web.Views.Candidate
{
    public class AppliedJobs : PageModel
    {
        private readonly ILogger<AppliedJobs> _logger;

        public AppliedJobs(ILogger<AppliedJobs> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}