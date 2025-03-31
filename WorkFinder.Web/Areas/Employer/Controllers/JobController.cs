using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using WorkFinder.Web.Areas.Employer.Models;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;
using WorkFinder.Web.Repositories;
using Microsoft.Extensions.Logging;

namespace WorkFinder.Web.Areas.Employer.Controllers
{
    [Area("Employer")]
    [Authorize(Roles = "Employer")]
    public class JobController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJobRepository _jobRepository;
        private readonly ICategoryRepository? _categoryRepository;
        private readonly ILogger<JobController> _logger;

        public JobController(
            UserManager<ApplicationUser> userManager,
            ICompanyRepository companyRepository,
            IJobRepository jobRepository,
            ILogger<JobController> logger,
            ICategoryRepository? categoryRepository = null)
        {
            _userManager = userManager;
            _companyRepository = companyRepository;
            _jobRepository = jobRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var company = await _companyRepository.GetByOwnerIdAsync(user.Id);
            if (company == null)
            {
                return RedirectToAction("SetupBasic", "Company", new { area = "Employer" });
            }

            // Lấy dữ liệu trang đầu tiên
            var (jobs, totalCount) = await _jobRepository.GetAllJobsByCompanyIdAsync(company.Id, 1, 10);
            ViewBag.TotalCount = totalCount;
            return View(jobs);
        }

        [HttpGet]
        public async Task<IActionResult> GetJobs(int page = 1, int pageSize = 10)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var company = await _companyRepository.GetByOwnerIdAsync(user.Id);
                if (company == null)
                {
                    return Json(new { success = false, message = "Company not found" });
                }

                // Đảm bảo pageSize hợp lệ
                pageSize = Math.Max(1, Math.Min(pageSize, 100));
                page = Math.Max(1, page);

                var (jobs, totalCount) = await _jobRepository.GetAllJobsByCompanyIdAsync(company.Id, page, pageSize);

                // Tính toán totalPages chính xác
                var totalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;

                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                    MaxDepth = 32
                };

                return Json(new
                {
                    success = true,
                    data = jobs,
                    pagination = new
                    {
                        currentPage = page,
                        totalPages,
                        totalCount,
                        pageSize
                    }
                }, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs");
                return Json(new { success = false, message = "Failed to load jobs: " + ex.Message });
            }
        }

        public IActionResult Create()
        {
            var model = new JobPostViewModel
            {
                ApplyMethod = "Jobpilot"
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobPostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                            )
                    });
                }
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "User not found" });
                }
                return NotFound();
            }

            var company = await _companyRepository.GetByOwnerIdAsync(user.Id);
            if (company == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Company not found" });
                }
                return RedirectToAction("SetupBasic", "Company", new { area = "Employer" });
            }

            try
            {
                var job = await CreateJobFromViewModel(model, company.Id);
                await _jobRepository.AddAsync(job);
                await _jobRepository.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        message = "Job posted successfully!",
                        redirectUrl = Url.Action("Index", "Job")
                    });
                }

                return RedirectToAction("Index", "Job", new { toastMessage = "Job posted successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job");
                ModelState.AddModelError("", "An error occurred while saving the job: " + ex.Message);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to create job: " + ex.Message });
                }

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var company = await _companyRepository.GetByOwnerIdAsync(user.Id);
            if (company == null)
            {
                return NotFound();
            }

            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            if (job.CompanyId != company.Id)
            {
                return Forbid();
            }

            await _jobRepository.DeleteAsync(id);
            await _jobRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { toastMessage = "Job deleted successfully!" });
        }

        private async Task<Job> CreateJobFromViewModel(JobPostViewModel model, int companyId)
        {
            var jobType = Enum.TryParse<JobType>(model.JobType?.Replace(" ", ""), out var parsedJobType)
                ? parsedJobType
                : JobType.FullTime;

            var experienceLevel = Enum.TryParse<ExperienceLevel>(model.Experience?.Split(' ')[0], out var parsedExpLevel)
                ? parsedExpLevel
                : ExperienceLevel.Entry;

            var metadata = new Dictionary<string, string>
            {
                { "SalaryType", model.SalaryType ?? "Monthly" },
                { "ApplyMethod", model.ApplyMethod ?? "Jobpilot" },
                { "Vacancies", model.Vacancies?.ToString() ?? "1" },
                { "JobLevel", model.JobLevel ?? "Entry" },
                { "JobRole", model.JobRole ?? model.Title }
            };

            return new Job
            {
                Title = model.Title,
                Description = model.Description,
                Requirements = model.Responsibilities,
                JobType = jobType,
                ExperienceLevel = experienceLevel,
                SalaryMin = model.MinSalary ?? 0,
                SalaryMax = model.MaxSalary ?? 0,
                Location = await GetCompanyLocation(companyId),
                Benefits = !string.IsNullOrEmpty(model.Benefits) ? model.Benefits : "Standard benefits package",
                ExpiryDate = model.ExpirationDate.HasValue
                    ? DateTime.SpecifyKind(model.ExpirationDate.Value, DateTimeKind.Utc)
                    : DateTime.UtcNow.AddMonths(1),
                IsActive = true,
                CompanyId = companyId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Metadata = System.Text.Json.JsonSerializer.Serialize(metadata)
            };
        }

        private async Task<string> GetCompanyLocation(int companyId)
        {
            try
            {
                var company = await _jobRepository.GetCompanyByIdAsync(companyId);
                return !string.IsNullOrEmpty(company?.Location) ? company.Location : "Remote";
            }
            catch
            {
                return "Remote";
            }
        }
    }
}

