using WorkFinder.Web.Models.Enums;

namespace WorkFinder.Web.Models;

public class JobAlert : BaseEntity
{
    public string Keywords { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public virtual Category? Category { get; set; }
    public JobType? JobType { get; set; }
    public ExperienceLevel? ExperienceLevel { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }

    // Alert frequency settings
    public bool IsDaily { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastSentAt { get; set; } = null;

    // User who created the alert
    public int UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;
}