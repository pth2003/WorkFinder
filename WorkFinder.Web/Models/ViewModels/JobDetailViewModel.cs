using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkFinder.Web.DTOs.Jobs;

namespace WorkFinder.Web.Models.ViewModels
{
    public class JobDetailViewModel
    {
        // Thông tin chính của job
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
        public string Benefits { get; set; }
        public string Location { get; set; }
        public decimal SalaryMin { get; set; }
        public decimal SalaryMax { get; set; }
        public string JobType { get; set; }
        public string ExperienceLevelName { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PostedDate { get; set; }
        public string Slug { get; set; }
        
        // Thông tin nâng cao
        public string SalaryRange => $"{SalaryMin:C0}-{SalaryMax:C0}/month";
        public int DaysRemaining => Math.Max(0, (ExpiryDate - DateTime.Now).Days);
        public List<string> Categories { get; set; } = new List<string>();

        // Thông tin công ty
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyLogo { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyWebsite { get; set; }
        public string CompanyLocation { get; set; }
        public int CompanyEmployeeCount { get; set; }
        public string CompanyIndustry { get; set; }
        public bool CompanyIsVerified { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        // Các job liên quan
        public List<JobDto> RelatedJobs { get; set; } = new List<JobDto>();

        // Resume hiện có của user
        public Resume? UserResume { get; set; }
        
        // Thông tin về trạng thái đã apply
        public bool HasApplied { get; set; }
        public DateTime? PreviouslyAppliedDate { get; set; }
        
        // Format ngày apply
        public string FormattedPreviouslyAppliedDate => PreviouslyAppliedDate.HasValue 
            ? PreviouslyAppliedDate.Value.ToString("dd MMM yyyy 'at' HH:mm") 
            : string.Empty;
    }
}