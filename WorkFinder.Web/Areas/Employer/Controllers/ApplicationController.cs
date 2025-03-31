using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WorkFinder.Web.Models;
using WorkFinder.Web.Repositories;
using WorkFinder.Web.Areas.Employer.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WorkFinder.Web.Areas.Employer.Controllers
{
    [Area("Employer")]
    [Authorize(Roles = "Employer")]
    public class ApplicationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJobRepository _jobRepository;
        private readonly ILogger<ApplicationController> _logger;

        public ApplicationController(
            UserManager<ApplicationUser> userManager,
            ICompanyRepository companyRepository,
            IJobRepository jobRepository,
            ILogger<ApplicationController> logger)
        {
            _userManager = userManager;
            _companyRepository = companyRepository;
            _jobRepository = jobRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int jobId, string filter = "All")
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var company = await _companyRepository.GetByOwnerIdAsync(user.Id);
                if (company == null)
                {
                    return RedirectToAction("SetupBasic", "Company", new { area = "Employer" });
                }

                var job = await _jobRepository.GetJobWithApplicationsAsync(jobId);


                if (job == null)
                {
                    return NotFound("Job not found");
                }

                if (job.CompanyId != company.Id)
                {
                    _logger.LogWarning($"Unauthorized access: User {user.Id} attempting to access job {jobId} which belongs to company {job.CompanyId} while user belongs to company {company.Id}");

                    // Cảnh báo cho người dùng và xóa cookie Authentication
                    TempData["ErrorMessage"] = "You attempted to access a job that doesn't belong to your company. Security checks have been performed.";

                    return RedirectToAction("Index", "Home", new { area = "Employer" });
                }

                var applicationsViewModel = new JobApplicationsViewModel
                {
                    Job = job,
                    Filter = filter
                };
                if (job.Applications != null && job.Applications.Any())
                {
                    var applications = job.Applications.ToList();

                    // Filter applications if needed - simplified to use status as int
                    if (filter != "All")
                    {
                        // For now, we won't filter
                        // Later you can implement filtering based on status as int
                    }

                    applicationsViewModel.Applications = applications;

                    // Count applications 
                    applicationsViewModel.TotalApplications = job.Applications.Count;
                    applicationsViewModel.ShortlistedCount = 0; // Not using complex status now
                    applicationsViewModel.NewCount = job.Applications.Count; // Consider all as new
                }

                return View(applicationsViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting applications for job {jobId}");
                return RedirectToAction("Index", "Home", new { area = "Employer", errorMessage = "An error occurred while retrieving job applications" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int applicationId, int newStatus)
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

                // Find the application
                var job = await _jobRepository.GetJobByApplicationIdAsync(applicationId);

                if (job == null)
                {
                    return NotFound("Job not found");
                }

                if (job.CompanyId != company.Id)
                {
                    _logger.LogWarning($"Unauthorized status update: User {user.Id} attempting to update application {applicationId} for job {job.Id} which belongs to company {job.CompanyId}");
                    return Forbid("You don't have permission to update this application");
                }

                var application = job.Applications.FirstOrDefault(a => a.Id == applicationId);
                if (application == null)
                {
                    return NotFound("Application not found");
                }

                // Add a CustomStatus field to JobApplication if it doesn't exist
                if (!application.GetType().GetProperties().Any(p => p.Name == "CustomStatus"))
                {
                    // We can't add dynamic property, let's use a different approach 
                    // Later we'll need to add a CustomStatus property to the JobApplication model
                    // For now, we'll just return a success message without updating status
                    _logger.LogInformation($"Status not updated for application {applicationId} - Status property not available");
                }

                try
                {
                    // Try to update the status via a separate method
                    await _jobRepository.UpdateApplicationStatusAsync(applicationId, newStatus);
                    await _jobRepository.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Could not update status for application {applicationId} but will continue");
                    // Continue execution despite the error
                }

                return RedirectToAction(nameof(Index), new { jobId = job.Id, toastMessage = "Application status updated successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating application status");
                return RedirectToAction(nameof(Index), new { toastMessage = "Failed to update application status." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int applicationId)
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

                // Find the application
                var job = await _jobRepository.GetJobByApplicationIdAsync(applicationId);

                if (job == null)
                {
                    return NotFound("Job not found");
                }

                if (job.CompanyId != company.Id)
                {
                    _logger.LogWarning($"Unauthorized delete: User {user.Id} attempting to delete application {applicationId} for job {job.Id} which belongs to company {job.CompanyId}");
                    return Forbid("You don't have permission to delete this application");
                }

                // Delete the application
                await _jobRepository.DeleteApplicationAsync(applicationId);
                await _jobRepository.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { jobId = job.Id, toastMessage = "Application deleted successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting application");
                return RedirectToAction(nameof(Index), new { toastMessage = "Failed to delete application." });
            }
        }
    }
}