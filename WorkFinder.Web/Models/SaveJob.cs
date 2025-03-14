namespace WorkFinder.Web.Models;

public class SavedJob : BaseEntity
{
    public DateTime SavedDate { get; set; }
        
    // Navigation properties
    public int JobId { get; set; }
    public virtual Job Job { get; set; }
    public int UserId { get; set; }
    public virtual ApplicationUser User { get; set; }
}