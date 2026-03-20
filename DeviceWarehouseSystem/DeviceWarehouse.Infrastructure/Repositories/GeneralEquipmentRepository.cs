using Dapper;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Enums;
using DeviceWarehouse.Domain.Interfaces;
using DeviceWarehouse.Infrastructure.Data;
using DeviceWarehouse.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DeviceWarehouse.Infrastructure.Repositories;

public class GeneralEquipmentRepository : IGeneralEquipmentRepository
{
    private readonly ApplicationDbContext _context;

    public GeneralEquipmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GeneralEquipment?> GetByIdAsync(int id)
    {
        return await _context.GeneralEquipments
            .Include(d => d.Inventory)
            .Include(d => d.Images)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<GeneralEquipment?> GetByCodeAsync(string code)
    {
        return await _context.GeneralEquipments
            .FirstOrDefaultAsync(d => d.DeviceCode == code);
    }

    public async Task<IEnumerable<GeneralEquipment>> GetAllAsync()
    {
        return await _context.GeneralEquipments
            .FromSqlRaw("SELECT * FROM GeneralEquipment ORDER BY SortOrder, Id")
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<GeneralEquipment>> GetByTypeAsync(DeviceType type)
    {
        return await _context.GeneralEquipments
            .Where(d => d.DeviceType == type)
            .Include(d => d.Inventory)
            .OrderBy(d => d.DeviceName == null ? string.Empty : d.DeviceName)
            .ToListAsync();
    }

    public async Task<GeneralEquipment> AddAsync(GeneralEquipment equipment)
    {
        equipment.CreatedAt = DateTime.Now;
        
        // 设置SortOrder为当前最大值+1
        var maxSortOrder = await _context.GeneralEquipments.MaxAsync(e => (int?)e.SortOrder) ?? 0;
        equipment.SortOrder = maxSortOrder + 1;
        
        _context.GeneralEquipments.Add(equipment);
        await _context.SaveChangesAsync();
        return equipment;
    }

    public async Task UpdateAsync(GeneralEquipment equipment)
    {
        // 先更新基本属性
        await _context.GeneralEquipments
            .Where(s => s.Id == equipment.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.SortOrder, equipment.SortOrder)
                .SetProperty(s => s.DeviceType, equipment.DeviceType)
                .SetProperty(s => s.DeviceName, equipment.DeviceName)
                .SetProperty(s => s.DeviceCode, equipment.DeviceCode)
                .SetProperty(s => s.Brand, equipment.Brand)
                .SetProperty(s => s.Model, equipment.Model)
                .SetProperty(s => s.SerialNumber, equipment.SerialNumber)
                .SetProperty(s => s.Specification, equipment.Specification)
                .SetProperty(s => s.Quantity, equipment.Quantity)
                .SetProperty(s => s.Unit, equipment.Unit)
                .SetProperty(s => s.DeviceStatus, equipment.DeviceStatus)
                .SetProperty(s => s.UseStatus, equipment.UseStatus)
                .SetProperty(s => s.Status, equipment.Status)
                .SetProperty(s => s.Company, equipment.Company)
                .SetProperty(s => s.Accessories, equipment.Accessories)
                .SetProperty(s => s.Remark, equipment.Remark)
                .SetProperty(s => s.RepairStatus, equipment.RepairStatus)
                .SetProperty(s => s.RepairPerson, equipment.RepairPerson)
                .SetProperty(s => s.RepairDate, equipment.RepairDate)
                .SetProperty(s => s.FaultReason, equipment.FaultReason)
                .SetProperty(s => s.Location, equipment.Location)
                .SetProperty(s => s.ProjectName, equipment.ProjectName)
                .SetProperty(s => s.ProjectTime, equipment.ProjectTime)
                .SetProperty(s => s.UpdatedAt, DateTime.Now)
                .SetProperty(s => s.UpdatedBy, equipment.UpdatedBy ?? "System")
            );
        
        // 处理图片数据
        // 由于EF Core的跟踪问题，我们不再处理Images集合
        // 图片的处理将在GeneralEquipmentService中通过其他方式实现
    }

    public async Task DeleteAsync(int id)
    {
        // 使用ExecuteDeleteAsync直接删除，避免EF Core跟踪冲突
        await _context.GeneralEquipments
            .Where(s => s.Id == id)
            .ExecuteDeleteAsync();
    }

    public async Task<bool> ExistsAsync(string code)
    {
        return await _context.GeneralEquipments
            .AnyAsync(d => d.DeviceCode == code);
    }

    public async Task<IEnumerable<GeneralEquipment>> SearchAsync(string keyword)
    {
        var lowerKeyword = keyword.ToLower();
        return await _context.GeneralEquipments
            .Where(d => 
                d.DeviceName.ToLower().Contains(lowerKeyword) ||
                (d.DeviceCode != null && d.DeviceCode.ToLower().Contains(lowerKeyword)) ||
                (d.Brand != null && d.Brand.ToLower().Contains(lowerKeyword)) ||
                (d.Model != null && d.Model.ToLower().Contains(lowerKeyword)))
            .Include(d => d.Inventory)
            .OrderBy(d => d.DeviceName == null ? string.Empty : d.DeviceName)
            .ToListAsync();
    }

    public async Task<IEnumerable<GeneralEquipment>> GetAvailableAsync(string? keyword = null)
    {
        try
        {
            var query = _context.GeneralEquipments
                .Where(e => e.Quantity > 0 && e.UseStatus != DeviceWarehouse.Domain.Enums.UsageStatus.InUse)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(e => 
                    (e.DeviceName != null && e.DeviceName.ToLower().Contains(lowerKeyword)) ||
                    (e.Brand != null && e.Brand.ToLower().Contains(lowerKeyword)) ||
                    (e.Model != null && e.Model.ToLower().Contains(lowerKeyword)));
                Console.WriteLine($"搜索通用设备，keyword: {keyword}, 小写: {lowerKeyword}");
            }

            var result = await query
                .OrderBy(e => e.DeviceName == null ? string.Empty : e.DeviceName)
                .ToListAsync();
            Console.WriteLine($"通用设备搜索结果数量: {result.Count}");
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
                        schemaCommand.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GeneralEquipment'";
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
                        FROM GeneralEquipment
                        {whereClause}
                        {(columns.Contains("DeviceName") ? "ORDER BY DeviceName" : "")}
                    ";
                    
                    var equipments = new List<GeneralEquipment>();
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
                                var equipment = new GeneralEquipment
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
                    
                    Console.WriteLine($"Fallback查询通用设备结果数量: {equipments.Count}");
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
                return new List<GeneralEquipment>();
            }
        }
    }

    // 新增：获取可用物品分页数据
    public async Task<(IEnumerable<GeneralEquipment> Items, int TotalCount)> GetAvailablePagedAsync(string? keyword, int pageNumber, int pageSize)
    {
        var connection = _context.Database.GetDbConnection();
        var parameters = new DynamicParameters();
        
        parameters.Add("Keyword", keyword);
        parameters.Add("PageNumber", pageNumber);
        parameters.Add("PageSize", pageSize);
        
        // 使用存储过程执行查询
        using var multi = await connection.QueryMultipleAsync("GetGeneralEquipmentAvailablePaged", parameters, commandType: System.Data.CommandType.StoredProcedure);
        
        var items = await multi.ReadAsync<GeneralEquipment>();
        var totalCount = await multi.ReadFirstAsync<int>();
        
        return (items, totalCount);
    }

    // 新增：获取分页数据
    public async Task<(IEnumerable<GeneralEquipment> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false, DeviceStatus? deviceStatus = null, UsageStatus? useStatus = null, string? brand = null)
    {
        var query = _context.GeneralEquipments.Include(d => d.Images).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.ToLower();
            query = query.Where(e => 
                (e.DeviceName != null && e.DeviceName.ToLower().Contains(lowerKeyword)) ||
                (e.DeviceCode != null && e.DeviceCode.ToLower().Contains(lowerKeyword)) ||
                (e.Brand != null && e.Brand.ToLower().Contains(lowerKeyword)) ||
                (e.Model != null && e.Model.ToLower().Contains(lowerKeyword)));
        }

        // 应用设备状态筛选
        if (deviceStatus.HasValue)
        {
            query = query.Where(e => e.DeviceStatus == deviceStatus.Value);
        }

        // 应用使用状态筛选
        if (useStatus.HasValue)
        {
            query = query.Where(e => e.UseStatus == useStatus.Value);
        }

        // 应用品牌筛选
        if (!string.IsNullOrWhiteSpace(brand))
        {
            var lowerBrand = brand.ToLower();
            query = query.Where(e => e.Brand != null && e.Brand.ToLower().Contains(lowerBrand));
        }

        // 排序
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            // 检查sortBy是否为有效的属性名
            var property = typeof(GeneralEquipment).GetProperty(sortBy);
            if (property != null)
            {
                query = query.ApplySorting(sortBy, sortDescending);
            }
            else
            {
                // 如果sortBy不是有效的属性名，使用默认排序
                query = query.OrderBy(e => e.DeviceName == null ? string.Empty : e.DeviceName);
            }
        }
        else
        {
            query = query.OrderBy(e => e.DeviceName == null ? string.Empty : e.DeviceName);
        }

        // 分页
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return (items, totalCount, pageNumber, pageSize, totalPages);
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
    public async Task<(IEnumerable<GeneralEquipment> Items, int TotalCount)> GetGroupedPagedAsync(string? keyword, int pageNumber, int pageSize)
    {
        var connection = _context.Database.GetDbConnection();
        
        try
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }
            
            var whereClause = "WHERE 1=1";
            var parameters = new DynamicParameters();
            
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                whereClause += " AND (DeviceName LIKE @Keyword OR Brand LIKE @Keyword OR Model LIKE @Keyword)";
                parameters.Add("Keyword", $"%{keyword}%");
            }
            
            // 限制分页大小，防止查询过慢
            pageSize = Math.Min(pageSize, 50);
            
            // 计数查询 - 统计分组数量
            var countSql = $@"
                SELECT COUNT(*) FROM (
                    SELECT DISTINCT DeviceName, Brand, Model
                    FROM GeneralEquipment
                    {whereClause}
                ) AS DistinctGroups";
            
            // 数据查询 - 先获取分组，再获取设备
            var dataSql = $@"
                -- 获取指定页的分组
                DECLARE @Groups TABLE (DeviceName NVARCHAR(255), Brand NVARCHAR(255), Model NVARCHAR(255))
                
                INSERT INTO @Groups
                SELECT DISTINCT DeviceName, Brand, Model
                FROM GeneralEquipment
                {whereClause}
                ORDER BY DeviceName
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                
                -- 获取这些分组内的所有设备
                SELECT e.*
                FROM GeneralEquipment e
                INNER JOIN @Groups g ON e.DeviceName = g.DeviceName 
                    AND ISNULL(e.Brand, '') = ISNULL(g.Brand, '')
                    AND ISNULL(e.Model, '') = ISNULL(g.Model, '')
                ORDER BY e.DeviceName, e.SortOrder, e.Id";

            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("PageSize", pageSize);
            
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
            var items = await connection.QueryAsync<GeneralEquipment>(dataSql, parameters);
            
            return (items, totalCount);
        }
        catch (Exception ex)
        {
            // 记录错误
            Console.WriteLine($"GetGroupedPagedAsync error: {ex.Message}");
            throw;
        }
        finally
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                await connection.CloseAsync();
            }
        }
    }

    public async Task<IEnumerable<GeneralEquipment>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _context.GeneralEquipments
            .Where(e => ids.Contains(e.Id))
            .Include(e => e.Inventory)
            .ToListAsync();
    }

    public async Task<int> CountByNameAsync(string deviceName)
    {
        return await _context.GeneralEquipments
            .Where(d => d.DeviceName == deviceName)
            .CountAsync();
    }

    // 新增：批量删除所有通用设备
    public async Task DeleteAllAsync()
    {
        try
        {
            Console.WriteLine("开始批量删除通用设备...");
            
            // 先删除所有关联的入库订单项目
            var inboundOrderItems = await _context.InboundOrderItems
                .Where(i => i.GeneralEquipmentId != null)
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
                .Where(i => i.GeneralEquipmentId != null)
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
                .Where(i => i.GeneralEquipmentId != null)
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
                .Where(i => i.GeneralEquipmentId != null)
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
            
            // 最后删除所有通用设备
            var equipments = await _context.GeneralEquipments.ToListAsync();
            Console.WriteLine($"找到 {equipments.Count} 台通用设备");
            
            if (equipments.Any())
            {
                _context.GeneralEquipments.RemoveRange(equipments);
                await _context.SaveChangesAsync();
                Console.WriteLine($"删除了 {equipments.Count} 台通用设备");
            } else {
                Console.WriteLine("没有找到通用设备");
            }
            
            // 再次检查是否还有通用设备
            var remainingEquipments = await _context.GeneralEquipments.ToListAsync();
            Console.WriteLine($"删除后剩余 {remainingEquipments.Count} 台通用设备");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"批量删除通用设备失败: {ex.Message}");
            throw;
        }
    }
}
