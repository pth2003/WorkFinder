using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;
using WorkFinder.Web.Models.ViewModels;
using WorkFinder.Web.Repositories;

namespace WorkFinder.Web.Controllers
{
    [Authorize(Roles = "JobSeeker")]
    [Route("Dashboard/JobAlerts")]
    public class JobAlertController : Controller
    {
        private readonly IJobAlertRepository _jobAlertRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<JobAlertController> _logger;

        public JobAlertController(
            IJobAlertRepository jobAlertRepository,
            ICategoryRepository categoryRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<JobAlertController> logger)
        {
            _jobAlertRepository = jobAlertRepository;
            _categoryRepository = categoryRepository;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var alerts = await _jobAlertRepository.GetJobAlertsByUserIdAsync(user.Id);
            return View(alerts);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryRepository.GetAllAsync();
            return View();
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobAlert alert)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                // Set user ID
                alert.UserId = user.Id;

                // Validate job type and experience level
                if (!string.IsNullOrWhiteSpace(Request.Form["JobTypeValue"]) &&
                    Enum.TryParse<JobType>(Request.Form["JobTypeValue"], out var jobType))
                {
                    alert.JobType = jobType;
                }

                if (!string.IsNullOrWhiteSpace(Request.Form["ExperienceLevelValue"]) &&
                    Enum.TryParse<ExperienceLevel>(Request.Form["ExperienceLevelValue"], out var expLevel))
                {
                    alert.ExperienceLevel = expLevel;
                }

                // Map IsDaily property
                alert.IsDaily = alert.IsDaily;

                // Set created time
                alert.CreatedAt = DateTime.UtcNow;

                // Save the alert
                var result = await _jobAlertRepository.CreateJobAlertAsync(alert);
                if (result)
                {
                    TempData["SuccessMessage"] = "Job alert created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = "Failed to create job alert.";
                ViewBag.Categories = await _categoryRepository.GetAllAsync();
                return View(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job alert");
                TempData["ErrorMessage"] = "An error occurred while creating the job alert.";
                ViewBag.Categories = await _categoryRepository.GetAllAsync();
                return View(alert);
            }
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var alert = await _jobAlertRepository.GetJobAlertByIdAsync(id);
                if (alert == null)
                {
                    return NotFound();
                }

                // Ensure the alert belongs to the current user
                if (alert.UserId != user.Id)
                {
                    return Forbid();
                }

                ViewBag.Categories = await _categoryRepository.GetAllAsync();
                return View(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job alert for editing");
                TempData["ErrorMessage"] = "An error occurred while loading the job alert.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JobAlert alert)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var existingAlert = await _jobAlertRepository.GetJobAlertByIdAsync(id);
                if (existingAlert == null)
                {
                    return NotFound();
                }

                // Ensure the alert belongs to the current user
                if (existingAlert.UserId != user.Id)
                {
                    return Forbid();
                }

                // Update properties
                existingAlert.Keywords = alert.Keywords;
                existingAlert.Location = alert.Location;
                existingAlert.CategoryId = alert.CategoryId;
                existingAlert.MinSalary = alert.MinSalary;
                existingAlert.MaxSalary = alert.MaxSalary;
                existingAlert.IsDaily = alert.IsDaily;
                existingAlert.IsActive = alert.IsActive;

                // Validate job type and experience level
                if (!string.IsNullOrWhiteSpace(Request.Form["JobTypeValue"]) &&
                    Enum.TryParse<JobType>(Request.Form["JobTypeValue"], out var jobType))
                {
                    existingAlert.JobType = jobType;
                }
                else
                {
                    existingAlert.JobType = null;
                }

                if (!string.IsNullOrWhiteSpace(Request.Form["ExperienceLevelValue"]) &&
                    Enum.TryParse<ExperienceLevel>(Request.Form["ExperienceLevelValue"], out var expLevel))
                {
                    existingAlert.ExperienceLevel = expLevel;
                }
                else
                {
                    existingAlert.ExperienceLevel = null;
                }

                existingAlert.UpdatedAt = DateTime.UtcNow;

                // Save the changes
                var result = await _jobAlertRepository.UpdateJobAlertAsync(existingAlert);
                if (result)
                {
                    TempData["SuccessMessage"] = "Job alert updated successfully!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = "Failed to update job alert.";
                ViewBag.Categories = await _categoryRepository.GetAllAsync();
                return View(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job alert");
                TempData["ErrorMessage"] = "An error occurred while updating the job alert.";
                ViewBag.Categories = await _categoryRepository.GetAllAsync();
                return View(alert);
            }
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var alert = await _jobAlertRepository.GetJobAlertByIdAsync(id);
                if (alert == null)
                {
                    return NotFound();
                }

                // Ensure the alert belongs to the current user
                if (alert.UserId != user.Id)
                {
                    return Forbid();
                }

                // Delete the alert
                var result = await _jobAlertRepository.DeleteJobAlertAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Job alert deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete job alert.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job alert");
                TempData["ErrorMessage"] = "An error occurred while deleting the job alert.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("ToggleStatus/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var alert = await _jobAlertRepository.GetJobAlertByIdAsync(id);
                if (alert == null)
                {
                    return NotFound();
                }

                // Ensure the alert belongs to the current user
                if (alert.UserId != user.Id)
                {
                    return Forbid();
                }

                // Toggle status
                alert.IsActive = !alert.IsActive;
                alert.UpdatedAt = DateTime.UtcNow;

                // Save the changes
                var result = await _jobAlertRepository.UpdateJobAlertAsync(alert);
                if (result)
                {
                    TempData["SuccessMessage"] = alert.IsActive ?
                        "Job alert activated successfully!" :
                        "Job alert paused successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update job alert status.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling job alert status");
                TempData["ErrorMessage"] = "An error occurred while updating the job alert status.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}