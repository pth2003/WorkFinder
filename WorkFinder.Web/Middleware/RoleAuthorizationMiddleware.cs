using Microsoft.AspNetCore.Identity;
using WorkFinder.Web.Models;

namespace WorkFinder.Web.Middleware
{
    public class RoleAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(context.User);
                var roles = await userManager.GetRolesAsync(user);

                // Kiểm tra path hiện tại
                var path = context.Request.Path.Value.ToLower();

                // Nếu user có role là employer
                if (roles.Contains("Employer"))
                {
                    // Nếu đang cố truy cập vào trang của jobseeker
                    if (path.StartsWith("/jobseeker") || path == "/" || path == "/home" || path == "/home/index")
                    {
                        // Chuyển hướng về trang home của employer
                        context.Response.Redirect("/employer/home");
                        return;
                    }
                }
                // Nếu user có role là jobseeker
                else if (roles.Contains("JobSeeker"))
                {
                    // Nếu đang cố truy cập vào trang của employer
                    if (path.StartsWith("/employer"))
                    {
                        // Chuyển hướng về trang home của jobseeker
                        context.Response.Redirect("/");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

    // Extension method để thêm middleware vào pipeline
    public static class RoleAuthorizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseRoleAuthorization(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RoleAuthorizationMiddleware>();
        }
    }
}