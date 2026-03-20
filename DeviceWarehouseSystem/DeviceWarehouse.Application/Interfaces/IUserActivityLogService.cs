using DeviceWarehouse.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceWarehouse.Application.Interfaces
{
    public interface IUserActivityLogService
    {
        Task<IEnumerable<UserActivityLogDto>> GetAllAsync();
        Task<IEnumerable<UserActivityLogDto>> GetByUserIdAsync(int userId);
        Task<UserActivityLogDto> CreateAsync(int? userId, string activityType, string activityDescription, string? ipAddress = null, string? userAgent = null);
        Task<IEnumerable<UserActivityLogDto>> GetByDateRangeAsync(System.DateTime startDate, System.DateTime endDate);
        Task<IEnumerable<UserActivityLogDto>> GetByActivityTypeAsync(string activityType);
    }
}
