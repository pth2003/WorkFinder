using System;
using System.Collections.Generic;

namespace WorkFinder.Web.Models.ViewModels
{
    public class DashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public int AppliedJobsCount { get; set; }
        public int FavoriteJobsCount { get; set; }
        public int JobAlertsCount { get; set; }
        public Resume? Resume { get; set; }
        public List<AppliedJobViewModel> RecentlyAppliedJobs { get; set; } = new();
    }

    public class AppliedJobViewModel
    {
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string JobType { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Salary { get; set; } = string.Empty;
        public string CompanyLogo { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime DateApplied { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}