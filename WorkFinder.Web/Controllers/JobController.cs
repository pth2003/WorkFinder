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
        private readonly ICategoryRepository _categoryRepository;
        public JobController(IJobRepository jobRepository, ICategoryRepository categoryRepository)
        {
            _jobRepository = jobRepository;
            _categoryRepository = categoryRepository;
        }
        // get all jobs
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index(
                string keyword = "",
                string location = "",
                int? categoryId = null,
                string jobType = null,
                string experienceLevel = null,
                decimal? minSalary = null,
                decimal? maxSalary = null,
                string jobLevel = null,
                DateTime? postedAfter = null,
                int page = 1,
                int pageSize = 12)
        {
            // Chuyển đổi string thành enum nếu có giá trị
            JobType? jobTypeEnum = null;
            if (!string.IsNullOrEmpty(jobType) && Enum.TryParse<JobType>(jobType.Replace(" ", ""), out var parsedJobType))
            {
                jobTypeEnum = parsedJobType;
            }

            ExperienceLevel? experienceLevelEnum = null;
            if (!string.IsNullOrEmpty(experienceLevel))
            {
                var firstWord = experienceLevel.Split(' ')[0];
                if (Enum.TryParse<ExperienceLevel>(firstWord, out var parsedExpLevel))
                {
                    experienceLevelEnum = parsedExpLevel;
                }
            }
            ExperienceLevel? jobLevelEnum = null;
            if (!string.IsNullOrEmpty(jobLevel))
            {
                var firstWord = jobLevel.Split(' ')[0];
                if (Enum.TryParse<ExperienceLevel>(firstWord, out var parsedJobLevel))
                {
                    jobLevelEnum = parsedJobLevel;
                }
            }

            // Sử dụng phương thức phân trang với bộ lọc nâng cao
            if (postedAfter.HasValue)
            {
                // Chuyển đổi thành UTC trước khi truy vấn
                postedAfter = DateTime.SpecifyKind(postedAfter.Value, DateTimeKind.Utc);
            }
            var (jobs, totalCount) = await _jobRepository.GetJobsAdvancedPagedAsync(
                keyword, location, categoryId, jobTypeEnum, experienceLevelEnum,
                minSalary, maxSalary, jobLevelEnum, postedAfter, page, pageSize);

            // Chuyển đổi sang DTO
            var jobDtos = jobs.Select(j => new JobDto
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                Requirements = j.Requirements,
                Benefits = j.Benefits,
                Location = j.Location,
                SalaryMin = j.SalaryMin,
                SalaryMax = j.SalaryMax,
                JobType = j.JobType.ToString(),
                ExperienceLevelName = j.ExperienceLevel.ToString(),
                ExpiryDate = j.ExpiryDate,
                IsActive = j.IsActive,
                CompanyId = j.CompanyId,
                CompanyName = j.Company?.Name,
                Logo = j.Company?.Logo
            }).ToList();

            // Lấy danh sách categories cho dropdown
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = categories;

            // Tính toán thông tin phân trang
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Truyền thông tin phân trang và bộ lọc vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalJobs = totalCount;
            ViewBag.Keyword = keyword;
            ViewBag.Location = location;
            ViewBag.CategoryId = categoryId;
            ViewBag.JobType = jobType;
            ViewBag.ExperienceLevel = experienceLevel;
            ViewBag.MinSalary = minSalary;
            ViewBag.MaxSalary = maxSalary;
            ViewBag.JobLevel = jobLevel;
            ViewBag.PostedAfter = postedAfter;

            // Add jobs to ViewBag for easier inspection in development tools
            ViewBag.JobsDebug = jobDtos;


            return View(jobDtos);
        }

        // get job details
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