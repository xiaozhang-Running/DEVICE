using AutoMapper;
using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace DeviceWarehouse.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IMapper mapper, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<UserDto> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto> CreateAsync(CreateUserDto userDto)
        {
            // Check if username already exists
            if (await _userRepository.UsernameExistsAsync(userDto.Username))
            {
                throw new Exception("Username already exists");
            }

            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(userDto.Email))
            {
                throw new Exception("Email already exists");
            }

            // Hash password
            var passwordHash = HashPassword(userDto.Password);

            var user = new User
            {
                Username = userDto.Username,
                PasswordHash = passwordHash,
                Email = userDto.Email,
                FullName = userDto.FullName,
                Role = userDto.Role,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _userRepository.AddAsync(user);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> UpdateAsync(int id, UpdateUserDto userDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            user.Email = userDto.Email;
            user.FullName = userDto.FullName;
            user.Role = userDto.Role;
            user.IsActive = userDto.IsActive;
            user.UpdatedAt = DateTime.Now;

            await _userRepository.UpdateAsync(user);
            return _mapper.Map<UserDto>(user);
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            await _userRepository.DeleteAsync(user);
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            if (string.IsNullOrEmpty(loginDto.Username) || string.IsNullOrEmpty(loginDto.Password))
            {
                throw new Exception("Username and password are required");
            }

            var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
            if (user == null)
            {
                throw new Exception("Invalid username or password");
            }

            if (!VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                throw new Exception("Invalid username or password");
            }

            var token = GenerateJwtToken(user);
            return new LoginResponseDto
            {
                Token = token,
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            if (!VerifyPassword(changePasswordDto.OldPassword, user.PasswordHash))
            {
                throw new Exception("Old password is incorrect");
            }

            user.PasswordHash = HashPassword(changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.Now;
            await _userRepository.UpdateAsync(user);
        }

        public async Task<IEnumerable<UserDto>> GetByRoleAsync(string role)
        {
            var users = await _userRepository.GetByRoleAsync(role);
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task ResetPasswordAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // 生成密码重置令牌
            var token = GenerateResetToken();
            // 这里可以发送邮件，包含重置密码的链接
            // 实际项目中，应该将令牌存储在数据库中，并设置过期时间
            
            // 模拟发送邮件
            Console.WriteLine($"Password reset token for {email}: {token}");
        }

        public async Task ResetPasswordWithTokenAsync(string token, string newPassword)
        {
            // 验证令牌
            // 实际项目中，应该从数据库中验证令牌的有效性
            
            // 假设令牌有效，更新密码
            // 这里简化处理，实际项目中应该根据令牌找到对应的用户
            
            // 模拟更新密码
            Console.WriteLine($"Resetting password with token: {token}");
            await Task.CompletedTask;
        }

        public async Task BulkCreateAsync(IEnumerable<CreateUserDto> userDtos)
        {
            foreach (var dto in userDtos)
            {
                // 检查用户名是否已存在
                if (await _userRepository.UsernameExistsAsync(dto.Username))
                {
                    throw new Exception($"Username {dto.Username} already exists");
                }

                // 检查邮箱是否已存在
                if (await _userRepository.EmailExistsAsync(dto.Email))
                {
                    throw new Exception($"Email {dto.Email} already exists");
                }

                // 哈希密码
                var passwordHash = HashPassword(dto.Password);

                var user = new User
                {
                    Username = dto.Username,
                    PasswordHash = passwordHash,
                    Email = dto.Email,
                    FullName = dto.FullName,
                    Role = dto.Role,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                await _userRepository.AddAsync(user);
            }
        }

        public async Task BulkDeleteAsync(IEnumerable<int> userIds)
        {
            foreach (var id in userIds)
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user != null)
                {
                    await _userRepository.DeleteAsync(user);
                }
            }
        }

        public async Task BulkUpdateStatusAsync(IEnumerable<int> userIds, bool isActive)
        {
            foreach (var id in userIds)
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user != null)
                {
                    user.IsActive = isActive;
                    user.UpdatedAt = DateTime.Now;
                    await _userRepository.UpdateAsync(user);
                }
            }
        }

        public async Task LockUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            user.IsLockedOut = true;
            user.LockoutEnd = DateTime.Now.AddHours(24); // 锁定24小时
            user.UpdatedAt = DateTime.Now;
            await _userRepository.UpdateAsync(user);
        }

        public async Task UnlockUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            user.IsLockedOut = false;
            user.LockoutEnd = null;
            user.FailedLoginAttempts = 0;
            user.UpdatedAt = DateTime.Now;
            await _userRepository.UpdateAsync(user);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "default-secret-key-for-development");
            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                    new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateResetToken()
        {
            var randomBytes = new byte[32];
            System.Security.Cryptography.RandomNumberGenerator.Fill(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}