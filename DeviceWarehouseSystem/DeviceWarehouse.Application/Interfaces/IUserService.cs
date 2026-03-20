using DeviceWarehouse.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceWarehouse.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetByIdAsync(int id);
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto> CreateAsync(CreateUserDto userDto);
        Task<UserDto> UpdateAsync(int id, UpdateUserDto userDto);
        Task DeleteAsync(int id);
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
        Task ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<IEnumerable<UserDto>> GetByRoleAsync(string role);
        Task ResetPasswordAsync(string email);
        Task ResetPasswordWithTokenAsync(string token, string newPassword);
        Task BulkCreateAsync(IEnumerable<CreateUserDto> userDtos);
        Task BulkDeleteAsync(IEnumerable<int> userIds);
        Task BulkUpdateStatusAsync(IEnumerable<int> userIds, bool isActive);
        Task LockUserAsync(int userId);
        Task UnlockUserAsync(int userId);
    }
}