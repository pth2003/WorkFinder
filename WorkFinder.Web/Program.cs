using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Areas.Auth.Services;
using WorkFinder.Web.Areas.Employer.Services;
using WorkFinder.Web.Data;
using WorkFinder.Web.Middleware;
using WorkFinder.Web.Models;
using WorkFinder.Web.Repositories;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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
builder.Services.AddDbContext<WorkFinderContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
// Add specific repositories if needed
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IResumeRepository, ResumeRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
// builder.Services.AddScoped<IRepository<Resume>, Repository<Resume>>();
// builder.Services.AddScoped<IRepository<SavedJob>, Repository<SavedJob>>();
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
});
var app = builder.Build();




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

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

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
// Giữ lại các route hiện có
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
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
app.Run();