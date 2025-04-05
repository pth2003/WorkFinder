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

        public DashboardController(
            ILogger<DashboardController> logger,
            IAccountService accountService,
            IJobRepository jobRepository,
            ISaveJobRepository saveJobRepository,
            IAuthService authService)
        {
            _logger = logger;
            _accountService = accountService;
            _jobRepository = jobRepository;
            _saveJobRepository = saveJobRepository;
            _authService = authService;
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

                var dashboardViewModel = new DashboardViewModel
                {
                    UserName = $"{profile.FirstName} {profile.LastName}",
                    ProfilePicture = profile.CurrentProfilePicture,
                    AppliedJobsCount = 5, // Sample count
                    FavoriteJobsCount = savedJobsCount,
                    JobAlertsCount = 2, // Sample count based on the job alerts
                    RecentlyAppliedJobs = new List<AppliedJobViewModel>() // Will be populated below
                };

                // Add recently applied jobs (first 4 from the Applied method)
                var allAppliedJobs = GetAppliedJobs();
                dashboardViewModel.RecentlyAppliedJobs = allAppliedJobs.Take(4).ToList();

                return View(dashboardViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Dashboard Index");
                TempData["ErrorMessage"] = "Không thể tải dữ liệu Dashboard.";
                return View(new DashboardViewModel());
            }
        }

        public IActionResult Applied()
        {
            return View(GetAppliedJobs());
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

        public IActionResult Alerts()
        {
            // Sample data for job alerts
            var jobAlerts = new List<JobAlertViewModel>
            {
                new JobAlertViewModel
                {
                    Id = 1,
                    Name = "Web Developer Jobs",
                    JobTitle = "Web Developer",
                    Location = "Remote",
                    SalaryRange = "$60,000 - $80,000",
                    JobType = "Full Time",
                    IsActive = true,
                    Frequency = "Daily",
                    LastUpdated = DateTime.Now.AddDays(-2)
                },
                new JobAlertViewModel
                {
                    Id = 2,
                    Name = "UI/UX Design Jobs",
                    JobTitle = "UI/UX Designer",
                    Location = "New York",
                    SalaryRange = "$70,000 - $90,000",
                    JobType = "Remote",
                    IsActive = false,
                    Frequency = "Weekly",
                    LastUpdated = DateTime.Now.AddDays(-7)
                }
            };

            return View(jobAlerts);
        }

        // Helper method to get sample applied jobs
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}