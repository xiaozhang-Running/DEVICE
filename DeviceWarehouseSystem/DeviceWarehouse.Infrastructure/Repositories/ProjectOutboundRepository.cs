using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;
using DeviceWarehouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceWarehouse.Infrastructure.Repositories;

public class ProjectOutboundRepository : IProjectOutboundRepository
{
    private readonly ApplicationDbContext _context;

    public ProjectOutboundRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectOutbound?> GetByIdAsync(int id)
    {
        return await _context.ProjectOutbounds
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<ProjectOutbound?> GetByNumberAsync(string number)
    {
        return await _context.ProjectOutbounds
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.OutboundNumber == number);
    }

    public async Task<IEnumerable<ProjectOutbound>> GetAllAsync()
    {
        return await _context.ProjectOutbounds
            .Include(i => i.Items)
            .OrderByDescending(i => i.OutboundDate)
            .ToListAsync();
    }

    public async Task<ProjectOutbound> AddAsync(ProjectOutbound outbound)
    {
        outbound.CreatedAt = DateTime.Now;
        _context.ProjectOutbounds.Add(outbound);
        await _context.SaveChangesAsync();
        return outbound;
    }

    public async Task UpdateAsync(ProjectOutbound outbound)
    {
        outbound.UpdatedAt = DateTime.Now;
        _context.ProjectOutbounds.Update(outbound);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            // 先删除相关的ProjectInboundOutbound记录
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ProjectInboundOutbound WHERE ProjectOutboundId = {0}", id);
            
            // 再删除相关的ProjectOutboundItem记录
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ProjectOutboundItem WHERE OutboundId = {0}", id);
            
            // 最后删除ProjectOutbound记录
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ProjectOutbound WHERE Id = {0}", id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DeleteAsync] 删除出库单 {id} 失败: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.ProjectOutbounds
            .AnyAsync(i => i.Id == id);
    }

    public async Task<bool> ExistsAsync(string number)
    {
        return await _context.ProjectOutbounds
            .AnyAsync(i => i.OutboundNumber == number);
    }

    public async Task<IEnumerable<ProjectOutbound>> SearchAsync(string keyword)
    {
        if (!string.IsNullOrEmpty(keyword))
        {
            return await _context.ProjectOutbounds
                .Include(i => i.Items)
                .Where(i => i.OutboundNumber.Contains(keyword) || i.ProjectName.Contains(keyword))
                .OrderByDescending(i => i.OutboundDate)
                .ToListAsync();
        }
        else
        {
            return await _context.ProjectOutbounds
                .Include(i => i.Items)
                .OrderByDescending(i => i.OutboundDate)
                .ToListAsync();
        }
    }

    // 新增：获取分页数据
    public async Task<(IEnumerable<ProjectOutbound> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false)
    {
        var query = _context.ProjectOutbounds
            .Include(i => i.Items)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.ToLower();
            query = query.Where(i => 
                i.OutboundNumber.ToLower().Contains(lowerKeyword) ||
                i.ProjectName.ToLower().Contains(lowerKeyword) ||
                (i.Remark != null && i.Remark.ToLower().Contains(lowerKeyword)));
        }

        // 排序
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            switch (sortBy.ToLower())
            {
                case "outboundnumber":
                    query = sortDescending ? query.OrderByDescending(i => i.OutboundNumber) : query.OrderBy(i => i.OutboundNumber);
                    break;
                case "projectname":
                    query = sortDescending ? query.OrderByDescending(i => i.ProjectName) : query.OrderBy(i => i.ProjectName);
                    break;
                case "outbounddate":
                    query = sortDescending ? query.OrderByDescending(i => i.OutboundDate) : query.OrderBy(i => i.OutboundDate);
                    break;
                default:
                    query = query.OrderByDescending(i => i.OutboundDate);
                    break;
            }
        }
        else
        {
            query = query.OrderByDescending(i => i.OutboundDate);
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
