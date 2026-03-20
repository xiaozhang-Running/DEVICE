using Microsoft.AspNetCore.Builder;

namespace DeviceWarehouse.API.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseUserActivityLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserActivityLogMiddleware>();
        }
    }
}
