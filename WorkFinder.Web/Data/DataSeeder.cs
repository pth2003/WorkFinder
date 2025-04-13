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