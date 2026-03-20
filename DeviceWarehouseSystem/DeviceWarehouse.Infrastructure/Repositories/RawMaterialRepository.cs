using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;
using DeviceWarehouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceWarehouse.Infrastructure.Repositories;

public class RawMaterialRepository : IRawMaterialRepository
{
    private readonly ApplicationDbContext _context;

    public RawMaterialRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RawMaterial?> GetByIdAsync(int id)
    {
        return await _context.RawMaterials
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<RawMaterial>> GetAllAsync()
    {
        return await _context.RawMaterials
            .OrderBy(r => r.SortOrder)
            .ThenBy(r => r.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<RawMaterial> AddAsync(RawMaterial material)
    {
        material.CreatedAt = DateTime.Now;
        material.UsedQuantity = 0;
        material.RemainingQuantity = material.TotalQuantity;
        _context.RawMaterials.Add(material);
        await _context.SaveChangesAsync();
        return material;
    }

    public async Task UpdateAsync(RawMaterial material)
    {
        material.UpdatedAt = DateTime.Now;
        material.RemainingQuantity = material.TotalQuantity - material.UsedQuantity;
        _context.RawMaterials.Update(material);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var material = await _context.RawMaterials.FindAsync(id);
        if (material != null)
        {
            _context.RawMaterials.Remove(material);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<RawMaterial>> SearchAsync(string keyword)
    {
        var lowerKeyword = keyword.ToLower();
        return await _context.RawMaterials
            .Where(r => 
                r.ProductName.ToLower().Contains(lowerKeyword) ||
                (r.Specification != null && r.Specification.ToLower().Contains(lowerKeyword)) ||
                (r.Supplier != null && r.Supplier.ToLower().Contains(lowerKeyword)) ||
                (r.Remark != null && r.Remark.ToLower().Contains(lowerKeyword)))
            .OrderBy(r => r.ProductName)
            .ToListAsync();
    }

    // 新增：获取分页数据
    public async Task<(IEnumerable<RawMaterial> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false)
    {
        var query = _context.RawMaterials.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.ToLower();
            query = query.Where(r => 
                r.ProductName.ToLower().Contains(lowerKeyword) ||
                (r.Specification != null && r.Specification.ToLower().Contains(lowerKeyword)) ||
                (r.Supplier != null && r.Supplier.ToLower().Contains(lowerKeyword)) ||
                (r.Remark != null && r.Remark.ToLower().Contains(lowerKeyword)));
        }

        // 排序
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            switch (sortBy.ToLower())
            {
                case "productname":
                    query = sortDescending ? query.OrderByDescending(r => r.ProductName) : query.OrderBy(r => r.ProductName);
                    break;
                case "totalquantity":
                    query = sortDescending ? query.OrderByDescending(r => r.TotalQuantity) : query.OrderBy(r => r.TotalQuantity);
                    break;
                case "remainingquantity":
                    query = sortDescending ? query.OrderByDescending(r => r.RemainingQuantity) : query.OrderBy(r => r.RemainingQuantity);
                    break;
                default:
                    query = query.OrderBy(r => r.ProductName);
                    break;
            }
        }
        else
        {
            query = query.OrderBy(r => r.ProductName);
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

    public async Task<bool> ExistsAsync(string number)
    {
        return await _context.RawMaterials.AnyAsync(r => r.ProductName == number);
    }

    public async Task<IEnumerable<RawMaterial>> GetAvailableAsync(string? keyword = null)
    {
        try
        {
            var query = _context.RawMaterials
                .Where(m => m.RemainingQuantity > 0)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(m => 
                    (m.ProductName != null && m.ProductName.ToLower().Contains(lowerKeyword)) ||
                    (m.Specification != null && m.Specification.ToLower().Contains(lowerKeyword)) ||
                    (m.Company != null && m.Company.ToLower().Contains(lowerKeyword)));
            }

            var result = await query
                .OrderBy(m => m.ProductName)
                .ToListAsync();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetAvailableAsync error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            return [];
        }
    }
}
