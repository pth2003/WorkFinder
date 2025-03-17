using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WorkFinder.Web.DTOs.Jobs;
using WorkFinder.Web.Models.Enums;
using WorkFinder.Web.Repositories;

namespace WorkFinder.Web.Controllers
{
    [Route("[controller]")]
    public class JobController : Controller
    {
        private readonly IJobRepository _jobRepository;
        public JobController(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var jobs = await _jobRepository.GetAllAsync();
            var jobDto = jobs.Select(j => new JobDto
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                Requirements = j.Requirements,
                Benefits = j.Benefits,
                Location = j.Location,
                SalaryMin = j.SalaryMin,
                SalaryMax = j.SalaryMax,
                JobTypeName = j.JobType.ToString(),
                ExperienceLevelName = j.ExperienceLevel.ToString(),
                ExpiryDate = j.ExpiryDate,
                IsActive = j.IsActive,
                CompanyId = j.CompanyId,
                CompanyName = j.Company?.Name,
                CompanyLogo = j.Company?.Logo
            }).ToList();
            // Add jobs to ViewBag for easier inspection in development tools
            ViewBag.JobsDebug = jobDto;

            return View(jobDto);
        }

        [HttpGet("Job/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var job = await _jobRepository.GetJobWithDetailsAsync(id);
            if (job == null)
                return NotFound();

            return View(job);
        }
    }
}