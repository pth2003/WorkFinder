using WorkFinder.Web.Models.Enums;

namespace WorkFinder.Web.Models;

public class Job : BaseEntity
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Requirements { get; set; }
    public string Benefits { get; set; }
    public string Location { get; set; }
    public decimal SalaryMin { get; set; }
    public decimal SalaryMax { get; set; }
    public JobType JobType { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public string Slug { get; set; }

    // Thêm trường metadata để lưu trữ dữ liệu bổ sung có thể được serialize/deserialize
    public string Metadata { get; set; } = "{}";

    public int CompanyId { get; set; }
    public virtual Company Company { get; set; } = null!;
    public virtual ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    public virtual ICollection<SavedJob> SavedByUsers { get; set; } = new List<SavedJob>();
    public virtual ICollection<JobCategory> Categories { get; set; } = new List<JobCategory>();
}