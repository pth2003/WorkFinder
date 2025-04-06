using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WorkFinder.Web.Views.Candidate
{
    public class FavoriteJobs : PageModel
    {
        private readonly ILogger<FavoriteJobs> _logger;

        public FavoriteJobs(ILogger<FavoriteJobs> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}