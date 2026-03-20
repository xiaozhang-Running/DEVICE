using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;
using DeviceWarehouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceWarehouse.Infrastructure.Repositories;

public class ProjectInboundRepository : IProjectInboundRepository
{
    private readonly ApplicationDbContext _context;

    public ProjectInboundRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectInbound?> GetByIdAsync(int id)
    {
        return await _context.ProjectInbounds
            .Include(i => i.Items)
            .Include(i => i.ProjectInboundOutbounds)
                .ThenInclude(pi => pi.ProjectOutbound)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<ProjectInbound?> GetByNumberAsync(string number)
    {
        return await _context.ProjectInbounds
            .Include(i => i.Items)
            .Include(i => i.ProjectInboundOutbounds)
                .ThenInclude(pi => pi.ProjectOutbound)
            .FirstOrDefaultAsync(i => i.InboundNumber == number);
    }

    public async Task<IEnumerable<ProjectInbound>> GetAllAsync()
    {
        return await _context.ProjectInbounds
            .Include(i => i.Items)
            .Include(i => i.ProjectInboundOutbounds)
                .ThenInclude(pi => pi.ProjectOutbound)
            .OrderByDescending(i => i.InboundDate)
            .ToListAsync();
    }

    public async Task<ProjectInbound> AddAsync(ProjectInbound inbound)
    {
        Console.WriteLine($"[AddAsync] 保存前 Status: {inbound.Status}");
        inbound.CreatedAt = DateTime.Now;
        _context.ProjectInbounds.Add(inbound);
        await _context.SaveChangesAsync();
        Console.WriteLine($"[AddAsync] 保存后 Status: {inbound.Status}");
        return inbound;
    }

    public async Task UpdateAsync(ProjectInbound inbound)
    {
        // 先更新主表
        var existingInbound = await _context.ProjectInbounds.FindAsync(inbound.Id);
        if (existingInbound != null)
        {
            existingInbound.InboundNumber = inbound.InboundNumber;
            existingInbound.InboundDate = inbound.InboundDate;
            existingInbound.ProjectName = inbound.ProjectName;
            existingInbound.ProjectCode = inbound.ProjectCode;
            existingInbound.ProjectManager = inbound.ProjectManager;
            existingInbound.Supplier = inbound.Supplier;
            existingInbound.InboundType = inbound.InboundType;
            existingInbound.ProjectTime = inbound.ProjectTime;
            existingInbound.ContactPhone = inbound.ContactPhone;
            existingInbound.StorageLocation = inbound.StorageLocation;
            existingInbound.Handler = inbound.Handler;
            existingInbound.WarehouseKeeper = inbound.WarehouseKeeper;
            existingInbound.InboundImages = inbound.InboundImages;
            existingInbound.Remark = inbound.Remark;
            existingInbound.Status = inbound.Status;
            existingInbound.IsCompleted = inbound.IsCompleted;
            existingInbound.CompletedAt = inbound.CompletedAt;
            existingInbound.UpdatedAt = DateTime.Now;
            existingInbound.UpdatedBy = inbound.UpdatedBy;
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var inbound = await _context.ProjectInbounds
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (inbound != null)
        {
            _context.ProjectInbounds.Remove(inbound);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.ProjectInbounds
            .AnyAsync(i => i.Id == id);
    }

    public async Task<bool> ExistsAsync(string number)
    {
        return await _context.ProjectInbounds
            .AnyAsync(i => i.InboundNumber == number);
    }

    public async Task<IEnumerable<dynamic>> ExecuteRawSqlAsync(string sql, object parameters)
    {
        return await _context.Database.SqlQueryRaw<dynamic>(sql, new Microsoft.Data.SqlClient.SqlParameter[] { }).ToListAsync();
    }

    // 新增：获取分页数据
    public async Task<(IEnumerable<ProjectInbound> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false)
    {
        var query = _context.ProjectInbounds
            .Include(i => i.Items)
            .Include(i => i.ProjectInboundOutbounds)
                .ThenInclude(pi => pi.ProjectOutbound)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.ToLower();
            query = query.Where(i => 
                i.InboundNumber.ToLower().Contains(lowerKeyword) ||
                i.ProjectName.ToLower().Contains(lowerKeyword) ||
                (i.Remark != null && i.Remark.ToLower().Contains(lowerKeyword)));
        }

        // 排序
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            switch (sortBy.ToLower())
            {
                case "inboundnumber":
                    query = sortDescending ? query.OrderByDescending(i => i.InboundNumber) : query.OrderBy(i => i.InboundNumber);
                    break;
                case "projectname":
                    query = sortDescending ? query.OrderByDescending(i => i.ProjectName) : query.OrderBy(i => i.ProjectName);
                    break;
                case "inbounddate":
                    query = sortDescending ? query.OrderByDescending(i => i.InboundDate) : query.OrderBy(i => i.InboundDate);
                    break;
                case "status":
                    query = sortDescending ? query.OrderByDescending(i => i.Status) : query.OrderBy(i => i.Status);
                    break;
                default:
                    query = query.OrderByDescending(i => i.InboundDate);
                    break;
            }
        }
        else
        {
            query = query.OrderByDescending(i => i.InboundDate);
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
}
