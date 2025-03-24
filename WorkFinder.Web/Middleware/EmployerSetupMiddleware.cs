using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WorkFinder.Web.Models;
using WorkFinder.Web.Repositories;

namespace WorkFinder.Web.Middleware
{
    public class EmployerSetupMiddleware
    {
        private readonly RequestDelegate _next;

        public EmployerSetupMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            ICompanyRepository companyRepository)
        {
            // Bỏ qua nếu không phải request HTML hoặc là request tới Auth area
            if (!IsHtmlRequest(context.Request) ||
                context.Request.Path.StartsWithSegments("/Auth") ||
                context.Request.Path.StartsWithSegments("/Employer/Company/Setup"))
            {
                await _next(context);
                return;
            }

            // Kiểm tra nếu người dùng đã đăng nhập và có role Employer
            if (context.User.Identity.IsAuthenticated && context.User.IsInRole(UserRoles.Employer))
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user != null)
                {
                    var company = await companyRepository.GetByOwnerIdAsync(user.Id);

                    // Nếu chưa có thông tin công ty hoặc thông tin không đầy đủ
                    if (company == null || string.IsNullOrEmpty(company.Name) || string.IsNullOrEmpty(company.Description))
                    {
                        // Chỉ chuyển hướng nếu không phải đang ở trang setup
                        if (!context.Request.Path.StartsWithSegments("/Employer/Company"))
                        {
                            context.Response.Redirect("/Employer/Company/SetupBasic");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }

        private bool IsHtmlRequest(HttpRequest request)
        {
            // Kiểm tra xem request có phải là yêu cầu HTML hay không
            if (request.Headers.TryGetValue("Accept", out var values))
            {
                return values.ToString().Contains("text/html");
            }

            return false;
        }
    }

    // Extension method để đăng ký middleware
    public static class EmployerSetupMiddlewareExtensions
    {
        public static IApplicationBuilder UseEmployerSetup(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<EmployerSetupMiddleware>();
        }
    }
}