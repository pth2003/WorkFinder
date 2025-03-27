using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WorkFinder.Web.Areas.Employer.Models;
using WorkFinder.Web.Models;
using WorkFinder.Web.Repositories;

namespace WorkFinder.Web.Areas.Employer.Controllers
{
    [Area("Employer")]
    [Authorize(Roles = "Employer")]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJobRepository _jobRepository;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            ICompanyRepository companyRepository,
            IJobRepository jobRepository)
        {
            _userManager = userManager;
            _companyRepository = companyRepository;
            _jobRepository = jobRepository;
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

            var viewModel = new EmployerDashboardViewModel
            {
                CompanyName = company.Name,
                CompanyLogo = company.Logo
            };

            // Trong trường hợp trang load lần đầu, trả về view với model cơ bản
            // Chi tiết sẽ được load bằng AJAX sau
            return View(viewModel);
        }

        // AJAX Endpoint để lấy thông tin dashboard
        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
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

                var totalJobs = await _jobRepository.GetTotalJobsByCompanyIdAsync(company.Id);
                var activeJobs = await _jobRepository.GetActiveJobCountByCompanyIdAsync(company.Id);
                var totalApplications = await _jobRepository.GetTotalApplicationsByCompanyIdAsync(company.Id);

                // Luôn trả về JSON, dù là AJAX hay không
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        totalJobs,
                        activeJobs,
                        totalApplications,
                        companyName = company.Name,
                        companyLogo = company.Logo
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // AJAX Endpoint để lấy danh sách công việc gần đây
        [HttpGet]
        public async Task<IActionResult> GetRecentJobs()
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

                var listRecentJobs = await _jobRepository.GetRecentJobsByCompanyIdAsync(company.Id, 5);
                var recentJobs = listRecentJobs.Select(j => new RecentJobViewModel
                {
                    Id = j.Id,
                    Title = j.Title,
                    JobType = j.JobType.ToString(),
                    IsActive = j.IsActive,
                    DaysRemaining = (int)Math.Ceiling((j.ExpiryDate - DateTime.Now).TotalDays),
                    ApplicationCount = j.Applications.Count,
                    ExpirationDate = j.ExpiryDate.ToString("MMM d, yyyy")
                }).ToList();

                // Luôn trả về JSON, dù là AJAX hay không
                return Json(new
                {
                    success = true,
                    data = recentJobs
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // AJAX Endpoint để lấy danh sách ứng viên gần đây
        [HttpGet]
        public async Task<IActionResult> GetRecentApplications()
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

                // Logic lấy danh sách ứng viên gần đây
                // (Giả sử bạn có một repository method cho việc này)
                // var recentApplications = await _applicationRepository.GetRecentApplicationsByCompanyIdAsync(company.Id, 5);

                // Luôn trả về JSON, dù là AJAX hay không
                return Json(new
                {
                    success = true,
                    data = Array.Empty<object>() // recentApplications
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}