using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Data;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.ViewModels;

namespace WorkFinder.Web.ViewComponents.Home;

public class FeaturedJobViewComponent : ViewComponent
{
    private readonly WorkFinderContext _context;
    public  FeaturedJobViewComponent(WorkFinderContext context)
    {
        _context = context;
    }
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var jobs = await _context.Jobs
            .Include(j => j.Company)
            .Take(8)
            .Select(j => new JobCardViewModel
            {
                Title = j.Title,
                CompanyName = j.Company.Name,
                Logo = j.Company.Logo ?? "/images/default-company.png",
                Location = j.Location,
                JobType = j.JobType,
                SalaryMin = j.SalaryMin,
                SalaryMax = j.SalaryMax,
                ExpiryDate = j.ExpiryDate
            })
            .ToListAsync();
            
        return View(jobs);
    }
}