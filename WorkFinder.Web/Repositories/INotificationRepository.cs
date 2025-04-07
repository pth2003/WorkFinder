using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;

namespace WorkFinder.Web.Repositories
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(string userId, int limit = 20);
        Task<int> GetUnreadNotificationCountByUserIdAsync(string userId);
        Task<Notification> GetNotificationByIdAsync(int id);
        Task<bool> AddNotificationAsync(Notification notification);
        Task<bool> MarkAsReadAsync(int id);
        Task<bool> MarkAllAsReadForUserAsync(string userId);
        Task<bool> DeleteNotificationAsync(int id);
        Task<bool> DeleteAllForUserAsync(string userId);
        Task<bool> CreateJobAlertNotificationAsync(string userId, string jobTitle, string location, int matchCount);
    }
}