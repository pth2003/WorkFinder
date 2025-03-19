using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkFinder.Web.ViewComponents.Job;

namespace WorkFinder.Web.Models.ViewModels
{
    public class AdvancedFilterViewModel
    {
        public List<ExperienceOption> ExperienceLevels { get; set; }
        public List<SalaryOption> SalaryRanges { get; set; }
        public List<string> JobTypes { get; set; }
        public List<string> EducationLevels { get; set; }
        public List<string> JobLevels { get; set; }

        // Add selected values
        public string SelectedJobType { get; set; }
        public string SelectedExperienceLevel { get; set; }
        public decimal? SelectedMinSalary { get; set; }
        public decimal? SelectedMaxSalary { get; set; }
        public string SelectedJobLevel { get; set; }
        public DateTime? SelectedPostedAfter { get; set; }
    }

    public class ExperienceOption
    {
        public string Value { get; set; }
        public string Label { get; set; }
    }

    public class SalaryOption
    {
        public int Min { get; set; }
        public int? Max { get; set; }

        public string DisplayText => Max.HasValue
            ? $"${Min} - ${Max}"
            : $"${Min}+";
    }
}