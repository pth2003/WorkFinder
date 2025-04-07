using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Data;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;

namespace WorkFinder.Web.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly WorkFinderContext _context;

        public NotificationRepository(WorkFinderContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(string userId, int limit = 20)
        {
            int userIdInt = Int32.Parse(userId);
            return await _context.Notifications
                .Where(n => n.UserId == userIdInt)
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetUnreadNotificationCountByUserIdAsync(string userId)
        {
            int userIdInt = Int32.Parse(userId);
            return await _context.Notifications
                .Where(n => n.UserId == userIdInt && !n.IsRead)
                .CountAsync();
        }

        public async Task<Notification> GetNotificationByIdAsync(int id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task<bool> AddNotificationAsync(Notification notification)
        {
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;

            _context.Notifications.Add(notification);
            return await SaveChangesAsync();
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return false;

            notification.IsRead = true;
            _context.Notifications.Update(notification);
            return await SaveChangesAsync();
        }

        public async Task<bool> MarkAllAsReadForUserAsync(string userId)
        {
            int userIdInt = Int32.Parse(userId);
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userIdInt && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            return await SaveChangesAsync();
        }

        public async Task<bool> DeleteNotificationAsync(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return false;

            _context.Notifications.Remove(notification);
            return await SaveChangesAsync();
        }

        public async Task<bool> DeleteAllForUserAsync(string userId)
        {
            int userIdInt = Int32.Parse(userId);
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userIdInt)
                .ToListAsync();

            _context.Notifications.RemoveRange(notifications);
            return await SaveChangesAsync();
        }

        public async Task<bool> CreateJobAlertNotificationAsync(string userId, string jobTitle, string location, int matchCount)
        {
            int userIdInt = Int32.Parse(userId);

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Title.Contains(jobTitle) && j.Location.Contains(location));

            var notification = new Notification
            {
                UserId = userIdInt,
                Title = "New Job Matches",
                Message = $"We found {matchCount} new jobs matching '{jobTitle}' in {location}",
                Type = NotificationType.JobAlert,
                JobId = job?.Id,
                Link = job != null
                    ? $"/Job/Details/{job.Id}"
                    : $"/Job?keyword={Uri.EscapeDataString(jobTitle)}&location={Uri.EscapeDataString(location)}",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            return await SaveChangesAsync();
        }

        private async Task<bool> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}