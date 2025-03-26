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
            var savedCandidates = await _jobRepository.GetTotalApplicationsByCompanyIdAsync(company.Id);
            var listRecentJobs = await _jobRepository.GetRecentJobsByCompanyIdAsync(company.Id, 5);
            var recentJobs = listRecentJobs.Select(j => new RecentJobViewModel
            {
                Id = j.Id,
                Title = j.Title,
                JobType = j.JobType.ToString(),
                IsActive = j.IsActive,
                DaysRemaining = (int)Math.Ceiling((j.ExpiryDate - DateTime.Now).TotalDays),
                ApplicationCount = j.Applications?.Count ?? 0,
                ExpirationDate = j.ExpiryDate.ToString("MMM d, yyyy")
            });


            var viewModel = new EmployerDashboardViewModel
            {
                CompanyName = company.Name,
                CompanyLogo = company.Logo,
                OpenJobs = openJobs,
                SavedCandidates = savedCandidates,
                RecentJobs = recentJobs.ToList()
            };

            return View(viewModel);
        }
    }
}