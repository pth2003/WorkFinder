using Microsoft.AspNetCore.Identity;
using WorkFinder.Web.Models.Enums;

namespace WorkFinder.Web.Models;

public class ApplicationUser : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string ProfilePicture { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }

    // Navigation properties
    public virtual Resume? Resume { get; set; }
    public virtual ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
    public virtual ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    public virtual Company? Company { get; set; }

    // New navigation properties
    public virtual ICollection<JobAlert> JobAlerts { get; set; } = new List<JobAlert>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}