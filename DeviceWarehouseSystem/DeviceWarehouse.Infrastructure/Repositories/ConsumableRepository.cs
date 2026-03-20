using Dapper;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;
using DeviceWarehouse.Infrastructure.Data;
using DeviceWarehouse.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DeviceWarehouse.Infrastructure.Repositories;

public class ConsumableRepository : IConsumableRepository
{
    private readonly ApplicationDbContext _context;

    public ConsumableRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Consumable?> GetByIdAsync(int id)
    {
        return await _context.Consumables.FindAsync(id);
    }

    public async Task<IEnumerable<Consumable>> GetAllAsync()
    {
        return await _context.Consumables
            .OrderBy(c => c.Name == null ? string.Empty : c.Name)
            .ToListAsync();
    }

    public async Task<Consumable> AddAsync(Consumable consumable)
    {
        consumable.CreatedAt = DateTime.Now;
        consumable.RemainingQuantity = consumable.TotalQuantity - consumable.UsedQuantity;
        _context.Consumables.Add(consumable);
        await _context.SaveChangesAsync();
        return consumable;
    }

    public async Task UpdateAsync(Consumable consumable)
    {
        // 计算剩余数量
        var remainingQuantity = consumable.TotalQuantity - consumable.UsedQuantity;
        
        // 使用ExecuteUpdateAsync直接更新，避免跟踪冲突
        await _context.Consumables
            .Where(c => c.Id == consumable.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.Name, consumable.Name)
                .SetProperty(c => c.Brand, consumable.Brand)
                .SetProperty(c => c.ModelSpecification, consumable.ModelSpecification)
                .SetProperty(c => c.Unit, consumable.Unit)
                .SetProperty(c => c.TotalQuantity, consumable.TotalQuantity)
                .SetProperty(c => c.UsedQuantity, consumable.UsedQuantity)
                .SetProperty(c => c.RemainingQuantity, remainingQuantity)
                .SetProperty(c => c.Status, consumable.Status)
                .SetProperty(c => c.Company, consumable.Company)
                .SetProperty(c => c.Accessories, consumable.Accessories)
                .SetProperty(c => c.Remark, consumable.Remark)
                .SetProperty(c => c.Location, consumable.Location)
                .SetProperty(c => c.UpdatedAt, DateTime.Now)
            );
    }

    public async Task DeleteAsync(int id)
    {
        var consumable = await _context.Consumables.FindAsync(id);
        if (consumable != null)
        {
            _context.Consumables.Remove(consumable);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Consumable>> SearchAsync(string keyword)
    {
        var lowerKeyword = keyword.ToLower();
        return await _context.Consumables
            .Where(c => 
                (c.Name != null && c.Name.ToLower().Contains(lowerKeyword)) ||
                (c.Brand != null && c.Brand.ToLower().Contains(lowerKeyword)) ||
                (c.ModelSpecification != null && c.ModelSpecification.ToLower().Contains(lowerKeyword)) ||
                (c.Company != null && c.Company.ToLower().Contains(lowerKeyword)) ||
                (c.Location != null && c.Location.ToLower().Contains(lowerKeyword)))
            .OrderBy(c => c.Name == null ? string.Empty : c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Consumable>> GetAvailableAsync(string? keyword = null)
    {
        try
        {
            var query = _context.Consumables
                .Where(c => c.RemainingQuantity > 0)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(c => 
                    (c.Name != null && c.Name.ToLower().Contains(lowerKeyword)) ||
                    (c.Brand != null && c.Brand.ToLower().Contains(lowerKeyword)) ||
                    (c.ModelSpecification != null && c.ModelSpecification.ToLower().Contains(lowerKeyword)));
                Console.WriteLine($"搜索耗材，keyword: {keyword}, 小写: {lowerKeyword}");
            }

            var result = await query
                .OrderBy(c => c.Name == null ? string.Empty : c.Name)
                .ToListAsync();
            Console.WriteLine($"耗材搜索结果数量: {result.Count}");
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
                        schemaCommand.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Consumables'";
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
                    if (columns.Contains("Name")) selectColumns.Add("Name");
                    if (columns.Contains("Brand")) selectColumns.Add("Brand");
                    if (columns.Contains("ModelSpecification")) selectColumns.Add("ModelSpecification");
                    if (columns.Contains("Unit")) selectColumns.Add("Unit");
                    if (columns.Contains("RemainingQuantity")) selectColumns.Add("RemainingQuantity");
                    if (columns.Contains("Location")) selectColumns.Add("Location");
                    if (columns.Contains("Company")) selectColumns.Add("Company");
                    if (columns.Contains("Accessories")) selectColumns.Add("Accessories");
                    if (columns.Contains("Remark")) selectColumns.Add("Remark");
                    
                    var whereClause = "WHERE RemainingQuantity > 0";
                    var parameters = new Dictionary<string, object>();
                    
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        var keywordConditions = new List<string>();
                        if (columns.Contains("Name")) keywordConditions.Add("Name LIKE @Keyword");
                        if (columns.Contains("Brand")) keywordConditions.Add("Brand LIKE @Keyword");
                        if (columns.Contains("ModelSpecification")) keywordConditions.Add("ModelSpecification LIKE @Keyword");
                        
                        if (keywordConditions.Count > 0)
                        {
                            whereClause += " AND (" + string.Join(" OR ", keywordConditions) + ")";
                            parameters["@Keyword"] = $"%{keyword}%";
                        }
                    }
                    
                    var sql = $@"
                        SELECT {string.Join(", ", selectColumns)}
                        FROM Consumables
                        {whereClause}
                        {(columns.Contains("Name") ? "ORDER BY Name" : "")}
                    ";
                    
                    var consumables = new List<Consumable>();
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
                                var consumable = new Consumable
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Name = columns.Contains("Name") && !reader.IsDBNull(reader.GetOrdinal("Name")) ? reader.GetString(reader.GetOrdinal("Name")) : string.Empty,
                                    Brand = columns.Contains("Brand") && !reader.IsDBNull(reader.GetOrdinal("Brand")) ? reader.GetString(reader.GetOrdinal("Brand")) : null,
                                    ModelSpecification = columns.Contains("ModelSpecification") && !reader.IsDBNull(reader.GetOrdinal("ModelSpecification")) ? reader.GetString(reader.GetOrdinal("ModelSpecification")) : null,
                                    Unit = columns.Contains("Unit") && !reader.IsDBNull(reader.GetOrdinal("Unit")) ? reader.GetString(reader.GetOrdinal("Unit")) : null,
                                    RemainingQuantity = columns.Contains("RemainingQuantity") ? reader.GetInt32(reader.GetOrdinal("RemainingQuantity")) : 0,
                                    Location = columns.Contains("Location") && !reader.IsDBNull(reader.GetOrdinal("Location")) ? reader.GetString(reader.GetOrdinal("Location")) : null,
                                    Company = columns.Contains("Company") && !reader.IsDBNull(reader.GetOrdinal("Company")) ? reader.GetString(reader.GetOrdinal("Company")) : null,
                                    Accessories = columns.Contains("Accessories") && !reader.IsDBNull(reader.GetOrdinal("Accessories")) ? reader.GetString(reader.GetOrdinal("Accessories")) : null,
                                    Remark = columns.Contains("Remark") && !reader.IsDBNull(reader.GetOrdinal("Remark")) ? reader.GetString(reader.GetOrdinal("Remark")) : null
                                };
                                consumables.Add(consumable);
                            }
                        }
                    }
                    
                    Console.WriteLine($"Fallback查询耗材结果数量: {consumables.Count}");
                    return consumables;
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
                return new List<Consumable>();
            }
        }
    }

    // 新增：获取可用物品分页数据
    public async Task<(IEnumerable<Consumable> Items, int TotalCount)> GetAvailablePagedAsync(string? keyword, int pageNumber, int pageSize)
    {
        var connection = _context.Database.GetDbConnection();
        var parameters = new DynamicParameters();
        
        parameters.Add("Keyword", keyword);
        parameters.Add("PageNumber", pageNumber);
        parameters.Add("PageSize", pageSize);
        
        // 使用存储过程执行查询
        using var multi = await connection.QueryMultipleAsync("GetConsumableAvailablePaged", parameters, commandType: System.Data.CommandType.StoredProcedure);
        
        var items = await multi.ReadAsync<Consumable>();
        var totalCount = await multi.ReadFirstAsync<int>();
        
        return (items, totalCount);
    }

    // 新增：获取分页数据
    public async Task<(IEnumerable<Consumable> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false)
    {
        var query = _context.Consumables.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.ToLower();
            query = query.Where(c => 
                (c.Name != null && c.Name.ToLower().Contains(lowerKeyword)) ||
                (c.Brand != null && c.Brand.ToLower().Contains(lowerKeyword)) ||
                (c.ModelSpecification != null && c.ModelSpecification.ToLower().Contains(lowerKeyword)) ||
                (c.Company != null && c.Company.ToLower().Contains(lowerKeyword)) ||
                (c.Location != null && c.Location.ToLower().Contains(lowerKeyword)));
        }

        // 排序
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            query = query.ApplySorting(sortBy, sortDescending);
        }
        else
        {
            query = query.OrderBy(c => c.Name == null ? string.Empty : c.Name);
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
    public async Task<(IEnumerable<Consumable> Items, int TotalCount)> GetGroupedPagedAsync(string? keyword, int pageNumber, int pageSize)
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
                whereClause += " AND (Name LIKE @Keyword OR Brand LIKE @Keyword OR ModelSpecification LIKE @Keyword)";
                parameters.Add("Keyword", $"%{keyword}%");
            }
            
            // 限制分页大小，防止查询过慢
            pageSize = Math.Min(pageSize, 50);
            
            // 计数查询 - 统计分组数量
            var countSql = $@"
                SELECT COUNT(*) FROM (
                    SELECT DISTINCT Name, Brand, ModelSpecification
                    FROM Consumables
                    {whereClause}
                ) AS DistinctGroups";
            
            // 数据查询 - 先获取分组，再获取设备
            var dataSql = $@"
                -- 获取指定页的分组
                DECLARE @Groups TABLE (Name NVARCHAR(255), Brand NVARCHAR(255), ModelSpecification NVARCHAR(255))
                
                INSERT INTO @Groups
                SELECT DISTINCT Name, Brand, ModelSpecification
                FROM Consumables
                {whereClause}
                ORDER BY Name
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                
                -- 获取这些分组内的所有设备
                SELECT c.*
                FROM Consumables c
                INNER JOIN @Groups g ON c.Name = g.Name 
                    AND ISNULL(c.Brand, '') = ISNULL(g.Brand, '')
                    AND ISNULL(c.ModelSpecification, '') = ISNULL(g.ModelSpecification, '')
                ORDER BY c.Name";

            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("PageSize", pageSize);
            
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
            var items = await connection.QueryAsync<Consumable>(dataSql, parameters);
            
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

    public async Task<bool> ExistsAsync(string number)
    {
        return await _context.Consumables.AnyAsync(c => c.Name == number);
    }
}
