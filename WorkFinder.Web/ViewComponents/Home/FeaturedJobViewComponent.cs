using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Data;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.ViewModels;
using WorkFinder.Web.Repositories;

namespace WorkFinder.Web.ViewComponents.Home;

public class FeaturedJobViewComponent : ViewComponent
{
    private readonly IJobRepository _jobRepository;

    public FeaturedJobViewComponent(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var featuredJobs = await _jobRepository.GetFeaturedJobsAsync(6);

        var jobsViewModel = featuredJobs.Select(j => new JobCardViewModel
        {
            Id = j.Id,
            Title = j.Title,
            CompanyName = j.Company?.Name ?? "Unknown Company",
            Logo = j.Company?.Logo ?? "/img/default-company.png",
            Location = j.Location,
            JobType = j.JobType,
            SalaryMin = j.SalaryMin,
            SalaryMax = j.SalaryMax,
            ExpiryDate = j.ExpiryDate,
            CompanyId = j.CompanyId
        }).ToList();

        return View(jobsViewModel);
    }
}