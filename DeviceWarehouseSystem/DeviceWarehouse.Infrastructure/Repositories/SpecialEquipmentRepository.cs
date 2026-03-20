using Dapper;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Enums;
using DeviceWarehouse.Domain.Interfaces;
using DeviceWarehouse.Infrastructure.Data;
using DeviceWarehouse.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DeviceWarehouse.Infrastructure.Repositories;

public class SpecialEquipmentRepository : ISpecialEquipmentRepository
{
    private readonly ApplicationDbContext _context;

    public SpecialEquipmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SpecialEquipment?> GetByIdAsync(int id)
    {
        return await _context.SpecialEquipments
            .Include(d => d.Inventory)
            .Include(d => d.Images)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<SpecialEquipment?> GetByCodeAsync(string code)
    {
        return await _context.SpecialEquipments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DeviceCode == code);
    }

    public async Task<IEnumerable<SpecialEquipment>> GetAllAsync()
    {
        return await _context.SpecialEquipments
            .OrderBy(d => d.SortOrder)
            .ThenBy(d => d.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<SpecialEquipment>> GetByTypeAsync(DeviceType type)
    {
        return await _context.SpecialEquipments
            .Where(d => d.DeviceType == type)
            .Include(d => d.Inventory)
            .OrderBy(d => d.DeviceName == null ? string.Empty : d.DeviceName)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<SpecialEquipment> AddAsync(SpecialEquipment specialEquipment)
    {
        specialEquipment.CreatedAt = DateTime.Now;
        
        // 设置SortOrder为当前最大值+1
        var maxSortOrder = await _context.SpecialEquipments.MaxAsync(e => (int?)e.SortOrder) ?? 0;
        specialEquipment.SortOrder = maxSortOrder + 1;
        
        _context.SpecialEquipments.Add(specialEquipment);
        await _context.SaveChangesAsync();
        return specialEquipment;
    }

    public async Task UpdateAsync(SpecialEquipment specialEquipment)
    {
        // 由于EF Core的跟踪问题，我们不再处理Images集合
        // 图片的处理将在SpecialEquipmentService中通过其他方式实现
        
        // 更新基本属性
        await _context.SpecialEquipments
            .Where(s => s.Id == specialEquipment.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.SortOrder, specialEquipment.SortOrder)
                .SetProperty(s => s.DeviceType, specialEquipment.DeviceType)
                .SetProperty(s => s.DeviceName, specialEquipment.DeviceName)
                .SetProperty(s => s.DeviceCode, specialEquipment.DeviceCode)
                .SetProperty(s => s.Brand, specialEquipment.Brand)
                .SetProperty(s => s.Model, specialEquipment.Model)
                .SetProperty(s => s.SerialNumber, specialEquipment.SerialNumber)
                .SetProperty(s => s.Quantity, specialEquipment.Quantity)
                .SetProperty(s => s.Unit, specialEquipment.Unit)
                .SetProperty(s => s.DeviceStatus, specialEquipment.DeviceStatus)
                .SetProperty(s => s.UseStatus, specialEquipment.UseStatus)
                .SetProperty(s => s.Status, specialEquipment.Status)
                .SetProperty(s => s.Company, specialEquipment.Company)
                .SetProperty(s => s.Accessories, specialEquipment.Accessories)
                .SetProperty(s => s.Remark, specialEquipment.Remark)
                .SetProperty(s => s.RepairStatus, specialEquipment.RepairStatus)
                .SetProperty(s => s.RepairPerson, specialEquipment.RepairPerson)
                .SetProperty(s => s.RepairDate, specialEquipment.RepairDate)
                .SetProperty(s => s.FaultReason, specialEquipment.FaultReason)
                .SetProperty(s => s.Location, specialEquipment.Location)
                .SetProperty(s => s.ProjectName, specialEquipment.ProjectName)
                .SetProperty(s => s.ProjectTime, specialEquipment.ProjectTime)
                .SetProperty(s => s.UpdatedAt, DateTime.Now)
                .SetProperty(s => s.UpdatedBy, specialEquipment.UpdatedBy)
            );
    }

    public async Task DeleteAsync(int id)
    {
        // 使用ExecuteDeleteAsync直接删除，避免EF Core跟踪冲突
        await _context.SpecialEquipments
            .Where(s => s.Id == id)
            .ExecuteDeleteAsync();
    }

    public async Task<bool> ExistsAsync(string code)
    {
        return await _context.SpecialEquipments
            .AnyAsync(d => d.DeviceCode == code);
    }

    public async Task<IEnumerable<SpecialEquipment>> SearchAsync(string keyword)
    {
        var lowerKeyword = keyword.ToLower();
        return await _context.SpecialEquipments
            .Where(d => 
                (d.DeviceName != null && d.DeviceName.ToLower().Contains(lowerKeyword)) ||
                (d.DeviceCode != null && d.DeviceCode.ToLower().Contains(lowerKeyword)) ||
                (d.Brand != null && d.Brand.ToLower().Contains(lowerKeyword)) ||
                (d.Model != null && d.Model.ToLower().Contains(lowerKeyword)))
            .Include(d => d.Inventory)
            .OrderBy(d => d.DeviceName == null ? string.Empty : d.DeviceName)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<SpecialEquipment>> GetAvailableAsync(string? keyword = null)
    {
        try
        {
            var query = _context.SpecialEquipments
                .Where(e => e.Quantity > 0 && e.UseStatus != DeviceWarehouse.Domain.Enums.UsageStatus.InUse)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                // 放宽搜索条件，使用LIKE查询而不是精确匹配
                query = query.Where(e => 
                    (e.DeviceName != null && e.DeviceName.ToLower().Contains(lowerKeyword)) ||
                    (e.Brand != null && e.Brand.ToLower().Contains(lowerKeyword)) ||
                    (e.Model != null && e.Model.ToLower().Contains(lowerKeyword)));
                Console.WriteLine($"搜索专用设备，keyword: {keyword}, 小写: {lowerKeyword}");
            }

            var result = await query
                .OrderBy(e => e.DeviceName)
                .ToListAsync();
            Console.WriteLine($"专用设备搜索结果数量: {result.Count}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetAvailableAsync error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            
            // 尝试使用简化查询作为fallback
            try
            {
                var connection = _context.Database.GetDbConnection();
                bool wasOpen = connection.State == System.Data.ConnectionState.Open;
                if (!wasOpen)
                {
                    await connection.OpenAsync();
                }
                
                try
                {
                    // 首先检查数据库表结构，确定哪些列存在
                    var columns = new List<string>();
                    using (var schemaCommand = connection.CreateCommand())
                    {
                        schemaCommand.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SpecialEquipment'";
                        using (var schemaReader = await schemaCommand.ExecuteReaderAsync())
                        {
                            while (await schemaReader.ReadAsync())
                            {
                                columns.Add(schemaReader.GetString(0));
                            }
                        }
                    }
                    
                    // 构建SELECT语句，只包含存在的列
                    var selectColumns = new List<string> { "Id" };
                    if (columns.Contains("DeviceName")) selectColumns.Add("DeviceName");
                    if (columns.Contains("Brand")) selectColumns.Add("Brand");
                    if (columns.Contains("Model")) selectColumns.Add("Model");
                    if (columns.Contains("Quantity")) selectColumns.Add("Quantity");
                    if (columns.Contains("Unit")) selectColumns.Add("Unit");
                    if (columns.Contains("Location")) selectColumns.Add("Location");
                    if (columns.Contains("Company")) selectColumns.Add("Company");
                    if (columns.Contains("DeviceCode")) selectColumns.Add("DeviceCode");
                    if (columns.Contains("Accessories")) selectColumns.Add("Accessories");
                    if (columns.Contains("Remark")) selectColumns.Add("Remark");
                    if (columns.Contains("DeviceStatus")) selectColumns.Add("DeviceStatus");
                    if (columns.Contains("UseStatus")) selectColumns.Add("UseStatus");
                    
                    var whereClause = "WHERE Quantity > 0";
                    var parameters = new Dictionary<string, object>();
                    
                    // 添加UseStatus条件
                    if (columns.Contains("UseStatus"))
                    {
                        whereClause += " AND UseStatus != 1"; // 1 = InUse
                    }
                    
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        var keywordConditions = new List<string>();
                        if (columns.Contains("DeviceName")) keywordConditions.Add("DeviceName LIKE @Keyword");
                        if (columns.Contains("Brand")) keywordConditions.Add("Brand LIKE @Keyword");
                        if (columns.Contains("Model")) keywordConditions.Add("Model LIKE @Keyword");
                        
                        if (keywordConditions.Count > 0)
                        {
                            whereClause += " AND (" + string.Join(" OR ", keywordConditions) + ")";
                            parameters["@Keyword"] = $"%{keyword}%";
                        }
                    }
                    
                    var sql = $@"
                        SELECT {string.Join(", ", selectColumns)}
                        FROM SpecialEquipment
                        {whereClause}
                        {(columns.Contains("DeviceName") ? "ORDER BY DeviceName" : "")}
                    ";
                    
                    var equipments = new List<SpecialEquipment>();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        foreach (var param in parameters)
                        {
                            var dbParam = command.CreateParameter();
                            dbParam.ParameterName = param.Key;
                            dbParam.Value = param.Value;
                            command.Parameters.Add(dbParam);
                        }
                        
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var equipment = new SpecialEquipment
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    DeviceName = columns.Contains("DeviceName") && !reader.IsDBNull(reader.GetOrdinal("DeviceName")) ? reader.GetString(reader.GetOrdinal("DeviceName")) : string.Empty,
                                    Brand = columns.Contains("Brand") && !reader.IsDBNull(reader.GetOrdinal("Brand")) ? reader.GetString(reader.GetOrdinal("Brand")) : null,
                                    Model = columns.Contains("Model") && !reader.IsDBNull(reader.GetOrdinal("Model")) ? reader.GetString(reader.GetOrdinal("Model")) : null,
                                    Quantity = columns.Contains("Quantity") ? reader.GetInt32(reader.GetOrdinal("Quantity")) : 0,
                                    Unit = columns.Contains("Unit") && !reader.IsDBNull(reader.GetOrdinal("Unit")) ? reader.GetString(reader.GetOrdinal("Unit")) : null,
                                    Location = columns.Contains("Location") && !reader.IsDBNull(reader.GetOrdinal("Location")) ? reader.GetString(reader.GetOrdinal("Location")) : null,
                                    Company = columns.Contains("Company") && !reader.IsDBNull(reader.GetOrdinal("Company")) ? reader.GetString(reader.GetOrdinal("Company")) : null,
                                    DeviceCode = columns.Contains("DeviceCode") && !reader.IsDBNull(reader.GetOrdinal("DeviceCode")) ? reader.GetString(reader.GetOrdinal("DeviceCode")) : string.Empty,
                                    Accessories = columns.Contains("Accessories") && !reader.IsDBNull(reader.GetOrdinal("Accessories")) ? reader.GetString(reader.GetOrdinal("Accessories")) : null,
                                    Remark = columns.Contains("Remark") && !reader.IsDBNull(reader.GetOrdinal("Remark")) ? reader.GetString(reader.GetOrdinal("Remark")) : null,
                                    DeviceStatus = columns.Contains("DeviceStatus") ? (DeviceWarehouse.Domain.Enums.DeviceStatus)reader.GetInt32(reader.GetOrdinal("DeviceStatus")) : DeviceWarehouse.Domain.Enums.DeviceStatus.Normal,
                                    UseStatus = columns.Contains("UseStatus") ? (DeviceWarehouse.Domain.Enums.UsageStatus)reader.GetInt32(reader.GetOrdinal("UseStatus")) : DeviceWarehouse.Domain.Enums.UsageStatus.Unused
                                };
                                equipments.Add(equipment);
                            }
                        }
                    }
                    
                    Console.WriteLine($"Fallback查询专用设备结果数量: {equipments.Count}");
                    return equipments;
                }
                finally
                {
                    if (!wasOpen && connection.State == System.Data.ConnectionState.Open)
                    {
                        await connection.CloseAsync();
                    }
                }
            }
            catch (Exception fallbackEx)
            {
                Console.WriteLine($"Fallback query error: {fallbackEx.Message}");
                if (fallbackEx.InnerException != null)
                {
                    Console.WriteLine($"Fallback inner exception: {fallbackEx.InnerException.Message}");
                }
                return [];
            }
        }
    }

    // 新增：批量获取专用设备
    public async Task<IEnumerable<SpecialEquipment>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _context.SpecialEquipments
            .Where(e => ids.Contains(e.Id))
            .AsNoTracking()
            .ToListAsync();
    }

    // 新增：获取分页数据
    public async Task<(IEnumerable<SpecialEquipment> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false, DeviceStatus? deviceStatus = null, UsageStatus? useStatus = null, string? brand = null)
    {
        try
        {
            Console.WriteLine($"GetPagedAsync called with: pageNumber={pageNumber}, pageSize={pageSize}, keyword={keyword}, sortBy={sortBy}, sortDescending={sortDescending}, deviceStatus={deviceStatus}, useStatus={useStatus}, brand={brand}");
            
            var query = _context.SpecialEquipments.Include(d => d.Images).AsNoTracking();
            Console.WriteLine("Query created for SpecialEquipments with Images");

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(e => 
                    (e.DeviceName != null && e.DeviceName.ToLower().Contains(lowerKeyword)) ||
                    (e.DeviceCode != null && e.DeviceCode.ToLower().Contains(lowerKeyword)) ||
                    (e.Brand != null && e.Brand.ToLower().Contains(lowerKeyword)) ||
                    (e.Model != null && e.Model.ToLower().Contains(lowerKeyword)));
                Console.WriteLine($"Applied keyword filter: {keyword}");
            }

            // 应用设备状态筛选
            if (deviceStatus.HasValue)
            {
                query = query.Where(e => e.DeviceStatus == deviceStatus.Value);
                Console.WriteLine($"Applied device status filter: {deviceStatus.Value}");
            }

            // 应用使用状态筛选
            if (useStatus.HasValue)
            {
                query = query.Where(e => e.UseStatus == useStatus.Value);
                Console.WriteLine($"Applied use status filter: {useStatus.Value}");
            }

            // 应用品牌筛选
            if (!string.IsNullOrWhiteSpace(brand))
            {
                var lowerBrand = brand.ToLower();
                query = query.Where(e => e.Brand != null && e.Brand.ToLower().Contains(lowerBrand));
                Console.WriteLine($"Applied brand filter: {brand}");
            }

            // 排序
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                // 检查sortBy是否为有效的属性名
                var property = typeof(SpecialEquipment).GetProperty(sortBy);
                if (property != null)
                {
                    query = query.ApplySorting(sortBy, sortDescending);
                    Console.WriteLine($"Applied sorting: {sortBy}, descending={sortDescending}");
                }
                else
                {
                    // 如果sortBy不是有效的属性名，使用默认排序
                    query = query.OrderBy(e => e.DeviceName == null ? string.Empty : e.DeviceName);
                    Console.WriteLine("Applied default sorting by DeviceName");
                }
            }
            else
            {
                query = query.OrderBy(e => e.DeviceName == null ? string.Empty : e.DeviceName);
                Console.WriteLine("Applied default sorting by DeviceName");
            }

            // 分页
            var totalCount = await query.CountAsync();
            Console.WriteLine($"Total count: {totalCount}");
            
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            Console.WriteLine($"Items fetched: {items.Count}");

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            Console.WriteLine($"Total pages: {totalPages}");

            return (items, totalCount, pageNumber, pageSize, totalPages);
        }
        catch (Exception ex)
        {
            // 记录错误并返回空结果
            Console.WriteLine($"GetPagedAsync error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            return (Enumerable.Empty<SpecialEquipment>(), 0, pageNumber, pageSize, 0);
        }
    }

    // 新增：获取可用物品分页数据
    public async Task<(IEnumerable<SpecialEquipment> Items, int TotalCount)> GetAvailablePagedAsync(string? keyword, int pageNumber, int pageSize)
    {
        var connection = _context.Database.GetDbConnection();
        var parameters = new DynamicParameters();
        
        parameters.Add("Keyword", keyword);
        parameters.Add("PageNumber", pageNumber);
        parameters.Add("PageSize", pageSize);
        
        // 使用存储过程执行查询
        using var multi = await connection.QueryMultipleAsync("GetSpecialEquipmentAvailablePaged", parameters, commandType: System.Data.CommandType.StoredProcedure);
        
        var items = await multi.ReadAsync<SpecialEquipment>();
        var totalCount = await multi.ReadFirstAsync<int>();
        
        return (items, totalCount);
    }

    // 新增：执行原始SQL查询
    public async Task<List<dynamic>> ExecuteRawSqlAsync(string sql, object parameters)
    {
        var result = new List<dynamic>();
        
        // 使用DbContext的DatabaseFacade执行原始SQL
        var connection = _context.Database.GetDbConnection();
        
        try
        {
            // 确保连接已打开
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }
            
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 60; // 设置超时时间为60秒
            
            // 添加参数
            if (parameters != null)
            {
                var properties = parameters.GetType().GetProperties();
                foreach (var property in properties)
                {
                    var value = property.GetValue(parameters);
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@" + property.Name;
                    parameter.Value = value ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }
            }
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var expando = new System.Dynamic.ExpandoObject() as IDictionary<string, object?>;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    expando[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                result.Add(expando);
            }
        }
        catch
        {
            // 发生异常时不关闭连接，让DbContext管理连接生命周期
            throw;
        }
        
        return result;
    }

    // 新增：获取分组分页数据
    public async Task<(IEnumerable<SpecialEquipment> Items, int TotalCount)> GetGroupedPagedAsync(string? keyword, int pageNumber, int pageSize)
    {
        try
        {
            var query = _context.SpecialEquipments.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(e => 
                    (e.DeviceName != null && e.DeviceName.ToLower().Contains(lowerKeyword)) ||
                    (e.DeviceCode != null && e.DeviceCode.ToLower().Contains(lowerKeyword)) ||
                    (e.Brand != null && e.Brand.ToLower().Contains(lowerKeyword)) ||
                    (e.Model != null && e.Model.ToLower().Contains(lowerKeyword)));
            }

            // 先获取所有符合条件的设备
            var allItems = await query.ToListAsync();

            // 按设备名称、品牌、型号分组
            var groupedItems = allItems
                .GroupBy(d => new { d.DeviceName, d.Brand, d.Model })
                .Select(g => g.First()) // 每个组取第一个设备作为代表
                .OrderBy(g => g.DeviceName == null ? string.Empty : g.DeviceName)
                .ToList();

            // 计算总组数
            var totalGroups = groupedItems.Count;

            // 分页
            var pagedGroups = groupedItems
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 获取这些组对应的所有设备
            var resultItems = new List<SpecialEquipment>();
            foreach (var group in pagedGroups)
            {
                var groupDevices = allItems
                    .Where(d => d.DeviceName == group.DeviceName && 
                                d.Brand == group.Brand && 
                                d.Model == group.Model)
                    .OrderBy(d => d.DeviceCode)
                    .ToList();
                resultItems.AddRange(groupDevices);
            }

            return (resultItems, totalGroups);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetGroupedPagedAsync error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            return (new List<SpecialEquipment>(), 0);
        }
    }

    public async Task<int> CountByNameAsync(string deviceName)
    {
        return await _context.SpecialEquipments
            .Where(d => d.DeviceName == deviceName)
            .CountAsync();
    }

    // 新增：批量删除所有专用设备
    public async Task DeleteAllAsync()
    {
        try
        {
            Console.WriteLine("开始批量删除专用设备...");
            
            // 先删除所有关联的入库订单项目
            var inboundOrderItems = await _context.InboundOrderItems
                .ToListAsync();
            
            if (inboundOrderItems.Any())
            {
                Console.WriteLine($"找到 {inboundOrderItems.Count} 条关联的入库订单项目");
                _context.InboundOrderItems.RemoveRange(inboundOrderItems);
                await _context.SaveChangesAsync();
                Console.WriteLine($"删除了 {inboundOrderItems.Count} 条入库订单项目");
            } else {
                Console.WriteLine("没有找到关联的入库订单项目");
            }
            
            // 再删除所有关联的出库订单项目
            var outboundOrderItems = await _context.OutboundOrderItems
                .ToListAsync();
            
            if (outboundOrderItems.Any())
            {
                Console.WriteLine($"找到 {outboundOrderItems.Count} 条关联的出库订单项目");
                _context.OutboundOrderItems.RemoveRange(outboundOrderItems);
                await _context.SaveChangesAsync();
                Console.WriteLine($"删除了 {outboundOrderItems.Count} 条出库订单项目");
            } else {
                Console.WriteLine("没有找到关联的出库订单项目");
            }
            
            // 再删除所有关联的图片
            var images = await _context.Images
                .Where(i => i.SpecialEquipmentId != null)
                .ToListAsync();
            
            if (images.Any())
            {
                Console.WriteLine($"找到 {images.Count} 条关联的图片记录");
                _context.Images.RemoveRange(images);
                await _context.SaveChangesAsync();
                Console.WriteLine($"删除了 {images.Count} 条图片记录");
            } else {
                Console.WriteLine("没有找到关联的图片记录");
            }
            
            // 再删除所有关联的库存记录
            var inventories = await _context.Inventories
                .Where(i => i.SpecialEquipmentId != null)
                .ToListAsync();
            
            if (inventories.Any())
            {
                Console.WriteLine($"找到 {inventories.Count} 条关联的库存记录");
                _context.Inventories.RemoveRange(inventories);
                await _context.SaveChangesAsync();
                Console.WriteLine($"删除了 {inventories.Count} 条库存记录");
            } else {
                Console.WriteLine("没有找到关联的库存记录");
            }
            
            // 最后删除所有专用设备
            var equipments = await _context.SpecialEquipments.ToListAsync();
            Console.WriteLine($"找到 {equipments.Count} 台专用设备");
            
            if (equipments.Any())
            {
                _context.SpecialEquipments.RemoveRange(equipments);
                await _context.SaveChangesAsync();
                Console.WriteLine($"删除了 {equipments.Count} 台专用设备");
            } else {
                Console.WriteLine("没有找到专用设备");
            }
            
            // 再次检查是否还有专用设备
            var remainingEquipments = await _context.SpecialEquipments.ToListAsync();
            Console.WriteLine($"删除后剩余 {remainingEquipments.Count} 台专用设备");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"批量删除专用设备失败: {ex.Message}");
            throw;
        }
    }
}
