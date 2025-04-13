using Microsoft.AspNetCore.Identity;

namespace WorkFinder.Web.Data;

public static class RoleInitializer
{
    public static async Task InitializeAsync(RoleManager<IdentityRole<int>> roleManager)
    {
        // Danh sách các role cần khởi tạo
        string[] roleNames = { "Admin", "Employer", "Candidate" };

        // Lặp qua từng role và tạo nếu chưa tồn tại
        foreach (var roleName in roleNames)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                // Tạo role mới
                var role = new IdentityRole<int>(roleName);
                var result = await roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    Console.WriteLine($"Role '{roleName}' created successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine($"Role '{roleName}' already exists.");
            }
        }
    }
}