using Microsoft.Extensions.Logging;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;
using WorkFinder.Web.Repositories;

namespace WorkFinder.Web.Services
{
    public class JobAlertService
    {
        private readonly IJobAlertRepository _jobAlertRepository;
        private readonly IJobRepository _jobRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<JobAlertService> _logger;

        public JobAlertService(
            IJobAlertRepository jobAlertRepository,
            IJobRepository jobRepository,
            INotificationRepository notificationRepository,
            ILogger<JobAlertService> logger)
        {
            _jobAlertRepository = jobAlertRepository;
            _jobRepository = jobRepository;
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        // Process job alerts (called by background job)
        public async Task ProcessJobAlertsAsync(bool dailyOnly = false)
        {
            _logger.LogInformation("Processing job alerts. Daily only: {DailyOnly}", dailyOnly);

            // Get all active alerts that need to be processed
            var alerts = await _jobAlertRepository.GetActiveAlertsForProcessingAsync(dailyOnly);
            _logger.LogInformation("Found {AlertCount} alerts to process", alerts.Count());

            // Need to determine when to start looking for jobs
            var lastProcessingTime = DateTime.UtcNow.AddDays(dailyOnly ? -1 : -7);

            foreach (var alert in alerts)
            {
                try
                {
                    await ProcessAlertAsync(alert, lastProcessingTime);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing alert {AlertId} for user {UserId}", alert.Id, alert.UserId);
                }
            }
        }

        private async Task ProcessAlertAsync(JobAlert alert, DateTime sinceTime)
        {
            // Convert alert criteria to job search parameters
            var matchingJobs = await _jobRepository.GetJobsAdvancedPagedAsync(
                keyword: alert.Keywords,
                location: alert.Location,
                categoryId: alert.CategoryId,
                jobType: alert.JobType,
                experienceLevel: alert.ExperienceLevel,
                minSalary: alert.MinSalary,
                maxSalary: alert.MaxSalary,
                jobLevel: alert.ExperienceLevel,
                postedAfter: sinceTime,
                page: 1,
                pageSize: 50  // Limit to avoid creating too many notifications
            );

            if (matchingJobs.Jobs.Any())
            {
                _logger.LogInformation("Found {JobCount} matching jobs for alert {AlertId}",
                    matchingJobs.Jobs.Count(), alert.Id);

                // Create notifications for each matching job
                foreach (var job in matchingJobs.Jobs)
                {
                    var success = await _notificationRepository.CreateJobAlertNotificationAsync(
                        alert.UserId.ToString(),
                        job.Title,
                        job.Location,
                        matchingJobs.Jobs.Count());

                    _logger.LogInformation("Created notification for alert {AlertId}, job {JobTitle}, success: {Success}",
                        alert.Id, job.Title, success);
                }

                // Update the last sent time
                await _jobAlertRepository.UpdateAlertLastSentTimeAsync(alert.Id);
                _logger.LogInformation("Updated LastSentAt for alert {AlertId} to {Time}", alert.Id, DateTime.UtcNow);
            }
            else
            {
                _logger.LogInformation("No matching jobs found for alert {AlertId}", alert.Id);

                // Still update the last sent time to avoid processing again until next cycle
                await _jobAlertRepository.UpdateAlertLastSentTimeAsync(alert.Id);
                _logger.LogInformation("Updated LastSentAt for alert {AlertId} to {Time}", alert.Id, DateTime.UtcNow);
            }
        }
    }
}