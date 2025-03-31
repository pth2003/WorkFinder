using System.Collections.Generic;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.Areas.Employer.Models
{
    public class EmployerDashboardViewModel
    {
        public string CompanyName { get; set; }
        public string CompanyLogo { get; set; }

        // Số liệu thống kê trên dashboard
        public int OpenJobs { get; set; }
        public int SavedCandidates { get; set; }

        // Danh sách công việc gần đây
        public List<RecentJobViewModel> RecentJobs { get; set; } = new List<RecentJobViewModel>();
    }

    public class RecentJobViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string JobType { get; set; } // Full Time, Part Time, Internship
        public bool IsActive { get; set; }
        public int DaysRemaining { get; set; }
        public int ApplicationCount { get; set; }
        public string ExpirationDate { get; set; }
        public List<JobApplication> Applications { get; set; } = new List<JobApplication>();
    }
}