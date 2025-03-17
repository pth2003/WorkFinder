using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;

namespace WorkFinder.Web.Data;
// Data/DataSeeder.cs
public static class DataSeeder
{
    public static async Task SeedData(WorkFinderContext context, UserManager<ApplicationUser> userManager)
    {
        // Create default user first
     if (!userManager.Users.Any())
     {
         var user = new ApplicationUser
         {
             UserName = "admin@workfinder.com",
             Email = "admin@workfinder.com",
             EmailConfirmed = true,
             FirstName = "Admin",
             LastName = "User"
         };
     
         var result = await userManager.CreateAsync(user, "Admin@123456!");
         if (!result.Succeeded)
         {
             throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
         }
     }

        // Seed Categories
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new Category { Name = "Software Development", Description = "Programming and software engineering roles" },
                new Category { Name = "Design", Description = "UI/UX and graphic design positions" },
                new Category { Name = "Marketing", Description = "Digital marketing and SEO roles" },
                new Category { Name = "Sales", Description = "Sales and business development positions" },
                new Category { Name = "Data Science", Description = "Data analysis and machine learning roles" }
            };
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // Seed Companies
        Company techVision = null;
        Company digitalMinds = null;
        
        if (!context.Companies.Any())
        {
            var owner = await userManager.FindByEmailAsync("admin@workfinder.com");
            
            techVision = new Company
            {
                Name = "TechVision",
                Description = "Leading software development company",
                Logo = "/images/companies/techvision.png",
                Website = "https://techvision.com",
                Location = "Ha Noi",
                EmployeeCount = 200,
                Industry = "Technology",
                IsVerified = true,
                OwnerId = owner.Id
            };

            digitalMinds = new Company
            {
                Name = "Digital Minds",
                Description = "Digital marketing agency",
                Logo = "/images/companies/digitalminds.png",
                Website = "https://digitalminds.com",
                Location = "Ho Chi Minh",
                EmployeeCount = 100,
                Industry = "Marketing",
                IsVerified = true,
                OwnerId = owner.Id
            };

            await context.Companies.AddRangeAsync(new[] { techVision, digitalMinds });
            await context.SaveChangesAsync();
        }

        // Seed Jobs and JobCategories
        if (!context.Jobs.Any())
        {
            var now = DateTime.UtcNow;
            
            // Ensure we have the companies if they weren't just created
            if (techVision == null)
                techVision = await context.Companies.FirstOrDefaultAsync(c => c.Name == "TechVision");
            if (digitalMinds == null)
                digitalMinds = await context.Companies.FirstOrDefaultAsync(c => c.Name == "Digital Minds");

            var jobs = new List<Job>
            {
                new Job
                {
                    Title = "Senior .NET Developer",
                    Description = "Looking for experienced .NET developer",
                    Requirements = "5+ years experience with .NET Core",
                    Benefits = "Competitive salary, health insurance",
                    Location = "Ha Noi",
                    SalaryMin = 2000,
                    SalaryMax = 3500,
                    JobType = JobType.FullTime,
                    ExperienceLevel = ExperienceLevel.Senior,
                    ExpiryDate = now.AddMonths(1),
                    IsActive = true,
                    Company = techVision
                },
                new Job
                {
                    Title = "UI/UX Designer",
                    Description = "Creative designer needed",
                    Requirements = "3+ years UI/UX experience",
                    Benefits = "Flexible hours, remote work",
                    Location = "Ho Chi Minh",
                    SalaryMin = 1500,
                    SalaryMax = 2500,
                    JobType = JobType.FullTime,
                    ExperienceLevel = ExperienceLevel.Intermediate,
                    ExpiryDate = now.AddMonths(1),
                    IsActive = true,
                    Company = digitalMinds
                }
            };
            await context.Jobs.AddRangeAsync(jobs);
            await context.SaveChangesAsync();
        }
    }
}