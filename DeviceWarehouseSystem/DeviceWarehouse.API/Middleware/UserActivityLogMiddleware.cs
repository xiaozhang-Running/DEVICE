using DeviceWarehouse.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DeviceWarehouse.API.Middleware
{
    public class UserActivityLogMiddleware
    {
        private readonly RequestDelegate _next;

        public UserActivityLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUserActivityLogService activityLogService)
        {
            // 记录请求开始时间
            var startTime = System.DateTime.Now;
            
            // 提取用户信息
            int? userId = null;
            var userIdClaim = context.User.FindFirst("UserId");
            if (userIdClaim != null)
            {
                if (int.TryParse(userIdClaim.Value, out int id))
                {
                    userId = id;
                }
            }
            
            // 提取请求信息
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var method = context.Request.Method;
            var path = context.Request.Path.ToString();
            var queryString = context.Request.QueryString.ToString();
            
            // 执行请求
            await _next(context);
            
            // 记录响应信息
            var statusCode = context.Response.StatusCode;
            var endTime = System.DateTime.Now;
            var duration = (endTime - startTime).TotalMilliseconds;
            
            // 构建活动描述
            var activityDescription = $"{method} {path}{queryString} - 状态码: {statusCode} - 耗时: {duration:F2}ms";
            
            // 确定活动类型
            string activityType = "Other";
            switch (method)
            {
                case "POST":
                    activityType = "Create";
                    break;
                case "PUT":
                case "PATCH":
                    activityType = "Update";
                    break;
                case "DELETE":
                    activityType = "Delete";
                    break;
                case "GET":
                    activityType = "Read";
                    break;
            }
            
            // 记录日志（只记录非静态文件请求、非日志查询请求）
            if (!path.StartsWith("/static") && !path.StartsWith("/api/UserActivityLogs"))
            {
                try
                {
                    await activityLogService.CreateAsync(userId, activityType, activityDescription, ipAddress, userAgent);
                }
                catch (System.Exception ex)
                {
                    // 捕获并记录错误，但不影响主流程
                    System.Console.WriteLine($"记录活动日志失败: {ex.Message}");
                }
            }
        }
    }
}