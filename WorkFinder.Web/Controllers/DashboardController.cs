using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WorkFinder.Web.Areas.Auth.Services;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;
using WorkFinder.Web.Models.ViewModels;
using WorkFinder.Web.Repositories;

namespace WorkFinder.Web.Controllers
{
    [Authorize(Roles = UserRoles.JobSeeker)]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IAccountService _accountService;
        private readonly IJobRepository _jobRepository;
        private readonly ISaveJobRepository _saveJobRepository;
        private readonly IAuthService _authService;
        private readonly IJobAlertRepository _jobAlertRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IResumeRepository _resumeRepository;

        public DashboardController(
            ILogger<DashboardController> logger,
            IAccountService accountService,
            IJobRepository jobRepository,
            ISaveJobRepository saveJobRepository,
            IAuthService authService,
            IJobAlertRepository jobAlertRepository,
            ICategoryRepository categoryRepository,
            INotificationRepository notificationRepository,
            IResumeRepository resumeRepository)
        {
            _logger = logger;
            _accountService = accountService;
            _jobRepository = jobRepository;
            _saveJobRepository = saveJobRepository;
            _authService = authService;
            _jobAlertRepository = jobAlertRepository;
            _categoryRepository = categoryRepository;
            _notificationRepository = notificationRepository;
            _resumeRepository = resumeRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var profile = await _accountService.GetProfileAsync();
                var user = await _authService.GetCurrentUserAsync();

                // Get actual counts
                var savedJobs = await _saveJobRepository.GetSavedJobsByUserIdAsync(user.Id);
                var savedJobsCount = savedJobs.Count();

                // Get actual applied jobs count
                var appliedJobsCount = (await _jobRepository.GetJobApplicationsByUserIdAsync(user.Id, 1, 1000)).Count;

                // Get user's resume
                var resume = await _resumeRepository.GetResumeByUserIdAsync(user.Id);

                var dashboardViewModel = new DashboardViewModel
                {
                    UserName = $"{profile.FirstName} {profile.LastName}",
                    ProfilePicture = profile.CurrentProfilePicture,
                    AppliedJobsCount = appliedJobsCount,
                    FavoriteJobsCount = savedJobsCount,
                    JobAlertsCount = 2, // Sample count based on the job alerts
                    Resume = resume,
                    RecentlyAppliedJobs = new List<AppliedJobViewModel>() // Will be populated below
                };

                // Add recently applied jobs from the database
                var recentApplications = await _jobRepository.GetJobApplicationsByUserIdAsync(user.Id, 1, 4);
                var appliedJobs = recentApplications.Select(app => new AppliedJobViewModel
                {
                    JobId = app.JobId,
                    JobTitle = app.Job.Title,
                    JobType = app.Job.JobType.ToString(),
                    Location = app.Job.Location,
                    Salary = GetFormattedSalary(app.Job.SalaryMin, app.Job.SalaryMax),
                    CompanyName = app.Job.Company?.Name ?? "Unknown Company",
                    CompanyLogo = app.Job.Company?.Logo ?? "/images/default-company.png",
                    DateApplied = app.AppliedDate,
                    Status = app.Status.ToString()
                }).ToList();

                dashboardViewModel.RecentlyAppliedJobs = appliedJobs;

                return View(dashboardViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Dashboard Index");
                TempData["ErrorMessage"] = "Không thể tải dữ liệu Dashboard.";
                return View(new DashboardViewModel());
            }
        }

