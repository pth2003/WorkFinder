using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkFinder.Web.Areas.Auth.Services;
using WorkFinder.Web.Data;
using WorkFinder.Web.Models;
using WorkFinder.Web.Repositories;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Đăng ký DbContext với PostgreSQL
builder.Services.AddDbContext<WorkFinderContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
// Add specific repositories if needed
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
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
builder.Services.AddHttpContextAccessor();

// Configure cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
});
var app = builder.Build();


// using (var scope = app.Services.CreateScope())
// {
//     var services = scope.ServiceProvider;
//     var context = services.GetRequiredService<WorkFinderContext>();
//     var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
//     await DataSeeder.SeedData(context, userManager);
// }

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

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

app.Run();