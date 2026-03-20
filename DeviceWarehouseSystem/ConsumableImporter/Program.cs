using ConsumableImporter;

Console.WriteLine("耗材数据管理工具");
Console.WriteLine("================");
Console.WriteLine("1. 清空耗材数据");
Console.WriteLine("2. 导入Excel文件");
Console.WriteLine("3. 退出");

Console.Write("请选择操作: ");
var choice = Console.ReadLine();

if (choice == "1")
{
    Console.Write("是否确认清空所有耗材数据？(y/n): ");
    var confirm = Console.ReadLine();

    if (confirm?.ToLower() != "y")
    {
        Console.WriteLine("已取消操作");
        return;
    }

    Console.WriteLine("正在清空耗材数据...");

    try
    {
        using var context = new ConsumableDbContext();
        
        var count = context.Consumables.Count();
        context.Consumables.RemoveRange(context.Consumables);
        context.SaveChanges();
        
        Console.WriteLine($"成功清空 {count} 条耗材数据");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"清空失败: {ex.Message}");
        Console.WriteLine($"详细错误: {ex.StackTrace}");
    }
}
else if (choice == "2")
{
    var filePath = args.Length > 0 ? args[0] : "";

    if (string.IsNullOrEmpty(filePath))
    {
        Console.Write("请输入Excel文件路径: ");
        filePath = Console.ReadLine() ?? "";
    }

    if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
    {
        Console.WriteLine("错误: 文件不存在");
        return;
    }

    Console.WriteLine("正在解析Excel文件...");
    var parser = new ConsumableExcelParser();
    var consumables = parser.Parse(filePath);

    Console.WriteLine($"成功解析 {consumables.Count} 条耗材数据");

    if (consumables.Count == 0)
    {
        Console.WriteLine("没有找到可导入的数据");
        return;
    }

    Console.WriteLine("\n数据预览:");
    Console.WriteLine("序号\t名称\t品牌\t型号\t数量\t单位\t公司\t仓库\t备注");
    for (int i = 0; i < Math.Min(5, consumables.Count); i++)
    {
        var item = consumables[i];
        Console.WriteLine($"{i + 1}\t{item.Name}\t{item.Brand}\t{item.Model}\t{item.OriginalQuantity}\t{item.Unit}\t{item.Company}\t{item.Location}\t{item.Remark}");
    }

    if (consumables.Count > 5)
    {
        Console.WriteLine($"... 还有 {consumables.Count - 5} 条数据");
    }

    Console.Write("\n是否确认导入到数据库？(y/n): ");
    var confirm = Console.ReadLine();

    if (confirm?.ToLower() != "y")
    {
        Console.WriteLine("已取消导入");
        return;
    }

    Console.WriteLine("正在导入数据到数据库...");

    try
    {
        using var context = new ConsumableDbContext();
        
        var successCount = 0;
        var duplicateCount = 0;
        
        foreach (var item in consumables)
        {
            var existing = context.Consumables
                .FirstOrDefault(c => c.Name == item.Name && 
                                     c.Brand == item.Brand && 
                                     c.ModelSpecification == item.Model);
            
            if (existing != null)
            {
                existing.TotalQuantity = item.OriginalQuantity;
                existing.UsedQuantity = item.UsedQuantity;
                existing.RemainingQuantity = item.RemainingQuantity;
                existing.Unit = item.Unit;
                existing.Company = item.Company;
                existing.Location = item.Location;
                existing.Remark = item.Remark;
                existing.UpdatedAt = DateTime.Now;
                duplicateCount++;
                Console.WriteLine($"更新数据: {item.Name} - {item.Brand} - {item.Model}");
            }
            else
            {
                var entity = new ConsumableEntity
                {
                    Name = item.Name,
                    Brand = item.Brand,
                    ModelSpecification = item.Model,
                    TotalQuantity = item.OriginalQuantity,
                    UsedQuantity = item.UsedQuantity,
                    RemainingQuantity = item.RemainingQuantity,
                    Unit = item.Unit,
                    Company = item.Company,
                    Location = item.Location,
                    Remark = item.Remark,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                
                context.Consumables.Add(entity);
                successCount++;
            }
        }
        
        context.SaveChanges();
        
        Console.WriteLine($"\n导入完成!");
        Console.WriteLine($"成功导入: {successCount} 条");
        Console.WriteLine($"更新数据: {duplicateCount} 条");
        Console.WriteLine($"总计处理: {consumables.Count} 条");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n导入失败: {ex.Message}");
        Console.WriteLine($"详细错误: {ex.StackTrace}");
    }
}
else if (choice == "3")
{
    Console.WriteLine("退出程序...");
    return;
}
else
{
    Console.WriteLine("无效的选择");
    return;
}

Console.WriteLine("\n操作完成，按任意键退出...");
Console.ReadKey();