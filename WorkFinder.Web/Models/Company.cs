namespace WorkFinder.Web.Models;

public class Company : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Logo { get; set; }
    public string Website { get; set; }
    public string Location { get; set; }
    public int EmployeeCount { get; set; }
    public string Industry { get; set; }
    public bool IsVerified { get; set; }
    
    // Navigation properties
    public int OwnerId { get; set; }
    public virtual ApplicationUser Owner { get; set; } = null!;
    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}