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
                return RedirectToAction("SetupBasic", "Company");
            }

            // Lấy dữ liệu cho dashboard
            var openJobs = await _jobRepository.GetActiveJobCountByCompanyIdAsync(company.Id);
            var savedCandidates = 2517; // Giả lập dữ liệu hoặc lấy từ repository

            // Tạo dữ liệu giả cho demo
            var recentJobs = new List<RecentJobViewModel>
            {
                new RecentJobViewModel
                {
                    Id = 1,
                    Title = "UI/UX Designer",
                    JobType = "Full Time",
                    IsActive = true,
                    DaysRemaining = 27,
                    ApplicationCount = 798,
                    ExpirationDate = DateTime.Now.AddDays(27).ToString("MMM d, yyyy")
                },
                new RecentJobViewModel
                {
                    Id = 2,
                    Title = "Senior UX Designer",
                    JobType = "Internship",
                    IsActive = true,
                    DaysRemaining = 8,
                    ApplicationCount = 185,
                    ExpirationDate = DateTime.Now.AddDays(8).ToString("MMM d, yyyy")
                },
                new RecentJobViewModel
                {
                    Id = 3,
                    Title = "Technical Support Specialist",
                    JobType = "Part Time",
                    IsActive = true,
                    DaysRemaining = 4,
                    ApplicationCount = 556,
                    ExpirationDate = DateTime.Now.AddDays(4).ToString("MMM d, yyyy")
                },
                new RecentJobViewModel
                {
                    Id = 4,
                    Title = "Junior Graphic Designer",
                    JobType = "Full Time",
                    IsActive = true,
                    DaysRemaining = 24,
                    ApplicationCount = 583,
                    ExpirationDate = DateTime.Now.AddDays(24).ToString("MMM d, yyyy")
                },
                new RecentJobViewModel
                {
                    Id = 5,
                    Title = "Front End Developer",
                    JobType = "Full Time",
                    IsActive = false,
                    DaysRemaining = 0,
                    ApplicationCount = 740,
                    ExpirationDate = "Dec 7, 2019"
                }
            };

            var viewModel = new EmployerDashboardViewModel
            {
                CompanyName = company.Name,
                CompanyLogo = company.Logo,
                OpenJobs = openJobs,
                SavedCandidates = savedCandidates,
                RecentJobs = recentJobs
            };

            return View(viewModel);
        }
    }
}