using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Data;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;

namespace WorkFinder.Web.Repositories
{
    public class JobAlertRepository : Repository<JobAlert>, IJobAlertRepository
    {
        private readonly WorkFinderContext _context;

        public JobAlertRepository(WorkFinderContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobAlert>> GetJobAlertsByUserIdAsync(int userId)
        {
            return await _context.JobAlerts
                .Where(ja => ja.UserId == userId)
                .Include(ja => ja.Category)
                .OrderByDescending(ja => ja.CreatedAt)
                .ToListAsync();
        }

        public async Task<JobAlert> GetJobAlertByIdAsync(int id)
        {
            return await _context.JobAlerts
                .Include(ja => ja.Category)
                .FirstOrDefaultAsync(ja => ja.Id == id);
        }

        public async Task<bool> CreateJobAlertAsync(JobAlert jobAlert)
        {
            await _context.JobAlerts.AddAsync(jobAlert);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateJobAlertAsync(JobAlert jobAlert)
        {
            _context.JobAlerts.Update(jobAlert);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteJobAlertAsync(int id)
        {
            var jobAlert = await _context.JobAlerts.FindAsync(id);
            if (jobAlert == null) return false;

            _context.JobAlerts.Remove(jobAlert);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<JobAlert>> GetActiveAlertsForProcessingAsync(bool dailyOnly = false)
        {
            var query = _context.JobAlerts
                .Where(ja => ja.IsActive);

            if (dailyOnly)
            {
                query = query.Where(ja => ja.IsDaily);
            }

            // Only process alerts that haven't been sent in the last 24 hours for daily alerts
            // or in the last 7 days for weekly alerts
            var oneDayAgo = DateTime.UtcNow.AddDays(-1);
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            query = query.Where(ja =>
                ja.LastSentAt == null || // Include alerts that have never been sent
                (ja.IsDaily && ja.LastSentAt < oneDayAgo) ||
                (!ja.IsDaily && ja.LastSentAt < sevenDaysAgo));

            return await query
                .Include(ja => ja.User)
                .Include(ja => ja.Category)
                .ToListAsync();
        }

        public async Task UpdateAlertLastSentTimeAsync(int alertId)
        {
            var alert = await _context.JobAlerts.FindAsync(alertId);
            if (alert != null)
            {
                alert.LastSentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}