namespace WorkFinder.Web.Models.ViewModels;

public class HomeViewModel
{
    public ICollection<Company> Companies { get; set; } = new List<Company>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}