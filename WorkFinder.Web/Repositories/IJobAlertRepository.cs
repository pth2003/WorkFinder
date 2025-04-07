using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;

namespace WorkFinder.Web.Repositories
{
    public interface IJobAlertRepository : IRepository<JobAlert>
    {
        Task<IEnumerable<JobAlert>> GetJobAlertsByUserIdAsync(int userId);
        Task<JobAlert> GetJobAlertByIdAsync(int id);
        Task<bool> CreateJobAlertAsync(JobAlert jobAlert);
        Task<bool> UpdateJobAlertAsync(JobAlert jobAlert);
        Task<bool> DeleteJobAlertAsync(int id);

        // Method to get active alerts that need to be processed
        Task<IEnumerable<JobAlert>> GetActiveAlertsForProcessingAsync(bool dailyOnly = false);

        // Method to update the LastSentAt date after processing
        Task UpdateAlertLastSentTimeAsync(int alertId);
    }
}