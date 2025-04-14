using System;
using System.Collections.Generic;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;

namespace WorkFinder.Web.Areas.Employer.Models
{
    public class EmployerJobDetailViewModel
    {
        // Job details
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
        public string Benefits { get; set; }
        public string Location { get; set; }
        public decimal SalaryMin { get; set; }
        public decimal SalaryMax { get; set; }
        public JobType JobType { get; set; }
        public ExperienceLevel ExperienceLevel { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Metadata { get; set; }
        public string Slug { get; set; }

        // Computed properties
        public string SalaryRange => $"{SalaryMin:C0}-{SalaryMax:C0}/month";
        public int DaysRemaining => Math.Max(0, (ExpiryDate - DateTime.Now).Days);
        public string JobTypeName => JobType.ToString();
        public string ExperienceLevelName => ExperienceLevel.ToString();

        // Company information
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyLogo { get; set; }
        public string CompanyLocation { get; set; }

        // Application statistics
        public int TotalApplications { get; set; }
        public int NewApplications { get; set; }
        public int ReviewingApplications { get; set; }
        public int InterviewApplications { get; set; }
        public int AcceptedApplications { get; set; }
        public int RejectedApplications { get; set; }

        // Additional metadata parsed properties
        public Dictionary<string, string> MetadataDict { get; set; }

        public string JobRole => MetadataDict?.GetValueOrDefault("JobRole", Title);
        public string JobLevel => MetadataDict?.GetValueOrDefault("JobLevel", "Entry");
        public int Vacancies => int.TryParse(MetadataDict?.GetValueOrDefault("Vacancies", "1"), out var v) ? v : 1;
        public string SalaryType => MetadataDict?.GetValueOrDefault("SalaryType", "Monthly");
        public string ApplyMethod => MetadataDict?.GetValueOrDefault("ApplyMethod", "Jobpilot");
    }
}