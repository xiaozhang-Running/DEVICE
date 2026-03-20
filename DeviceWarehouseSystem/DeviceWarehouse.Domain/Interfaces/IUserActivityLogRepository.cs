using DeviceWarehouse.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceWarehouse.Domain.Interfaces
{
    public interface IUserActivityLogRepository
    {
        Task<IEnumerable<UserActivityLog>> GetAllAsync();
        Task<IEnumerable<UserActivityLog>> GetByUserIdAsync(int userId);
        Task<UserActivityLog> AddAsync(UserActivityLog log);
        Task<IEnumerable<UserActivityLog>> GetByDateRangeAsync(System.DateTime startDate, System.DateTime endDate);
        Task<IEnumerable<UserActivityLog>> GetByActivityTypeAsync(string activityType);
    }
}
