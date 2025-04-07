using WorkFinder.Web.Models.Enums;

namespace WorkFinder.Web.Models;

public class Notification : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public NotificationType Type { get; set; } = NotificationType.SystemNotification;

    // Reference data for specific notification types
    public int? JobId { get; set; }
    public virtual Job? Job { get; set; }

    public int? JobApplicationId { get; set; }
    public virtual JobApplication? JobApplication { get; set; }

    // User who receives the notification
    public int UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;
}