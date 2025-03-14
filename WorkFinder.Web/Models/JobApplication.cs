using WorkFinder.Web.Models.Enums;

namespace WorkFinder.Web.Models;

public class JobApplication : BaseEntity
{
    public string CoverLetter { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedDate { get; set; }
    
    public int JobId { get; set; }
    public virtual Job Job { get; set; } = null!;
    
    public int ApplicantId { get; set; }
    public virtual ApplicationUser Applicant { get; set; } = null!;
}