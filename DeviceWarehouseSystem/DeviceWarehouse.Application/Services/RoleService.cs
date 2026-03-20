using AutoMapper;
using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceWarehouse.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IMapper _mapper;

        public RoleService(IRoleRepository roleRepository, IPermissionRepository permissionRepository, IMapper mapper)
        {
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoleDto>> GetAllAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<RoleDto>>(roles);
        }

        public async Task<RoleDto> GetByIdAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                throw new System.Exception("Role not found");
            return _mapper.Map<RoleDto>(role);
        }

        public async Task<RoleDto> CreateAsync(CreateRoleDto dto)
        {
            // 检查角色名是否已存在
            var existingRole = await _roleRepository.GetByNameAsync(dto.Name);
            if (existingRole != null)
                throw new System.Exception("Role name already exists");

            // 获取权限
            var permissions = new List<Permission>();
            if (dto.PermissionIds != null && dto.PermissionIds.Count > 0)
            {
                foreach (var permissionId in dto.PermissionIds)
                {
                    var permission = await _permissionRepository.GetByIdAsync(permissionId);
                    if (permission != null)
                    {
                        permissions.Add(permission);
                    }
                }
            }

            var role = new Role
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive,
                Permissions = permissions
            };

            var createdRole = await _roleRepository.AddAsync(role);
            return _mapper.Map<RoleDto>(createdRole);
        }

        public async Task<RoleDto> UpdateAsync(int id, UpdateRoleDto dto)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                throw new System.Exception("Role not found");

            // 检查角色名是否已被其他角色使用
            var existingRole = await _roleRepository.GetByNameAsync(dto.Name);
            if (existingRole != null && existingRole.Id != id)
                throw new System.Exception("Role name already exists");

            // 更新角色信息
            role.Name = dto.Name;
            role.Description = dto.Description;
            role.IsActive = dto.IsActive;

            // 更新权限
            var permissions = new List<Permission>();
            if (dto.PermissionIds != null && dto.PermissionIds.Count > 0)
            {
                foreach (var permissionId in dto.PermissionIds)
                {
                    var permission = await _permissionRepository.GetByIdAsync(permissionId);
                    if (permission != null)
                    {
                        permissions.Add(permission);
                    }
                }
            }
            role.Permissions = permissions;

            await _roleRepository.UpdateAsync(role);
            return _mapper.Map<RoleDto>(role);
        }

        public async Task DeleteAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                throw new System.Exception("Role not found");

            await _roleRepository.DeleteAsync(id);
        }
    }
}
