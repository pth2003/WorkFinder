namespace WorkFinder.Web.Models;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public virtual ICollection<JobCategory> Jobs { get; set; } = new List<JobCategory>();
}