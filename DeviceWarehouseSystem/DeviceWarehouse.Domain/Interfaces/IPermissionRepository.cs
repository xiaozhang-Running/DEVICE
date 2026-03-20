using DeviceWarehouse.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceWarehouse.Domain.Interfaces
{
    public interface IPermissionRepository
    {
        Task<IEnumerable<Permission>> GetAllAsync();
        Task<Permission?> GetByIdAsync(int id);
        Task<Permission> AddAsync(Permission permission);
        Task UpdateAsync(Permission permission);
        Task DeleteAsync(int id);
        Task<Permission?> GetByCodeAsync(string code);
    }
}
