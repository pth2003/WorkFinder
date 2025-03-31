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
            // Bỏ qua middleware này cho trường hợp các route error 
            if (context.Request.Path.StartsWithSegments("/Error") ||
                context.Request.Path.StartsWithSegments("/Auth/Error") ||
                context.Request.Path.StartsWithSegments("/Employer/Error") ||
                context.Request.Path.StartsWithSegments("/Employer/NotFound") ||
                context.Request.Path.StartsWithSegments("/Auth/NotFound") ||
                context.Request.Path.StartsWithSegments("/NotFound"))
            {
                await _next(context);
                return;
            }

            if (context.User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(context.User);
                var roles = await userManager.GetRolesAsync(user);

                // Kiểm tra path hiện tại
                var path = context.Request.Path.Value.ToLower();

                // Nếu user có role là employer
                if (roles.Contains("Employer"))
                {
                    // Nếu đang cố truy cập vào trang không có prefix (trang dành cho JobSeeker)
                    if (!path.StartsWith("/employer") && !path.StartsWith("/auth"))
                    {
                        // Chuyển hướng về trang home của employer
                        context.Response.Redirect("/employer");
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