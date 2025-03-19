using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WorkFinder.Web.Models.ViewModels;
using WorkFinder.Web.Repositories;
namespace WorkFinder.Web.ViewComponents.Job
{
    public class AdvancedFilterViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string jobType = null,
        string experienceLevel = null,
        decimal? minSalary = null,
        decimal? maxSalary = null,
        string jobLevel = null,
        DateTime? postedAfter = null)
        {
            var viewModel = new AdvancedFilterViewModel
            {
                ExperienceLevels = new List<ExperienceOption>
            {
                new ExperienceOption { Value = "freshers", Label = "Freshers" },
                new ExperienceOption { Value = "1-2", Label = "1 - 2 Years" },
                new ExperienceOption { Value = "2-4", Label = "2 - 4 Years" },
                new ExperienceOption { Value = "4-6", Label = "4 - 6 Years" },
                new ExperienceOption { Value = "6-8", Label = "6 - 8 Years" },
                new ExperienceOption { Value = "8-10", Label = "8 - 10 Years" },
                new ExperienceOption { Value = "10-15", Label = "10 - 15 Years" },
                new ExperienceOption { Value = "15+", Label = "15+ Years" }
            },
                SalaryRanges = new List<SalaryOption>
            {
                new SalaryOption { Min = 50, Max = 1000 },
                new SalaryOption { Min = 1000, Max = 2000 },
                new SalaryOption { Min = 3000, Max = 4000 },
                new SalaryOption { Min = 4000, Max = 6000 },
                new SalaryOption { Min = 6000, Max = 8000 },
                new SalaryOption { Min = 8000, Max = 10000 },
                new SalaryOption { Min = 10000, Max = 15000 },
                new SalaryOption { Min = 15000, Max = null }
            },
                JobTypes = new List<string>
            {
                "All", "Full Time", "Part Time", "Internship", "Remote", "Temporary", "Contract Base"
            },
                EducationLevels = new List<string>
            {
                "All", "High School", "Intermediate", "Graduation", "Master Degree", "Bachelor Degree"
            },
                JobLevels = new List<string>
            {
                "Entry Level", "Mid Level", "Expert Level"
            },

                // Set selected values
                SelectedJobType = jobType,
                SelectedExperienceLevel = experienceLevel,
                SelectedMinSalary = minSalary,
                SelectedMaxSalary = maxSalary,
                SelectedJobLevel = jobLevel,
                SelectedPostedAfter = postedAfter
            };

            return View(viewModel);
        }
    }



}