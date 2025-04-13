using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace WorkFinder.Web.Extensions
{
    public static class WebApplicationExtensions
    {
        /// <summary>
        /// Ánh xạ tài nguyên tĩnh
        /// </summary>
        /// <param name="app">WebApplication instance</param>
        /// <returns>WebApplication</returns>
        public static WebApplication MapStaticAssets(this WebApplication app)
        {
            // Không làm gì cả, chỉ là một phương thức trống để tương thích với code cũ
            return app;
        }

        /// <summary>
        /// Thêm xử lý tài nguyên tĩnh cho các route
        /// </summary>
        /// <param name="builder">ControllerActionEndpointConventionBuilder instance</param>
        /// <returns>ControllerActionEndpointConventionBuilder</returns>
        public static ControllerActionEndpointConventionBuilder WithStaticAssets(this ControllerActionEndpointConventionBuilder builder)
        {
            // Không làm gì cả, chỉ là một phương thức trống để tương thích với code cũ
            return builder;
        }
    }
}