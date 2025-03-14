namespace WorkFinder.Web.Models;

public class Resume : BaseEntity
{
    
    public string Summary { get; set; }
    public string Skills { get; set; }
    public string Education { get; set; }
    public string Experience { get; set; }
    public string Certifications { get; set; }
    public string Languages { get; set; }
    public string FileUrl { get; set; }
    
    // Navigation properties
    public int UserId { get; set; }
    public virtual ApplicationUser User { get; set; }
}