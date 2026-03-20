using AutoMapper;
using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;

namespace DeviceWarehouse.Application.Services
{
    public class PermissionService(IPermissionRepository permissionRepository, IMapper mapper) : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository = permissionRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<PermissionDto>> GetAllAsync()
        {
            var permissions = await _permissionRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
        }

        public async Task<PermissionDto> GetByIdAsync(int id)
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            if (permission is null)
                throw new System.Exception("Permission not found");
            return _mapper.Map<PermissionDto>(permission);
        }

        public async Task<PermissionDto> CreateAsync(CreatePermissionDto dto)
        {
            // 检查权限代码是否已存在
            var existingPermission = await _permissionRepository.GetByCodeAsync(dto.Code);
            if (existingPermission is not null)
                throw new System.Exception("Permission code already exists");

            var permission = new Permission
            {
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description
            };

            var createdPermission = await _permissionRepository.AddAsync(permission);
            return _mapper.Map<PermissionDto>(createdPermission);
        }

        public async Task<PermissionDto> UpdateAsync(int id, UpdatePermissionDto dto)
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            if (permission is null)
                throw new System.Exception("Permission not found");

            // 检查权限代码是否已被其他权限使用
            var existingPermission = await _permissionRepository.GetByCodeAsync(dto.Code);
            if (existingPermission is not null && existingPermission.Id != id)
                throw new System.Exception("Permission code already exists");

            permission.Name = dto.Name;
            permission.Code = dto.Code;
            permission.Description = dto.Description;

            await _permissionRepository.UpdateAsync(permission);
            return _mapper.Map<PermissionDto>(permission);
        }

        public async Task DeleteAsync(int id)
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            if (permission is null)
                throw new System.Exception("Permission not found");

            await _permissionRepository.DeleteAsync(id);
        }
    }
}
