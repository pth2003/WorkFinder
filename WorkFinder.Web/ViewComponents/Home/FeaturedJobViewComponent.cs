using Microsoft.AspNetCore.Mvc;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.ViewComponents.Home;

public class FeaturedJobViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Mock dữ liệu giả lập
        // var jobs = new List<Job>
        // {
        //     new Job
        //     {
        //         Id = 1,
        //         Title = "Senior UX Designer",
        //         Description = "Design user experience for web applications.",
        //         Salary = 35000m,
        //         Location = "Australia",
        //         CompanyId = 1,
        //         CategoryId = 1,
        //         PostedDate = DateTime.Now.AddDays(-2),
        //         Status = "open",
        //         Company = new Company
        //         {
        //             Id = 1,
        //             Name = "Upwork",
        //             LogoUrl = "https://img.logo.dev/apple.com?token=pk_JNtYBCy6RyOkevRWc-7ghw"
        //         }
        //     },
        //     new Job
        //     {
        //         Id = 2,
        //         Title = "Software Engineer",
        //         Description = "Develop software solutions.",
        //         Salary = 55000m,
        //         Location = "China",
        //         CompanyId = 2,
        //         CategoryId = 2,
        //         PostedDate = DateTime.Now.AddDays(-3),
        //         Status = "open",
        //         Company = new Company
        //         {
        //             Id = 2,
        //             Name = "Apple",
        //             LogoUrl = "https://img.logo.dev/nvidia.com?token=pk_JNtYBCy6RyOkevRWc-7ghw"
        //         }
        //     },
        //     new Job
        //     {
        //         Id = 3,
        //         Title = "Junior Graphic Designer",
        //         Description = "Create visual designs.",
        //         Salary = 60000m,
        //         Location = "Canada",
        //         CompanyId = 3,
        //         CategoryId = 3,
        //         PostedDate = DateTime.Now.AddDays(-1),
        //         Status = "open",
        //         Company = new Company
        //         {
        //             Id = 3,
        //             Name = "Figma",
        //             LogoUrl = "https://img.logo.dev/google.com?token=pk_JNtYBCy6RyOkevRWc-7ghw"
        //         }
        //     },
        //     new Job
        //     {
        //         Id = 1,
        //         Title = "Senior UX Designer",
        //         Description = "Design user experience for web applications.",
        //         Salary = 35000m,
        //         Location = "Australia",
        //         CompanyId = 1,
        //         CategoryId = 1,
        //         PostedDate = DateTime.Now.AddDays(-2),
        //         Status = "open",
        //         Company = new Company
        //         {
        //             Id = 1,
        //             Name = "Upwork",
        //             LogoUrl = "https://img.logo.dev/apple.com?token=pk_JNtYBCy6RyOkevRWc-7ghw"
        //         }
        //     },
        //     new Job
        //     {
        //         Id = 2,
        //         Title = "Software Engineer",
        //         Description = "Develop software solutions.",
        //         Salary = 55000m,
        //         Location = "China",
        //         CompanyId = 2,
        //         CategoryId = 2,
        //         PostedDate = DateTime.Now.AddDays(-3),
        //         Status = "open",
        //         Company = new Company
        //         {
        //             Id = 2,
        //             Name = "Apple",
        //             LogoUrl = "https://img.logo.dev/nvidia.com?token=pk_JNtYBCy6RyOkevRWc-7ghw"
        //         }
        //     },
        //     new Job
        //     {
        //         Id = 3,
        //         Title = "Junior Graphic Designer",
        //         Description = "Create visual designs.",
        //         Salary = 60000m,
        //         Location = "Canada",
        //         CompanyId = 3,
        //         CategoryId = 3,
        //         PostedDate = DateTime.Now.AddDays(-1),
        //         Status = "open",
        //         Company = new Company
        //         {
        //             Id = 3,
        //             Name = "Figma",
        //             LogoUrl = "https://img.logo.dev/google.com?token=pk_JNtYBCy6RyOkevRWc-7ghw"
        //         }
        //     }
        // };

        return View();
    
    }
}