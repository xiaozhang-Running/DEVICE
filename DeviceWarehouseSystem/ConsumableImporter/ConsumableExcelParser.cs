using OfficeOpenXml;

namespace ConsumableImporter;

public class ConsumableExcelParser
{
    public List<ConsumableData> Parse(string filePath)
    {
        var consumables = new List<ConsumableData>();
        
        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        
        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            // 尝试获取"耗材"Sheet，如果不存在则使用第一个Sheet
            var worksheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name == "耗材") ?? package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.Rows;
            var colCount = worksheet.Dimension.Columns;
            
            Console.WriteLine($"Excel文件信息: {rowCount}行, {colCount}列");
            Console.WriteLine($"当前Sheet: {worksheet.Name}");
            
            for (int row = 1; row <= Math.Min(5, rowCount); row++)
            {
                Console.WriteLine($"第{row}行: ");
                for (int col = 1; col <= Math.Min(8, colCount); col++)
                {
                    var cellValue = worksheet.Cells[row, col].Text?.Trim() ?? "";
                    Console.WriteLine($"  列{col}: {cellValue}");
                }
            }
            
            for (int row = 2; row <= rowCount; row++)
            {
                var name = worksheet.Cells[row, 2].Text?.Trim();
                var brand = worksheet.Cells[row, 3].Text?.Trim();
                var model = worksheet.Cells[row, 4].Text?.Trim();
                var quantityText = worksheet.Cells[row, 5].Text?.Trim(); // 原始总数列
                var usedQuantityText = worksheet.Cells[row, 6].Text?.Trim(); // 已使用数列
                var remainingQuantityText = worksheet.Cells[row, 7].Text?.Trim(); // 剩余数量列
                var unit = worksheet.Cells[row, 8].Text?.Trim(); // 单位列
                
                if (string.IsNullOrEmpty(name) || name.ToLower() == "名称" || name.ToLower() == "name")
                    continue;
                
                int originalQuantity = int.TryParse(quantityText, out int q) ? q : 0;
                int usedQuantity = int.TryParse(usedQuantityText, out int u) ? u : 0;
                int remainingQuantity = int.TryParse(remainingQuantityText, out int r) ? r : originalQuantity - usedQuantity;
                
                if (originalQuantity > 0)
                {
                    consumables.Add(new ConsumableData
                    {
                        Name = name,
                        Brand = brand,
                        Model = model,
                        OriginalQuantity = originalQuantity,
                        UsedQuantity = usedQuantity,
                        RemainingQuantity = remainingQuantity,
                        Unit = unit,
                        Company = "",
                        Location = "",
                        Remark = ""
                    });
                }
            }
        }
        
        return consumables;
    }
}

public class ConsumableData
{
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int OriginalQuantity { get; set; }
    public int UsedQuantity { get; set; }
    public int RemainingQuantity { get; set; }
    public string? Unit { get; set; }
    public string? Company { get; set; }
    public string? Location { get; set; }
    public string? Remark { get; set; }
}