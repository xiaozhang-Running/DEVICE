using DeviceWarehouse.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceWarehouse.Domain.Interfaces
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role?> GetByIdAsync(int id);
        Task<Role> AddAsync(Role role);
        Task UpdateAsync(Role role);
        Task DeleteAsync(int id);
        Task<Role?> GetByNameAsync(string name);
    }
}
