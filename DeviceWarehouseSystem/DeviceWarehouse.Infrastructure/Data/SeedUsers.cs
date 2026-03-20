using DeviceWarehouse.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DeviceWarehouse.Infrastructure.Data
{
    public static class SeedUsers
    {
        public static void Seed(ApplicationDbContext context)
        {
            // 先创建角色
            if (context.Roles.Count() == 0)
            {
                var roles = new List<Role>
                {
                    new Role
                    {
                        Name = "Admin",
                        Description = "管理员",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new Role
                    {
                        Name = "User",
                        Description = "普通用户",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new Role
                    {
                        Name = "Warehouse",
                        Description = "库管",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    }
                };

                context.Roles.AddRange(roles);
                context.SaveChanges();
            }

            // 再创建用户
            if (context.Users.Count() == 0)
            {
                var users = new List<User>
                {
                    new User
                    {
                        Username = "admin",
                        PasswordHash = HashPassword("admin123"),
                        Email = "admin@example.com",
                        FullName = "管理员",
                        Role = "Admin",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new User
                    {
                        Username = "user",
                        PasswordHash = HashPassword("user123"),
                        Email = "user@example.com",
                        FullName = "普通用户",
                        Role = "User",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    }
                };

                context.Users.AddRange(users);
                context.SaveChanges();
            }
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
