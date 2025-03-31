using System.Collections.Generic;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.Areas.Employer.Models
{
    public class JobApplicationsViewModel
    {
        public Job Job { get; set; }
        public IList<JobApplication> Applications { get; set; } = new List<JobApplication>();
        public string Filter { get; set; } = "All";
        public int TotalApplications { get; set; }
        public int ShortlistedCount { get; set; }
        public int NewCount { get; set; }
    }
}