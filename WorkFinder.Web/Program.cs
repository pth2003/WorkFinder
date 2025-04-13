using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.FileProviders;
using WorkFinder.Web.Areas.Auth.Services;
using WorkFinder.Web.Areas.Employer.Services;
using WorkFinder.Web.Data;
using WorkFinder.Web.Middleware;
using WorkFinder.Web.Models;
using WorkFinder.Web.Repositories;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http.Features;
using WorkFinder.Web.Areas.Employer.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using WorkFinder.Web.BackgroundServices;
using WorkFinder.Web.Services;
using WorkFinder.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Có thể thêm global filters nếu cần
})
.ConfigureApplicationPartManager(manager =>
{
    // Clear và đăng ký lại các phần của ứng dụng nếu cần
    // manager.ApplicationParts.Clear();
    // manager.ApplicationParts.Add(new AssemblyPart(typeof(Program).Assembly));
});

// Cấu hình giới hạn kích thước form
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 30 * 1024 * 1024; // 30MB
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 30 * 1024 * 1024; // 30MB
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 30 * 1024 * 1024; // 30MB
});

// Cấu hình form options
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 30 * 1024 * 1024; // 30MB
    options.ValueLengthLimit = 30 * 1024 * 1024; // 30MB
    options.MultipartHeadersLengthLimit = 30 * 1024 * 1024; // 30MB
});

// Đăng ký DbContext với PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Kiểm tra và ghi đè connection string từ biến môi trường nếu có
var pgHost = Environment.GetEnvironmentVariable("PGHOST");
var pgPort = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
var pgUser = Environment.GetEnvironmentVariable("PGUSER");
var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");

if (!string.IsNullOrEmpty(pgHost) && !string.IsNullOrEmpty(pgDatabase) &&
    !string.IsNullOrEmpty(pgUser) && !string.IsNullOrEmpty(pgPassword))
{
    connectionString = $"Host={pgHost};Port={pgPort};Database={pgDatabase};Username={pgUser};Password={pgPassword};SSL Mode=Require;Trust Server Certificate=true;";
    Console.WriteLine($"Using PostgreSQL connection from environment variables. Host: {pgHost}");
}
else
{
    Console.WriteLine("Using PostgreSQL connection from appsettings.json");
}

builder.Services.AddDbContext<WorkFinderContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
// Add specific repositories if needed
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IResumeRepository, ResumeRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ISaveJobRepository, SaveJobRepository>();

// Đăng ký Identity
// Add authentication services
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
    })
    .AddEntityFrameworkStores<WorkFinderContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICompanySetupService, CompanySetupService>();
builder.Services.AddHttpContextAccessor();

// Đăng ký session để lưu trữ dữ liệu tạm thời
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".WorkFinder.Session";
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Trong development, set là None
});

// Configure cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.Cookie.Name = ".WorkFinder.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// Register repositories
builder.Services.AddScoped<IJobAlertRepository, JobAlertRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Register services
builder.Services.AddScoped<JobAlertService>();

// Register background services
builder.Services.AddHostedService<JobAlertBackgroundService>();

var app = builder.Build();

// Tự động áp dụng migrations trong Production
if (app.Environment.IsProduction())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<WorkFinderContext>();
        try
        {
            dbContext.Database.Migrate();
            Console.WriteLine("Database migrations applied successfully.");

            // Seed data
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            await WorkFinder.Web.Data.DataSeeder.SeedData(dbContext, userManager);
            Console.WriteLine("Database seeded successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while applying migrations: {ex.Message}");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Cấu hình xử lý lỗi status code (404, 500, etc.)
app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

// Thêm endpoint health check cho Render
app.MapGet("/health", () => "Healthy");

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// Thêm middleware RoleAuthorization
app.UseRoleAuthorization();

// Thêm middleware kiểm tra và chuyển hướng Employer
app.UseEmployerSetup();

app.MapStaticAssets();
// Thêm route riêng cho Auth area
app.MapControllerRoute(
    name: "auth",
    pattern: "Auth/{action=Login}",
    defaults: new { area = "Auth", controller = "Auth" });

// Route cho Employer area
app.MapControllerRoute(
    name: "employer",
    pattern: "Employer/{controller=Home}/{action=Index}/{id?}",
    defaults: new { area = "Employer" });

// Route mặc định cho JobSeeker 
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Khởi tạo roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

        string[] roleNames = { UserRoles.Admin, UserRoles.JobSeeker, UserRoles.Employer };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(roleName));
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding roles");
    }
}

// Cập nhật slug cho các job hiện có
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var dbContext = services.GetRequiredService<WorkFinderContext>();

        var jobsWithoutSlug = await dbContext.Jobs.Where(j => string.IsNullOrEmpty(j.Slug)).ToListAsync();
        if (jobsWithoutSlug.Any())
        {
            logger.LogInformation($"Found {jobsWithoutSlug.Count} jobs without slug. Updating...");

            foreach (var job in jobsWithoutSlug)
            {
                job.Slug = job.Title.ToSlug();
                dbContext.Update(job);
            }

            await dbContext.SaveChangesAsync();
            logger.LogInformation("Slug update completed.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while updating job slugs");
    }
}

app.Run();