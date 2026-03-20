using System;
using System.Collections.Generic;

namespace DeviceWarehouse.Application.DTOs
{
    public class RoleDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
    }

    public class CreateRoleDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public List<int> PermissionIds { get; set; } = new List<int>();
    }

    public class UpdateRoleDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public List<int> PermissionIds { get; set; } = new List<int>();
    }

    public class PermissionDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreatePermissionDto
    {
        public required string Name { get; set; }
        public required string Code { get; set; }
        public string? Description { get; set; }
    }

    public class UpdatePermissionDto
    {
        public required string Name { get; set; }
        public required string Code { get; set; }
        public string? Description { get; set; }
    }

    public class UserActivityLogDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public required string ActivityType { get; set; }
        public required string ActivityDescription { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDto? User { get; set; }
    }

    public class ResetPasswordDto
    {
        public required string Email { get; set; }
    }

    public class PasswordResetDto
    {
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
    }
}
