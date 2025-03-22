using WorkFinder.Web.Models.Enums;

namespace WorkFinder.Web.Models.ViewModels;

public class JobCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string CompanyName { get; set; }
    public string Logo { get; set; }
    public string Location { get; set; }
    public JobType JobType { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int CompanyId { get; set; }

    public int DaysRemaining => (ExpiryDate - DateTime.Now).Days;
}