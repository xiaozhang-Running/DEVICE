using Microsoft.EntityFrameworkCore;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Infrastructure.Data;

var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionsBuilder.UseSqlServer("Server=localhost;Database=DeviceWarehouse;Trusted_Connection=True;TrustServerCertificate=True");

using (var context = new ApplicationDbContext(optionsBuilder.Options))
{
    // 检查是否已有数据
    if (context.Consumables.Any())
    {
        Console.WriteLine("数据库中已有耗材数据");
        var existingCount = context.Consumables.Count();
        Console.WriteLine($"当前耗材数量: {existingCount}");
        
        // 显示前5条数据
        var top5 = context.Consumables.Take(5).ToList();
        Console.WriteLine("\n前5条耗材数据:");
        foreach (var item in top5)
        {
            Console.WriteLine($"ID: {item.Id}, 名称: {item.Name}, 品牌: {item.Brand}, 剩余数量: {item.RemainingQuantity}");
        }
        return;
    }
    
    // 添加测试数据
    var consumables = new List<Consumable>
    {
        new Consumable
        {
            Name = "打印纸",
            Brand = "得力",
            ModelSpecification = "A4 70g",
            TotalQuantity = 100,
            OriginalQuantity = 100,
            UsedQuantity = 20,
            RemainingQuantity = 80,
            Unit = "包",
            Location = "主仓库",
            Company = "博拉尔特",
            Remark = "日常办公用纸",
            CreatedAt = DateTime.Now
        },
        new Consumable
        {
            Name = "墨盒",
            Brand = "惠普",
            ModelSpecification = "HP-803",
            TotalQuantity = 50,
            OriginalQuantity = 50,
            UsedQuantity = 15,
            RemainingQuantity = 35,
            Unit = "个",
            Location = "主仓库",
            Company = "博拉尔特",
            Remark = "打印机用墨盒",
            CreatedAt = DateTime.Now
        },
        new Consumable
        {
            Name = "订书钉",
            Brand = "得力",
            ModelSpecification = "24/6",
            TotalQuantity = 200,
            OriginalQuantity = 200,
            UsedQuantity = 50,
            RemainingQuantity = 150,
            Unit = "盒",
            Location = "主仓库",
            Company = "博拉尔特",
            Remark = "办公用订书钉",
            CreatedAt = DateTime.Now
        },
        new Consumable
        {
            Name = "文件夹",
            Brand = "晨光",
            ModelSpecification = "A4蓝色",
            TotalQuantity = 300,
            OriginalQuantity = 300,
            UsedQuantity = 80,
            RemainingQuantity = 220,
            Unit = "个",
            Location = "主仓库",
            Company = "博拉尔特",
            Remark = "文件归档用",
            CreatedAt = DateTime.Now
        },
        new Consumable
        {
            Name = "胶水",
            Brand = "得力",
            ModelSpecification = "固体胶",
            TotalQuantity = 100,
            OriginalQuantity = 100,
            UsedQuantity = 30,
            RemainingQuantity = 70,
            Unit = "支",
            Location = "主仓库",
            Company = "博拉尔特",
            Remark = "办公用胶水",
            CreatedAt = DateTime.Now
        }
    };
    
    context.Consumables.AddRange(consumables);
    context.SaveChanges();
    
    Console.WriteLine($"成功添加 {consumables.Count} 条耗材数据");
}
