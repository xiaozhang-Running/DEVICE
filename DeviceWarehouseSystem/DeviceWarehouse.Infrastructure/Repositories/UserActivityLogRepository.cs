using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;
using DeviceWarehouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceWarehouse.Infrastructure.Repositories
{
    public class UserActivityLogRepository : IUserActivityLogRepository
    {
        private readonly ApplicationDbContext _context;

        public UserActivityLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserActivityLog>> GetAllAsync()
        {
            return await _context.UserActivityLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserActivityLog>> GetByUserIdAsync(int userId)
        {
            return await _context.UserActivityLogs
                .Include(l => l.User)
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<UserActivityLog> AddAsync(UserActivityLog log)
    {
        log.CreatedAt = System.DateTime.Now;
        
        try
        {
            // 检查用户是否存在（跳过 UserId 为 0 的情况，因为 0 是默认值）
            if (log.UserId != 0)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == log.UserId);
                if (!userExists)
                {
                    // 用户不存在，不插入日志
                    Console.WriteLine($"用户ID {log.UserId} 不存在，跳过活动日志记录");
                    return log;
                }
                
                // 直接使用EF Core添加，不使用原始SQL
                _context.UserActivityLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            else
            {
                // UserId 为 0，跳过活动日志记录
                Console.WriteLine("用户未登录，跳过活动日志记录");
            }
        }
        catch (Exception ex)
        {
            // 捕获并记录错误，但不影响主流程
            Console.WriteLine($"插入活动日志失败: {ex.Message}");
        }
        
        return log;
    }

        public async Task<IEnumerable<UserActivityLog>> GetByDateRangeAsync(System.DateTime startDate, System.DateTime endDate)
        {
            return await _context.UserActivityLogs
                .Include(l => l.User)
                .Where(l => l.CreatedAt >= startDate && l.CreatedAt <= endDate)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserActivityLog>> GetByActivityTypeAsync(string activityType)
        {
            return await _context.UserActivityLogs
                .Include(l => l.User)
                .Where(l => l.ActivityType == activityType)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }
    }
}
