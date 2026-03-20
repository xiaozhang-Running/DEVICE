using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceWarehouse.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserActivityLogService _activityLogService;

        public UsersController(IUserService userService, IUserActivityLogService activityLogService)
        {
            _userService = userService;
            _activityLogService = activityLogService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var result = await _userService.LoginAsync(loginDto);
                
                // 记录登录日志
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
                await _activityLogService.CreateAsync(result.User.Id, "Login", $"用户 {result.User.Username} 登录成功", ipAddress, userAgent);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto userDto)
        {
            try
            {
                var user = await _userService.CreateAsync(userDto);
                
                // 记录操作日志
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
                await _activityLogService.CreateAsync(1, "Create", $"创建用户: {user.Username}", ipAddress, userAgent);
                
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateUserDto userDto)
        {
            try
            {
                var user = await _userService.UpdateAsync(id, userDto);
                
                // 记录操作日志
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim != null)
                {
                    var userId = int.Parse(userIdClaim.Value);
                    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                    var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
                    await _activityLogService.CreateAsync(userId, "Update", $"更新用户: {user.Username}", ipAddress, userAgent);
                }
                
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _userService.DeleteAsync(id);
                
                // 记录操作日志
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim != null)
                {
                    var userId = int.Parse(userIdClaim.Value);
                    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                    var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
                    await _activityLogService.CreateAsync(userId, "Delete", $"删除用户 (ID: {id})", ipAddress, userAgent);
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                await _userService.ChangePasswordAsync(id, changePasswordDto);
                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("role/{role}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetByRole(string role)
        {
            var users = await _userService.GetByRoleAsync(role);
            return Ok(users);
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                await _userService.ResetPasswordAsync(dto.Email);
                return Ok(new { message = "Password reset link sent to your email" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("reset-password-with-token")]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPasswordWithToken([FromBody] PasswordResetDto dto)
        {
            try
            {
                await _userService.ResetPasswordWithTokenAsync(dto.Token, dto.NewPassword);
                return Ok(new { message = "Password reset successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("bulk-create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> BulkCreate([FromBody] IEnumerable<CreateUserDto> dtos)
        {
            try
            {
                await _userService.BulkCreateAsync(dtos);
                return Ok(new { message = "Users created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("bulk-delete")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> BulkDelete([FromBody] IEnumerable<int> userIds)
        {
            try
            {
                await _userService.BulkDeleteAsync(userIds);
                return Ok(new { message = "Users deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("bulk-update-status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> BulkUpdateStatus([FromBody] dynamic data)
        {
            try
            {
                var userIds = ((IEnumerable<int>)data.userIds).ToList();
                var isActive = (bool)data.isActive;
                await _userService.BulkUpdateStatusAsync(userIds, isActive);
                return Ok(new { message = "Users status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/lock")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> LockUser(int id)
        {
            try
            {
                await _userService.LockUserAsync(id);
                return Ok(new { message = "User locked successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/unlock")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UnlockUser(int id)
        {
            try
            {
                await _userService.UnlockUserAsync(id);
                return Ok(new { message = "User unlocked successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}