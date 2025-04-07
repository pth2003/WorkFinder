using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using WorkFinder.Web.Services;

namespace WorkFinder.Web.BackgroundServices
{
    public class JobAlertBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobAlertBackgroundService> _logger;

        // Run daily by default
        private readonly TimeSpan _period = TimeSpan.FromMinutes(5);

        public JobAlertBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<JobAlertBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Job Alert Background Service is starting.");

            using PeriodicTimer timer = new PeriodicTimer(_period);

            try
            {
                // Run immediately on startup
                await ProcessJobAlertsAsync();

                // Then run periodically
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await ProcessJobAlertsAsync();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Job Alert Background Service is stopping.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the Job Alert Background Service.");
                throw;
            }
        }

        private async Task ProcessJobAlertsAsync()
        {
            _logger.LogInformation("Processing job alerts at: {time}", DateTimeOffset.Now);

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var jobAlertService = scope.ServiceProvider.GetRequiredService<JobAlertService>();

                    // Process all alerts (both daily and weekly)
                    // For daily alerts, you can call jobAlertService.ProcessJobAlertsAsync(true)
                    await jobAlertService.ProcessJobAlertsAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing job alerts");
            }
        }
    }
}