        public async Task<IActionResult> Applied(int page = 1)
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    return RedirectToAction("Login", "Auth", new { area = "Auth" });
                }

                int pageSize = 10;
                var applications = await _jobRepository.GetJobApplicationsByUserIdAsync(user.Id, page, pageSize);

                var appliedJobs = applications.Select(app => new AppliedJobViewModel
                {
                    JobId = app.JobId,
                    JobTitle = app.Job.Title,
                    JobType = app.Job.JobType.ToString(),
                    Location = app.Job.Location,
                    Salary = GetFormattedSalary(app.Job.SalaryMin, app.Job.SalaryMax),
                    CompanyName = app.Job.Company?.Name ?? "Unknown Company",
                    CompanyLogo = app.Job.Company?.Logo ?? "/images/default-company.png",
                    DateApplied = app.AppliedDate,
                    Status = app.Status.ToString()
                }).ToList();

                // Get total applications count for pagination
                var totalApplications = (await _jobRepository.GetJobApplicationsByUserIdAsync(user.Id, 1, 1000)).Count;
                int totalPages = (int)Math.Ceiling(totalApplications / (double)pageSize);

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalApplications;

                return View(appliedJobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Dashboard Applied");
                TempData["ErrorMessage"] = "Không thể tải danh sách công việc đã ứng tuyển.";
                return View(new List<AppliedJobViewModel>());
            }
        }

        public async Task<IActionResult> Favorites(int page = 1)
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    return RedirectToAction("Login", "Auth", new { area = "Auth" });
                }

                // Lấy danh sách saved jobs của user
                var savedJobs = await _saveJobRepository.GetSavedJobsByUserIdAsync(user.Id);

                // Tạo danh sách favorite jobs
                var allFavoriteJobs = new List<FavoriteJobViewModel>();

                foreach (var savedJob in savedJobs)
                {
                    // Lấy thông tin đầy đủ của job từ repository
                    var job = await _jobRepository.GetJobWithDetailsAsync(savedJob.JobId);

                    if (job != null)
                    {
                        var companyLogo = job.Company?.Logo ?? "/images/default-company.png";

                        allFavoriteJobs.Add(new FavoriteJobViewModel
                        {
                            JobId = job.Id,
                            JobTitle = job.Title,
                            JobType = job.JobType.ToString(),
                            Location = job.Location,
                            Salary = GetFormattedSalary(job.SalaryMin, job.SalaryMax),
                            CompanyName = job.Company?.Name ?? "Unknown Company",
                            CompanyLogo = companyLogo,
                            DateAdded = savedJob.SavedDate
                        });
                    }
                }

                // Phân trang
                int pageSize = 5; // 5 jobs mỗi trang
                int totalItems = allFavoriteJobs.Count;
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                var favoriteJobs = allFavoriteJobs
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Truyền thông tin phân trang vào ViewBag
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;

                return View(favoriteJobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Dashboard Favorites");
                TempData["ErrorMessage"] = "Không thể tải danh sách công việc đã lưu.";
                return View(new List<FavoriteJobViewModel>());
            }
        }

        // Helper method to format salary
        private string GetFormattedSalary(decimal? min, decimal? max)
        {
            if (min.HasValue && max.HasValue)
            {
                return $"${min.Value.ToString("#,##0")}-{max.Value.ToString("#,##0")}/year";
            }
            else if (min.HasValue)
            {
                return $"From ${min.Value.ToString("#,##0")}/year";
            }
            else if (max.HasValue)
            {
                return $"Up to ${max.Value.ToString("#,##0")}/year";
            }

            return "Salary not specified";
        }

        [HttpGet("jobAlerts")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Alerts()
        {
            return RedirectToAction("Index", "JobAlert");
        }

        // Helper method to get sample applied jobs (can be removed later)
        private List<AppliedJobViewModel> GetAppliedJobs()
        {
            return new List<AppliedJobViewModel>
            {
                new AppliedJobViewModel
                {
                    JobId = 1,
                    JobTitle = "Networking Engineer",
                    JobType = "Remote",
                    Location = "Washington",
                    Salary = "$50k-80k/month",
                    CompanyName = "Microsoft",
                    DateApplied = DateTime.Now.AddDays(-5),
                    Status = "Active"
                },
                new AppliedJobViewModel
                {
                    JobId = 2,
                    JobTitle = "Product Designer",
                    JobType = "Full Time",
                    Location = "Dhaka",
                    Salary = "$50k-80k/month",
                    CompanyName = "Google",
                    DateApplied = DateTime.Now.AddDays(-10),
                    Status = "Active"
                },
                new AppliedJobViewModel
                {
                    JobId = 3,
                    JobTitle = "Junior Graphic Designer",
                    JobType = "Temporary",
                    Location = "Brazil",
                    Salary = "$50k-80k/month",
                    CompanyName = "Amazon",
                    DateApplied = DateTime.Now.AddDays(-15),
                    Status = "Pending"
                },
                new AppliedJobViewModel
                {
                    JobId = 4,
                    JobTitle = "Visual Designer",
                    JobType = "Contract Base",
                    Location = "Wisconsin",
                    Salary = "$50k-80k/month",
                    CompanyName = "Adobe",
                    DateApplied = DateTime.Now.AddDays(-20),
                    Status = "Rejected"
                },
                new AppliedJobViewModel
                {
                    JobId = 5,
                    JobTitle = "Software Engineer",
                    JobType = "Full Time",
                    Location = "Boston",
                    Salary = "$80k-120k/year",
                    CompanyName = "Apple",
                    DateApplied = DateTime.Now.AddDays(-25),
                    Status = "Active"
                }
            };
        }

        public async Task<IActionResult> Notifications()
        {
            try
            {
                var user = await _authService.GetCurrentUserAsync();
                if (user == null)
                {
                    return RedirectToAction("Login", "Auth", new { area = "Auth" });
                }

                // Get notifications for the current user
                var notifications = await _notificationRepository.GetNotificationsByUserIdAsync(user.Id.ToString(), 50);
                return View(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notifications");
                TempData["ErrorMessage"] = "Could not load notifications.";
                return View(new List<Notification>());
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}