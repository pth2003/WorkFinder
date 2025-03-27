using System;
using System.ComponentModel.DataAnnotations;

namespace WorkFinder.Web.Areas.Employer.Models
{
    public class JobPostViewModel
    {
        [Required(ErrorMessage = "Job title is required")]
        public string Title { get; set; }

        public string Tags { get; set; }

        [Required(ErrorMessage = "Job role is required")]
        public string JobRole { get; set; }

        [Display(Name = "Minimum Salary")]
        public decimal? MinSalary { get; set; }

        [Display(Name = "Maximum Salary")]
        public decimal? MaxSalary { get; set; }

        [Display(Name = "Salary Type")]
        public string SalaryType { get; set; }

        [Display(Name = "Education Level")]
        public string Education { get; set; }

        [Display(Name = "Experience Level")]
        public string Experience { get; set; }

        [Display(Name = "Job Type")]
        public string JobType { get; set; }

        [Display(Name = "Number of Vacancies")]
        public int? Vacancies { get; set; }

        [Display(Name = "Application Deadline")]
        [DataType(DataType.Date)]
        public DateTime? ExpirationDate { get; set; }

        [Display(Name = "Job Level")]
        public string JobLevel { get; set; }

        [Display(Name = "Apply Method")]
        public string ApplyMethod { get; set; } // "Jobpilot", "External", "Email"

        [Display(Name = "Benefits")]
        public string Benefits { get; set; }

        [Required(ErrorMessage = "Job description is required")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Job responsibilities are required")]
        [Display(Name = "Responsibilities")]
        public string Responsibilities { get; set; }
    }
}