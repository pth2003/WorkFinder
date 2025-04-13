using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Models;
using WorkFinder.Web.Models.Enums;

namespace WorkFinder.Web.Data;
// Data/DataSeeder.cs
public static class DataSeeder
{
    public static async Task SeedData(WorkFinderContext context, UserManager<ApplicationUser> userManager, bool resetData = false)
    {
        // Luôn đảm bảo có một tài khoản Admin, bất kể database đã có dữ liệu hay chưa
        await EnsureAdminUserCreated(userManager);

        // Nếu yêu cầu reset dữ liệu, xóa toàn bộ dữ liệu hiện có trước
        if (resetData)
        {
            await ResetDatabase(context);
            Console.WriteLine("Database has been reset. Now seeding fresh data...");
        }

        // Kiểm tra xem database có dữ liệu không
        if (resetData || (!context.Users.Any() && !context.Companies.Any() && !context.Jobs.Any()))
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
                    var errorMessage = "ERROR: WorkFinder.sql file not found. Database initialization cannot continue.";
                    Console.WriteLine(errorMessage);
                    throw new FileNotFoundException(errorMessage, "WorkFinder.sql");
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"ERROR: Failed to import SQL data: {ex.Message}";
                Console.WriteLine(errorMessage);
                throw new Exception(errorMessage, ex);
            }
        }
        else
        {
            Console.WriteLine("Database already contains data, skipping initialization.");
        }
    }

    // Phương thức đảm bảo tài khoản Admin luôn tồn tại trong database
    private static async Task EnsureAdminUserCreated(UserManager<ApplicationUser> userManager)
    {
        try
        {
            // Thông tin tài khoản admin
            string adminEmail = "admin@workfinder.com";
            string adminPassword = "Admin@123456";

            Console.WriteLine("Ensuring admin user exists...");

            // Kiểm tra xem tài khoản admin đã tồn tại chưa
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // Tạo tài khoản admin mới nếu chưa tồn tại
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Administrator"
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    Console.WriteLine($"Admin user ({adminEmail}) created successfully.");

                    // Kiểm tra xem role Admin đã tồn tại chưa
                    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                    {
                        // Thêm role Admin cho tài khoản admin
                        result = await userManager.AddToRoleAsync(adminUser, "Admin");

                        if (result.Succeeded)
                        {
                            Console.WriteLine("Admin role assigned successfully.");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to assign Admin role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Kiểm tra xem admin đã có role Admin chưa
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    var result = await userManager.AddToRoleAsync(adminUser, "Admin");

                    if (result.Succeeded)
                    {
                        Console.WriteLine("Admin role assigned to existing user.");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to assign Admin role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    Console.WriteLine("Admin user already exists with Admin role.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error ensuring admin user exists: {ex.Message}");
        }
    }

    // Phương thức để reset toàn bộ dữ liệu trong database
    private static async Task ResetDatabase(WorkFinderContext context)
    {
        try
        {
            Console.WriteLine("Starting database reset...");

            // Tạo và thực thi các lệnh SQL để xóa dữ liệu từ các bảng
            // Lưu ý: Thứ tự xóa quan trọng để tránh lỗi khóa ngoại
            string[] deleteCommands = new string[]
            {
                "DELETE FROM \"job_applications\"",
                "DELETE FROM \"saved_jobs\"",
                "DELETE FROM \"job_categories\"",
                "DELETE FROM \"jobs\"",
                "DELETE FROM \"resumes\"",
                "DELETE FROM \"companies\"",
                "DELETE FROM \"categories\"",
                "DELETE FROM \"AspNetUserRoles\"",
                "DELETE FROM \"AspNetUserLogins\"",
                "DELETE FROM \"AspNetUserClaims\"",
                "DELETE FROM \"AspNetUserTokens\"",
                "DELETE FROM \"AspNetRoleClaims\"",
                "DELETE FROM \"AspNetRoles\"",
                "DELETE FROM \"users\"" // Đổi tên bảng nếu cần thiết
            };

            // Vô hiệu hóa ràng buộc khóa ngoại tạm thời
            await context.Database.ExecuteSqlRawAsync("SET CONSTRAINTS ALL DEFERRED;");

            foreach (var command in deleteCommands)
            {
                try
                {
                    await context.Database.ExecuteSqlRawAsync(command);
                    Console.WriteLine($"Executed: {command}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing '{command}': {ex.Message}");
                }
            }

            // Khôi phục các ràng buộc khóa ngoại
            await context.Database.ExecuteSqlRawAsync("SET CONSTRAINTS ALL IMMEDIATE;");

            // Reset các sequence (chỉ dành cho PostgreSQL)
            string[] resetSequenceCommands = new string[]
            {
                "SELECT setval('\"users_Id_seq\"', 1, false)",
                "SELECT setval('\"companies_Id_seq\"', 1, false)",
                "SELECT setval('\"jobs_Id_seq\"', 1, false)",
                "SELECT setval('\"categories_Id_seq\"', 1, false)",
                "SELECT setval('\"job_categories_Id_seq\"', 1, false)",
                "SELECT setval('\"job_applications_Id_seq\"', 1, false)",
                "SELECT setval('\"saved_jobs_Id_seq\"', 1, false)",
                "SELECT setval('\"resumes_Id_seq\"', 1, false)"
            };

            foreach (var command in resetSequenceCommands)
            {
                try
                {
                    await context.Database.ExecuteSqlRawAsync(command);
                    Console.WriteLine($"Executed: {command}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error resetting sequence: {ex.Message}");
                }
            }

            Console.WriteLine("Database reset completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to reset database: {ex.Message}");
            throw;
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
            Console.WriteLine($"Successfully executed {commands.Count} SQL commands");
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