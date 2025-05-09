using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;
using WorkFinder.Web.Repositories;
using WorkFinder.Web.Areas.Employer.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace WorkFinder.Web.Areas.Employer.Controllers
{
    [Area("Employer")]
    [Authorize(Roles = "Employer")]
    public class ApplicationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJobRepository _jobRepository;
        private readonly IResumeRepository _resumeRepository;
        private readonly ILogger<ApplicationController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ApplicationController(
            UserManager<ApplicationUser> userManager,
            ICompanyRepository companyRepository,
            IJobRepository jobRepository,
            IResumeRepository resumeRepository,
            ILogger<ApplicationController> logger,
            IServiceProvider serviceProvider)
        {
            _userManager = userManager;
            _companyRepository = companyRepository;
            _jobRepository = jobRepository;
            _resumeRepository = resumeRepository;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int jobId)
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
                    return RedirectToAction("Index", "Home", new { area = "" });
                }

                var job = await _jobRepository.GetJobWithApplicationsAsync(jobId);
                if (job == null || job.CompanyId != company.Id)
                {
                    TempData["ErrorMessage"] = "Job not found or you don't have permission to view applications for this job.";
                    return RedirectToAction("Index", "Job", new { area = "Employer" });
                }

                var applications = job.Applications.ToList();

                // Lấy tất cả user IDs từ applications
                var userIds = applications.Select(a => a.Applicant.Id).Distinct().ToList();

                // Tạo dictionary để lưu trữ resumes theo user ID
                var userResumes = new Dictionary<int, Resume>();

                // Lấy tất cả resumes cho một lần truy vấn
                if (userIds.Any())
                {
                    userResumes = await _resumeRepository.GetResumesByUserIdsAsync(userIds);
                }

                var applicationsViewModel = new JobApplicationsViewModel
                {
                    Job = job,
                    Applications = applications,
                    UserResumes = userResumes
                };

                return View(applicationsViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving job applications");
                TempData["ErrorMessage"] = "An error occurred while retrieving job applications.";
                return RedirectToAction("Index", "Job", new { area = "Employer" });
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

                var oldStatus = application.Status;
                var newStatusEnum = (ApplicationStatus)newStatus;

                if (oldStatus == newStatusEnum)
                {
                    return RedirectToAction("Index", new { jobId = job.Id });
                }

                // Update the status
                application.Status = newStatusEnum;

                await _jobRepository.UpdateApplicationStatusAsync(application.Id, (int)newStatusEnum);
                await _jobRepository.SaveChangesAsync();

                // Send notification to the applicant about status change
                try
                {
                    var notificationRepo = _serviceProvider.GetService<INotificationRepository>();
                    if (notificationRepo != null)
                    {
                        // Tạo thông báo cho ứng viên
                        string message = GetStatusChangeMessage(application.JobId, newStatusEnum);
                        await notificationRepo.AddNotificationAsync(new Notification
                        {
                            UserId = application.ApplicantId,
                            Title = "Application Status Updated",
                            Message = message,
                            Type = NotificationType.ApplicationStatusChanged,
                            JobApplicationId = application.Id,
                            JobId = application.JobId,
                            Link = $"/Dashboard/Applied"
                        });
                    }
                }
                catch (Exception notifEx)
                {
                    // Ghi log lỗi nhưng không dừng luồng chính
                    _logger.LogError(notifEx, "Error sending notification for application status change");
                }

                TempData["SuccessMessage"] = $"Application status updated to {newStatusEnum}";
                return RedirectToAction("Index", new { jobId = job.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating application status");
                TempData["ErrorMessage"] = "An error occurred while updating the application status.";
                return RedirectToAction("Index", "Job", new { area = "Employer" });
            }
        }

        // Phương thức hỗ trợ để tạo thông báo về thay đổi trạng thái
        private string GetStatusChangeMessage(int jobId, ApplicationStatus status)
        {
            return status switch
            {
                ApplicationStatus.Reviewing => "Your application is now being reviewed by the employer.",
                ApplicationStatus.Interview => "Congratulations! You've been selected for an interview.",
                ApplicationStatus.Accepted => "Congratulations! Your application has been accepted.",
                ApplicationStatus.Rejected => "We're sorry, your application has not been selected at this time.",
                _ => $"Your application status has been updated to {status}."
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetCandidateDetailPartial(int applicationId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return PartialView("_Error", "User not found");
                }

                var company = await _companyRepository.GetByOwnerIdAsync(user.Id);
                if (company == null)
                {
                    return PartialView("_Error", "Company not found");
                }

                // Find the application
                var job = await _jobRepository.GetJobByApplicationIdAsync(applicationId);

                if (job == null)
                {
                    return PartialView("_Error", "Job not found");
                }

                if (job.CompanyId != company.Id)
                {
                    return PartialView("_Error", "You don't have permission to view this candidate");
                }

                var application = job.Applications.FirstOrDefault(a => a.Id == applicationId);

                if (application == null)
                {
                    return PartialView("_Error", "Application not found");
                }

                var applicant = application.Applicant;

                // Get the resume if available
                Resume resume = null;
                try
                {
                    resume = await _resumeRepository.GetResumeByUserIdAsync(applicant.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error getting resume for applicant {applicant.Id}");
                    // Continue without resume data
                }

                // Create ViewModel with full information
                var candidateDetail = new CandidateDetailViewModel
                {
                    // Info from ApplicationUser
                    Id = applicant.Id,
                    FirstName = applicant.FirstName,
                    LastName = applicant.LastName,
                    Email = applicant.Email,
                    PhoneNumber = applicant.PhoneNumber ?? "Not specified",
                    ProfilePicture = applicant.ProfilePicture ?? "/images/avatar-placeholder.jpg",
                    DateOfBirth = applicant.DateOfBirth,

                    // Info from Resume
                    Title = resume?.Skills?.Split(',').FirstOrDefault() ?? "Job Seeker",
                    Summary = resume?.Summary ?? "No biography available.",
                    Skills = resume?.Skills ?? "Not specified",
                    Education = resume?.Education ?? "Not specified",
                    Experience = resume?.Experience ?? "Not specified",
                    Certifications = resume?.Certifications ?? "Not specified",
                    Languages = resume?.Languages ?? "Not specified",
                    ResumeFileUrl = resume?.FileUrl,

                    // Info from JobApplication
                    ApplicationId = application.Id,
                    CoverLetter = application.CoverLetter ?? "No cover letter provided.",
                    Status = application.Status,
                    AppliedDate = application.AppliedDate,

                    // Additional info
                    JobId = job.Id,
                    JobTitle = job.Title
                };

                return PartialView("~/Areas/Employer/Views/Shared/_CandidateDetailModal.cshtml", candidateDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving candidate details for application {applicationId}");
                return PartialView("_Error", $"An error occurred: {ex.Message}");
            }
        }
    }
}