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
using WorkFinder.Web.Extensions;

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
                // Create new job
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

                TempData["SuccessMessage"] = "Job posted successfully!";
                return RedirectToAction("Index", "Job");
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
            try
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

                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        message = "Job deleted successfully!"
                    });
                }

                // Set success message in TempData
                TempData["SuccessMessage"] = "Job deleted successfully!";

                // Stay on the same page
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job");

                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to delete job: " + ex.Message
                    });
                }

                TempData["ErrorMessage"] = "Failed to delete job: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
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
                Slug = model.Title.ToSlug(),
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

        [HttpGet]
        [Route("/Employer/Job/JobDetail/{id}")]
        public async Task<IActionResult> JobDetail(int id)
        {
            try
            {
                _logger.LogInformation($"Attempting to retrieve job details for job ID: {id}");

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning($"User not authenticated when accessing job details for job ID: {id}");
                    return RedirectToAction("Login", "Auth", new { area = "Auth" });
                }

                var company = await _companyRepository.GetByOwnerIdAsync(user.Id);
                if (company == null)
                {
                    _logger.LogWarning($"User {user.Id} does not have an associated company when accessing job details");
                    return RedirectToAction("SetupBasic", "Company", new { area = "Employer" });
                }

                _logger.LogInformation($"Retrieving job with applications for job ID: {id}");
                var job = await _jobRepository.GetJobWithApplicationsAsync(id);

                if (job == null)
                {
                    _logger.LogWarning($"Job with ID {id} not found");
                    TempData["ErrorMessage"] = "Job not found";
                    return RedirectToAction(nameof(Index));
                }

                if (job.CompanyId != company.Id)
                {
                    _logger.LogWarning($"User {user.Id} with company {company.Id} attempted to access job {id} belonging to company {job.CompanyId}");
                    TempData["ErrorMessage"] = "You don't have permission to view this job";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation($"Successfully retrieved job {id} with {job.Applications?.Count ?? 0} applications");

                var metadataDict = new Dictionary<string, string>();
                try
                {
                    if (!string.IsNullOrEmpty(job.Metadata))
                    {
                        metadataDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(job.Metadata);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error parsing job metadata for job ID {id}");
                }

                // Count applications by status
                int totalApplications = job.Applications?.Count ?? 0;
                int newApplications = job.Applications?.Count(a => a.Status == ApplicationStatus.Applied) ?? 0;
                int reviewingApplications = job.Applications?.Count(a => a.Status == ApplicationStatus.Reviewing) ?? 0;
                int interviewApplications = job.Applications?.Count(a => a.Status == ApplicationStatus.Interview) ?? 0;
                int acceptedApplications = job.Applications?.Count(a => a.Status == ApplicationStatus.Accepted) ?? 0;
                int rejectedApplications = job.Applications?.Count(a => a.Status == ApplicationStatus.Rejected) ?? 0;

                var viewModel = new EmployerJobDetailViewModel
                {
                    Id = job.Id,
                    Title = job.Title,
                    Description = job.Description,
                    Requirements = job.Requirements,
                    Benefits = job.Benefits,
                    Location = job.Location,
                    SalaryMin = job.SalaryMin,
                    SalaryMax = job.SalaryMax,
                    JobType = job.JobType,
                    ExperienceLevel = job.ExperienceLevel,
                    ExpiryDate = job.ExpiryDate,
                    IsActive = job.IsActive,
                    CreatedAt = job.CreatedAt,
                    UpdatedAt = job.UpdatedAt,
                    Metadata = job.Metadata,
                    Slug = job.Slug,

                    CompanyId = company.Id,
                    CompanyName = company.Name,
                    CompanyLogo = company.Logo,
                    CompanyLocation = company.Location,

                    TotalApplications = totalApplications,
                    NewApplications = newApplications,
                    ReviewingApplications = reviewingApplications,
                    InterviewApplications = interviewApplications,
                    AcceptedApplications = acceptedApplications,
                    RejectedApplications = rejectedApplications,

                    MetadataDict = metadataDict
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving job details for job ID {id}");
                TempData["ErrorMessage"] = $"An error occurred while retrieving job details: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(JobPostViewModel model, int jobId)
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

                ViewData["EditMode"] = true;
                ViewData["JobId"] = jobId;
                return View("Create", model);
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
                // Get the existing job
                var existingJob = await _jobRepository.GetByIdAsync(jobId);
                if (existingJob == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Job not found" });
                    }
                    TempData["ErrorMessage"] = "Job not found";
                    return RedirectToAction(nameof(Index));
                }

                // Verify ownership
                if (existingJob.CompanyId != company.Id)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "You don't have permission to edit this job" });
                    }
                    TempData["ErrorMessage"] = "You don't have permission to edit this job";
                    return RedirectToAction(nameof(Index));
                }

                // Update job properties
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

                // Update job properties
                existingJob.Title = model.Title;
                existingJob.Slug = model.Title.ToSlug();
                existingJob.Description = model.Description;
                existingJob.Requirements = model.Responsibilities;
                existingJob.JobType = jobType;
                existingJob.ExperienceLevel = experienceLevel;
                existingJob.SalaryMin = model.MinSalary ?? 0;
                existingJob.SalaryMax = model.MaxSalary ?? 0;
                existingJob.Benefits = !string.IsNullOrEmpty(model.Benefits) ? model.Benefits : "Standard benefits package";
                existingJob.ExpiryDate = model.ExpirationDate.HasValue
                    ? DateTime.SpecifyKind(model.ExpirationDate.Value, DateTimeKind.Utc)
                    : DateTime.UtcNow.AddMonths(1);
                existingJob.UpdatedAt = DateTime.UtcNow;
                existingJob.Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);

                // Save changes
                await _jobRepository.UpdateAsync(existingJob);
                await _jobRepository.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        message = "Job updated successfully!",
                        redirectUrl = Url.Action("JobDetail", "Job", new { id = existingJob.Id, area = "Employer" })
                    });
                }

                TempData["SuccessMessage"] = "Job updated successfully!";
                return RedirectToAction("JobDetail", new { id = existingJob.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job");
                ModelState.AddModelError("", "An error occurred while updating the job: " + ex.Message);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to update job: " + ex.Message });
                }

                ViewData["EditMode"] = true;
                ViewData["JobId"] = jobId;
                return View("Create", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Auth", new { area = "Auth" });
                }

                var company = await _companyRepository.GetByOwnerIdAsync(user.Id);
                if (company == null)
                {
                    return RedirectToAction("SetupBasic", "Company", new { area = "Employer" });
                }

                var job = await _jobRepository.GetByIdAsync(id);
                if (job == null)
                {
                    TempData["ErrorMessage"] = "Job not found";
                    return RedirectToAction(nameof(Index));
                }

                if (job.CompanyId != company.Id)
                {
                    TempData["ErrorMessage"] = "You don't have permission to edit this job";
                    return RedirectToAction(nameof(Index));
                }

                // Parse metadata
                var metadataDict = new Dictionary<string, string>();
                try
                {
                    if (!string.IsNullOrEmpty(job.Metadata))
                    {
                        metadataDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(job.Metadata);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error parsing job metadata for job ID {id}");
                }

                // Convert job to view model
                var viewModel = new JobPostViewModel
                {
                    Title = job.Title,
                    JobRole = metadataDict.TryGetValue("JobRole", out var jobRole) ? jobRole : job.Title,
                    MinSalary = job.SalaryMin,
                    MaxSalary = job.SalaryMax,
                    SalaryType = metadataDict.TryGetValue("SalaryType", out var salaryType) ? salaryType : "Monthly",
                    Experience = job.ExperienceLevel.ToString(),
                    JobType = job.JobType.ToString(),
                    Vacancies = metadataDict.TryGetValue("Vacancies", out var vacancies) && int.TryParse(vacancies, out var vacanciesInt) ? vacanciesInt : 1,
                    ExpirationDate = job.ExpiryDate,
                    JobLevel = metadataDict.TryGetValue("JobLevel", out var jobLevel) ? jobLevel : "Entry",
                    ApplyMethod = metadataDict.TryGetValue("ApplyMethod", out var applyMethod) ? applyMethod : "Jobpilot",
                    Benefits = job.Benefits,
                    Description = job.Description,
                    Responsibilities = job.Requirements
                };

                ViewData["EditMode"] = true;
                ViewData["JobId"] = id;

                return View("Create", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving job for editing. Job ID: {id}");
                TempData["ErrorMessage"] = $"An error occurred while retrieving job: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExpireJob(int id)
        {
            try
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

                // Set expiry date to yesterday to expire the job immediately
                job.ExpiryDate = DateTime.UtcNow.AddDays(-1);
                job.IsActive = false;
                job.UpdatedAt = DateTime.UtcNow;

                await _jobRepository.UpdateAsync(job);
                await _jobRepository.SaveChangesAsync();

                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        message = "Job has been expired successfully!"
                    });
                }

                // Set success message in TempData
                TempData["SuccessMessage"] = "Job has been expired successfully!";

                // Redirect based on the referrer
                var referer = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(referer) && referer.Contains("/JobDetail/"))
                {
                    return RedirectToAction("JobDetail", new { id = job.Id });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error expiring job");

                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to expire job: " + ex.Message
                    });
                }

                TempData["ErrorMessage"] = "Failed to expire job: " + ex.Message;

                // Redirect based on the referrer
                var referer = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(referer) && referer.Contains("/JobDetail/"))
                {
                    return RedirectToAction("JobDetail", new { id = id });
                }

                return RedirectToAction(nameof(Index));
            }
        }
    }
}

