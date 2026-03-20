using DeviceWarehouse.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceWarehouse.Application.Interfaces
{
    public interface IPermissionService
    {
        Task<IEnumerable<PermissionDto>> GetAllAsync();
        Task<PermissionDto> GetByIdAsync(int id);
        Task<PermissionDto> CreateAsync(CreatePermissionDto dto);
        Task<PermissionDto> UpdateAsync(int id, UpdatePermissionDto dto);
        Task DeleteAsync(int id);
    }
}
