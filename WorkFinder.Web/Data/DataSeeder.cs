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
        // Kiểm tra xem database có dữ liệu không
        if (!context.Users.Any() && !context.Companies.Any() && !context.Jobs.Any())
        {
            try
            {
                // Tìm file SQL trong thư mục gốc
                var sqlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "WorkFinder.sql");

                if (File.Exists(sqlFilePath))
                {
                    Console.WriteLine("Found SQL file, importing data...");

                    // Đọc nội dung file SQL
                    var sqlContent = await File.ReadAllTextAsync(sqlFilePath);

                    // Thực thi SQL script
                    await ExecuteSqlScript(context, sqlContent);

                    Console.WriteLine("SQL data import completed successfully.");
                    return;
                }
                else
                {
                    Console.WriteLine("SQL file not found, using default seeding...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing SQL data: {ex.Message}");
                Console.WriteLine("Falling back to default seeding...");
            }
        }

        // Default seeding logic continues below
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

    private static async Task ExecuteSqlScript(WorkFinderContext context, string sqlScript)
    {
        // Tách các câu lệnh SQL bằng dấu chấm phẩy (có thể cần xử lý đặc biệt cho câu lệnh phức tạp)
        var commands = new List<string>();
        var currentCommand = new System.Text.StringBuilder();

        // Xử lý từng dòng để đảm bảo không tách những dấu chấm phẩy nằm trong chuỗi hay comment
        foreach (var line in sqlScript.Split('\n'))
        {
            var trimmedLine = line.Trim();

            // Bỏ qua dòng trống và comment
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("--"))
            {
                continue;
            }

            currentCommand.AppendLine(line);

            // Nếu dòng kết thúc bằng dấu chấm phẩy và không phải trong chuỗi hoặc comment
            if (trimmedLine.EndsWith(";") && !IsInsideStringOrComment(trimmedLine))
            {
                commands.Add(currentCommand.ToString());
                currentCommand.Clear();
            }
        }

        // Thêm lệnh cuối cùng nếu có
        if (currentCommand.Length > 0)
        {
            commands.Add(currentCommand.ToString());
        }

        // Lấy connection từ context
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();
        try
        {
            foreach (var commandText in commands)
            {
                if (string.IsNullOrWhiteSpace(commandText.Trim()))
                    continue;

                using var command = connection.CreateCommand();
                command.CommandText = commandText;
                command.Transaction = transaction as System.Data.Common.DbTransaction;
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Error executing SQL script: {ex.Message}", ex);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    // Phương thức đơn giản để kiểm tra xem dấu chấm phẩy có nằm trong chuỗi hoặc comment không
    private static bool IsInsideStringOrComment(string line)
    {
        int singleQuoteCount = 0;
        int doubleQuoteCount = 0;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '\'')
                singleQuoteCount++;
            else if (line[i] == '"')
                doubleQuoteCount++;
        }

        // Nếu số lượng dấu nháy đơn hoặc kép là lẻ, chúng ta đang ở trong một chuỗi
        return (singleQuoteCount % 2 != 0) || (doubleQuoteCount % 2 != 0) || line.Contains("--");
    }
}