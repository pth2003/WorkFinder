namespace WorkFinder.Web.Models;

public class JobCategory : BaseEntity
{
    public int JobId { get; set; }
    public virtual Job Job { get; set; }
    public int CategoryId { get; set; }
    public virtual Category Category { get; set; }
}