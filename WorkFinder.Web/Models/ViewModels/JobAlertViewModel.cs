using System;

namespace WorkFinder.Web.Models.ViewModels
{
    public class JobAlertViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string JobTitle { get; set; }
        public string Location { get; set; }
        public string SalaryRange { get; set; }
        public string JobType { get; set; }
        public bool IsActive { get; set; }
        public string Frequency { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}