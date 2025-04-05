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
            // Bỏ qua middleware này cho trường hợp các route error hoặc tài khoản
            if (context.Request.Path.StartsWithSegments("/Error", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/Auth/Error", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/Employer/Error", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/Employer/NotFound", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/Auth/NotFound", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/NotFound", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/Account", StringComparison.OrdinalIgnoreCase)) // Cho phép tất cả các role truy cập vào AccountController
            {

                await _next(context);
                return;
            }

            if (context.User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null)
                {
                    // Nếu không tìm thấy user, đăng xuất và chuyển hướng về trang đăng nhập
                    Console.WriteLine("RoleAuthorizationMiddleware: User not found, redirecting to login");
                    context.Response.Redirect("/Auth/Login");
                    return;
                }

                var roles = await userManager.GetRolesAsync(user);

                // Kiểm tra path hiện tại
                var path = context.Request.Path.Value.ToLower();


                // Nếu user có role là employer
                if (roles.Contains(UserRoles.Employer))
                {
                    // Nếu đang cố truy cập vào trang không có prefix (trang dành cho JobSeeker)
                    if (!path.StartsWith("/employer") && !path.StartsWith("/auth"))
                    {
                        // Chuyển hướng về trang home của employer
                        Console.WriteLine("RoleAuthorizationMiddleware: Employer accessing JobSeeker area, redirecting");
                        context.Response.Redirect("/employer");
                        return;
                    }
                }
                // Nếu user có role là jobseeker
                else if (roles.Contains(UserRoles.JobSeeker))
                {
                    // Nếu đang cố truy cập vào trang của employer
                    if (path.StartsWith("/employer"))
                    {
                        // Chuyển hướng về trang home của jobseeker
                        Console.WriteLine("RoleAuthorizationMiddleware: JobSeeker accessing Employer area, redirecting");
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