using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WorkFinder.Web.Areas.Auth.Services;
using WorkFinder.Web.DTOs.JobApplications;
using WorkFinder.Web.DTOs.Jobs;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;
using WorkFinder.Web.Models.ViewModels;
using WorkFinder.Web.Repositories;

namespace WorkFinder.Web.Controllers
{
    [Route("[controller]")]
    public class JobController : Controller
    {
        private readonly IJobRepository _jobRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IResumeRepository _resumeRepository;
        private readonly IAuthService _authService;

        public JobController(
            IJobRepository jobRepository,
            ICategoryRepository categoryRepository,
            IResumeRepository resumeRepository,
            IAuthService authService)
        {
            _jobRepository = jobRepository;
            _categoryRepository = categoryRepository;
            _resumeRepository = resumeRepository;
            _authService = authService;
        }
        // get all jobs with advanced filter
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



            return View(jobDtos);
        }

        // get job details by id
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var job = await _jobRepository.GetJobWithDetailsAsync(id);
            if (job == null)
                return NotFound();

            // Log authentication status
            Console.WriteLine($"DEBUG: User.Identity.IsAuthenticated={User.Identity.IsAuthenticated}");

            var relatedJobs = await _jobRepository.GetRelatedJobsAsync(job.CompanyId);
            var relatedJobDtos = relatedJobs.Where(j => j.Id != id).Take(3).Select(j => new JobDto
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

            // Lấy thông tin resume của user hiện tại nếu đã đăng nhập
            Resume? userResume = null;
            if (User.Identity.IsAuthenticated)
            {
                var user = await _authService.GetCurrentUserAsync();

                // Set debug info
                ViewBag.CurrentUserId = user?.Id;
                ViewBag.CurrentUserEmail = user?.Email;

                if (user != null)
                {
                    userResume = await _resumeRepository.GetResumeByUserIdAsync(user.Id);

                }
            }

            // Tạo ViewModel
            var viewModel = new JobDetailViewModel
            {
                // Thông tin công việc
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Requirements = job.Requirements,
                Benefits = job.Benefits,
                Location = job.Location,
                SalaryMin = job.SalaryMin,
                SalaryMax = job.SalaryMax,
                JobType = job.JobType.ToString(),
                ExperienceLevelName = job.ExperienceLevel.ToString(),
                ExpiryDate = job.ExpiryDate,
                IsActive = job.IsActive,
                CreatedAt = job.CreatedAt,
                PostedDate = job.CreatedAt,

                // Thông tin danh mục
                Categories = job.Categories.Select(c => c.Category.Name).ToList(),

                // Thông tin công ty
                CompanyId = job.CompanyId,
                CompanyName = job.Company.Name,
                CompanyLogo = job.Company.Logo,
                CompanyDescription = job.Company.Description,
                CompanyWebsite = job.Company.Website,
                CompanyLocation = job.Company.Location,
                CompanyEmployeeCount = job.Company.EmployeeCount,
                CompanyIndustry = job.Company.Industry,
                CompanyIsVerified = job.Company.IsVerified,
                Email = "career@" + job.Company.Website.Replace("https://", "").Replace("http://", ""),
                Phone = "(406) 555-0120", // Giả định hoặc lấy từ thông tin công ty

                // Job liên quan
                RelatedJobs = relatedJobDtos,

                // Resume hiện có
                UserResume = userResume
            };
            return View(viewModel);
        }

        [HttpPost]
        [Authorize] // Yêu cầu đăng nhập để gửi application
        [Route("Apply/{id}")]
        public async Task<IActionResult> Apply(int id, JobApplicationDto model)
        {
            if (!ModelState.IsValid)
            {
                // Return to the job details page with validation errors
                TempData["ErrorMessage"] = "Please fill all required fields correctly.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Lấy thông tin user hiện tại
            var user = await _authService.GetCurrentUserAsync();

            // Set debug info
            ViewBag.CurrentUserId = user?.Id;
            ViewBag.CurrentUserEmail = user?.Email;

            if (user == null)
            {
                TempData["ErrorMessage"] = "Unable to identify user. Please try again.";
                return RedirectToAction(nameof(Details), new { id });
            }

            int applicantId = user.Id;

            // Kiểm tra xem user đã apply job này chưa
            bool hasApplied = await _jobRepository.HasUserAppliedToJobAsync(id, applicantId);
            if (hasApplied)
            {
                TempData["ErrorMessage"] = "You have already applied to this job.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Xác định đường dẫn đến resume
            string resumeFileUrl;

            if (model.UseExistingResume)
            {
                // Sử dụng resume đã có
                var existingResume = await _resumeRepository.GetResumeByUserIdAsync(applicantId);
                if (existingResume == null || string.IsNullOrEmpty(existingResume.FileUrl))
                {
                    TempData["ErrorMessage"] = "No resume found. Please upload a resume.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                resumeFileUrl = existingResume.FileUrl;
            }
            else
            {
                // Upload resume mới
                if (model.ResumeFile == null || model.ResumeFile.Length == 0)
                {
                    TempData["ErrorMessage"] = "Resume file is required.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ResumeFile.FileName)}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "resumes");

                // Ensure directory exists
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ResumeFile.CopyToAsync(fileStream);
                }

                resumeFileUrl = $"/uploads/resumes/{fileName}";

                // Kiểm tra xem user đã có resume chưa, nếu chưa thì tạo mới
                if (!await _resumeRepository.UserHasResumeAsync(applicantId))
                {
                    var resume = new Resume
                    {
                        UserId = applicantId,
                        FileUrl = resumeFileUrl,
                        Summary = "", // Default empty values
                        Skills = "",
                        Education = "",
                        Experience = "",
                        Certifications = "",
                        Languages = ""
                    };

                    await _resumeRepository.AddResumeAsync(resume);
                }
                else
                {
                    // Cập nhật FileUrl cho resume hiện có
                    var existingResume = await _resumeRepository.GetResumeByUserIdAsync(applicantId);
                    if (existingResume != null)
                    {
                        existingResume.FileUrl = resumeFileUrl;
                        await _resumeRepository.UpdateAsync(existingResume);
                        await _resumeRepository.SaveChangesAsync();
                    }
                }
            }

            // Tạo job application
            var jobApplication = new JobApplication
            {
                JobId = id,
                ApplicantId = applicantId,
                CoverLetter = model.CoverLetter,
                Status = ApplicationStatus.Applied,
                AppliedDate = DateTime.UtcNow
            };

            // Lưu job application vào database
            await _jobRepository.AddJobApplicationAsync(jobApplication);

            // Success message and redirect
            TempData["SuccessMessage"] = "Your application has been submitted successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}