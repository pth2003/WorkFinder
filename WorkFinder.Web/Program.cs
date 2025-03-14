using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
// Đăng ký Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddEntityFrameworkStores<WorkFinderContext>()
    .AddDefaultTokenProviders();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
// Add specific repositories if needed
builder.Services.AddScoped<IRepository<Resume>, Repository<Resume>>();
builder.Services.AddScoped<IRepository<SavedJob>, Repository<SavedJob>>();
var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<WorkFinderContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    await DataSeeder.SeedData(context, userManager);
}

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

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();