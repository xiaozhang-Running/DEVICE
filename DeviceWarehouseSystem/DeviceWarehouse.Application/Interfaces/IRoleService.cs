using DeviceWarehouse.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceWarehouse.Application.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllAsync();
        Task<RoleDto> GetByIdAsync(int id);
        Task<RoleDto> CreateAsync(CreateRoleDto dto);
        Task<RoleDto> UpdateAsync(int id, UpdateRoleDto dto);
        Task DeleteAsync(int id);
    }
}
