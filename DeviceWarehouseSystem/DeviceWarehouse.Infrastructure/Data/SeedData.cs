using DeviceWarehouse.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DeviceWarehouse.Infrastructure.Data
{
    public static class SeedData
    {
        public static void Seed(ApplicationDbContext context)
        {
            // 先创建角色
            if (context.Roles.Count() == 0) {
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
                        Name = "Operator",
                        Description = "操作员",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new Role
                    {
                        Name = "Guest",
                        Description = "游客",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    }
                };

                context.Roles.AddRange(roles);
                context.SaveChanges();
            }

            // 再创建用户
            if (context.Users.Count() == 0) {
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
                        Username = "operator",
                        PasswordHash = HashPassword("operator123"),
                        Email = "operator@example.com",
                        FullName = "操作员",
                        Role = "Operator",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new User
                    {
                        Username = "guest",
                        PasswordHash = HashPassword("guest123"),
                        Email = "guest@example.com",
                        FullName = "游客",
                        Role = "Guest",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    }
                };

                context.Users.AddRange(users);
                context.SaveChanges();
            }

            // 创建专用设备
            if (context.SpecialEquipments.Count() == 0)
            {
                var specialEquipments = new List<SpecialEquipment>
                {
                    new SpecialEquipment
                    {
                        SortOrder = 1,
                        DeviceType = DeviceWarehouse.Domain.Enums.DeviceType.SpecialDevice,
                        DeviceName = "高精度示波器",
                        DeviceCode = "KEY-2023-001",
                        Brand = "Keysight",
                        Model = "DSO-X 3024T",
                        SerialNumber = "KEY-2023-001",
                        Specification = "4通道，200MHz带宽",
                        Quantity = 1,
                        Unit = "台",
                        // 不再设置 ImageUrl 字段，使用 Images 集合
                        DeviceStatus = DeviceWarehouse.Domain.Enums.DeviceStatus.Normal,
                        UseStatus = DeviceWarehouse.Domain.Enums.UsageStatus.InUse,
                        Status = "正常",
                        Company = "Keysight",
                        Location = "实验楼301室",
                        ProjectName = "研发项目A",
                        Remark = "用于电子电路测试",
                        CreatedAt = DateTime.Now
                    },
                    new SpecialEquipment
                    {
                        SortOrder = 2,
                        DeviceType = DeviceWarehouse.Domain.Enums.DeviceType.SpecialDevice,
                        DeviceName = "频谱分析仪",
                        DeviceCode = "R&S-2023-001",
                        Brand = "Rohde & Schwarz",
                        Model = "FSV30",
                        SerialNumber = "R&S-2023-001",
                        Specification = "30GHz",
                        Quantity = 1,
                        Unit = "台",
                        // 不再设置 ImageUrl 字段，使用 Images 集合
                        DeviceStatus = DeviceWarehouse.Domain.Enums.DeviceStatus.Normal,
                        UseStatus = DeviceWarehouse.Domain.Enums.UsageStatus.InUse,
                        Status = "正常",
                        Company = "Rohde & Schwarz",
                        Location = "实验楼302室",
                        ProjectName = "研发项目B",
                        Remark = "用于信号分析",
                        CreatedAt = DateTime.Now
                    }
                };

                context.SpecialEquipments.AddRange(specialEquipments);
                context.SaveChanges();
            }

            // 创建通用设备
            if (context.GeneralEquipments.Count() == 0)
            {
                var generalEquipments = new List<GeneralEquipment>
                {
                    new GeneralEquipment
                    {
                        SortOrder = 1,
                        DeviceType = DeviceWarehouse.Domain.Enums.DeviceType.GeneralDevice,
                        DeviceName = "笔记本电脑",
                        DeviceCode = "DELL-2023-001",
                        Brand = "Dell",
                        Model = "XPS 13",
                        SerialNumber = "DELL-2023-001",
                        Specification = "i7-1165G7, 16GB RAM, 512GB SSD",
                        Quantity = 1,
                        Unit = "台",
                        // 不再设置 ImageUrl 字段，使用 Images 集合
                        DeviceStatus = DeviceWarehouse.Domain.Enums.DeviceStatus.Normal,
                        UseStatus = DeviceWarehouse.Domain.Enums.UsageStatus.InUse,
                        Status = "正常",
                        Company = "Dell",
                        Location = "行政楼201室",
                        ProjectName = "行政办公",
                        Remark = "办公用笔记本电脑",
                        CreatedAt = DateTime.Now
                    },
                    new GeneralEquipment
                    {
                        SortOrder = 2,
                        DeviceType = DeviceWarehouse.Domain.Enums.DeviceType.GeneralDevice,
                        DeviceName = "打印机",
                        DeviceCode = "HP-2023-001",
                        Brand = "HP",
                        Model = "LaserJet Pro M404n",
                        SerialNumber = "HP-2023-001",
                        Specification = "黑白激光打印机",
                        Quantity = 1,
                        Unit = "台",
                        // 不再设置 ImageUrl 字段，使用 Images 集合
                        DeviceStatus = DeviceWarehouse.Domain.Enums.DeviceStatus.Normal,
                        UseStatus = DeviceWarehouse.Domain.Enums.UsageStatus.InUse,
                        Status = "正常",
                        Company = "HP",
                        Location = "行政楼202室",
                        ProjectName = "行政办公",
                        Remark = "办公用打印机",
                        CreatedAt = DateTime.Now
                    }
                };

                context.GeneralEquipments.AddRange(generalEquipments);
                context.SaveChanges();
            }

            // 创建耗材
            if (context.Consumables.Count() == 0)
            {
                var consumables = new List<Consumable>
                {
                    new Consumable
                    {
                        Name = "打印纸",
                        Brand = "得力",
                        ModelSpecification = "70g",
                        TotalQuantity = 5000,
                        OriginalQuantity = 5000,
                        UsedQuantity = 1000,
                        RemainingQuantity = 4000,
                        Unit = "包",
                        Company = "得力文具",
                        Status = "正常",
                        Location = "仓库A区",
                        Remark = "办公用打印纸",
                        Image = "",
                        CreatedAt = DateTime.Now
                    },
                    new Consumable
                    {
                        Name = "墨盒",
                        Brand = "HP",
                        ModelSpecification = "黑色",
                        TotalQuantity = 50,
                        OriginalQuantity = 50,
                        UsedQuantity = 10,
                        RemainingQuantity = 40,
                        Unit = "个",
                        Company = "HP",
                        Status = "正常",
                        Location = "仓库B区",
                        Remark = "打印机墨盒",
                        Image = "",
                        CreatedAt = DateTime.Now
                    }
                };

                context.Consumables.AddRange(consumables);
                context.SaveChanges();
            }

            // 创建原材料
            if (context.RawMaterials.Count() == 0)
            {
                var rawMaterials = new List<RawMaterial>
                {
                    new RawMaterial
                    {
                        SortOrder = 1,
                        ProductName = "电阻",
                        Specification = "1kΩ",
                        TotalQuantity = 10000,
                        UsedQuantity = 2000,
                        RemainingQuantity = 8000,
                        Unit = "个",
                        Company = "国巨电子",
                        Supplier = "国巨",
                        Remark = "电子元器件",
                        CreatedAt = DateTime.Now
                    },
                    new RawMaterial
                    {
                        SortOrder = 2,
                        ProductName = "电容",
                        Specification = "10μF",
                        TotalQuantity = 5000,
                        UsedQuantity = 1000,
                        RemainingQuantity = 4000,
                        Unit = "个",
                        Company = "村田制作所",
                        Supplier = "村田",
                        Remark = "电子元器件",
                        CreatedAt = DateTime.Now
                    }
                };

                context.RawMaterials.AddRange(rawMaterials);
                context.SaveChanges();
            }

            // 创建项目入库
            if (context.ProjectInbounds.Count() == 0)
            {
                var projectInbounds = new List<ProjectInbound>
                {
                    new ProjectInbound
                    {
                        InboundNumber = "IN-2023-001",
                        InboundDate = DateTime.Now.AddMonths(-2),
                        ProjectName = "研发项目A",
                        Supplier = "供应商A",
                        ContactPhone = "13800138000",
                        Status = "已完成",
                        IsCompleted = true,
                        CreatedAt = DateTime.Now.AddMonths(-2),
                        CompletedAt = DateTime.Now.AddMonths(-1),
                        Remark = "研发项目A的设备入库"
                    },
                    new ProjectInbound
                    {
                        InboundNumber = "IN-2023-002",
                        InboundDate = DateTime.Now.AddMonths(-1),
                        ProjectName = "研发项目B",
                        Supplier = "供应商B",
                        ContactPhone = "13900139000",
                        Status = "进行中",
                        IsCompleted = false,
                        CreatedAt = DateTime.Now.AddMonths(-1),
                        Remark = "研发项目B的设备入库"
                    }
                };

                context.ProjectInbounds.AddRange(projectInbounds);
                context.SaveChanges();
            }

            // 创建项目出库
            if (context.ProjectOutbounds.Count() == 0)
            {
                var projectOutbounds = new List<ProjectOutbound>
                {
                    new ProjectOutbound
                    {
                        OutboundNumber = "OUT-2023-001",
                        OutboundDate = DateTime.Now.AddMonths(-3),
                        ProjectName = "项目X",
                        Recipient = "王五",
                        ContactPhone = "13700137000",
                        IsCompleted = true,
                        CreatedAt = DateTime.Now.AddMonths(-3),
                        CompletedAt = DateTime.Now.AddMonths(-2),
                        Remark = "项目X的设备出库"
                    },
                    new ProjectOutbound
                    {
                        OutboundNumber = "OUT-2023-002",
                        OutboundDate = DateTime.Now.AddMonths(-1),
                        ProjectName = "项目Y",
                        Recipient = "赵六",
                        ContactPhone = "13600136000",
                        IsCompleted = false,
                        CreatedAt = DateTime.Now.AddMonths(-1),
                        Remark = "项目Y的设备出库"
                    }
                };

                context.ProjectOutbounds.AddRange(projectOutbounds);
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