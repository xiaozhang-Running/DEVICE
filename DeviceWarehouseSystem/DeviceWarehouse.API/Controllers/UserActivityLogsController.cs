using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceWarehouse.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserActivityLogsController : ControllerBase
    {
        private readonly IUserActivityLogService _logService;

        public UserActivityLogsController(IUserActivityLogService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? userId = null,
            [FromQuery] string? activityType = null,
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null,
            [FromQuery] string? keyword = null)
        {
            var logs = await _logService.GetAllAsync();
            
            // 应用过滤
            if (userId.HasValue)
            {
                logs = logs.Where(log => log.UserId == userId.Value);
            }
            
            if (!string.IsNullOrEmpty(activityType))
            {
                logs = logs.Where(log => log.ActivityType == activityType);
            }
            
            if (!string.IsNullOrEmpty(startDate) && System.DateTime.TryParse(startDate, out var start))
            {
                logs = logs.Where(log => log.CreatedAt >= start);
            }
            
            if (!string.IsNullOrEmpty(endDate) && System.DateTime.TryParse(endDate, out var end))
            {
                // 将结束日期设置为当天的最后一刻
                end = end.AddDays(1).AddSeconds(-1);
                logs = logs.Where(log => log.CreatedAt <= end);
            }
            
            if (!string.IsNullOrEmpty(keyword))
            {
                logs = logs.Where(log => 
                    log.ActivityDescription.Contains(keyword) ||
                    (log.UserId.HasValue && log.UserId?.ToString().Contains(keyword) == true) ||
                    (log.IpAddress != null && log.IpAddress.Contains(keyword))
                );
            }
            
            var logList = logs.ToList();
            var totalCount = logList.Count;
            
            // 应用分页
            var paginatedLogs = logList
                .OrderByDescending(log => log.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            return Ok(new { items = paginatedLogs, totalCount });
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserActivityLogDto>>> GetByUserId(int userId)
        {
            var logs = await _logService.GetByUserIdAsync(userId);
            return Ok(logs);
        }

        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<UserActivityLogDto>>> GetByDateRange([FromQuery] System.DateTime startDate, [FromQuery] System.DateTime endDate)
        {
            var logs = await _logService.GetByDateRangeAsync(startDate, endDate);
            return Ok(logs);
        }

        [HttpGet("activity-type/{activityType}")]
        public async Task<ActionResult<IEnumerable<UserActivityLogDto>>> GetByActivityType(string activityType)
        {
            var logs = await _logService.GetByActivityTypeAsync(activityType);
            return Ok(logs);
        }
    }
}
