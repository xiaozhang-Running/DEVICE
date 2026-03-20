using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;
using DeviceWarehouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceWarehouse.Infrastructure.Repositories;

public class RawMaterialOutboundRepository : IRawMaterialOutboundRepository
{
    private readonly ApplicationDbContext _context;

    public RawMaterialOutboundRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RawMaterialOutbound?> GetByIdAsync(int id)
    {
        return await _context.RawMaterialOutbounds
            .Include(i => i.Items)
                .ThenInclude(item => item.RawMaterial)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<RawMaterialOutbound?> GetByNumberAsync(string number)
    {
        return await _context.RawMaterialOutbounds
            .Include(i => i.Items)
                .ThenInclude(item => item.RawMaterial)
            .FirstOrDefaultAsync(i => i.OutboundNumber == number);
    }

    public async Task<IEnumerable<RawMaterialOutbound>> GetAllAsync()
    {
        return await _context.RawMaterialOutbounds
            .Include(i => i.Items)
                .ThenInclude(item => item.RawMaterial)
            .OrderByDescending(i => i.OutboundDate)
            .ToListAsync();
    }

    public async Task<RawMaterialOutbound> AddAsync(RawMaterialOutbound outbound)
    {
        outbound.CreatedAt = DateTime.Now;
        _context.RawMaterialOutbounds.Add(outbound);
        await _context.SaveChangesAsync();
        return outbound;
    }

    public async Task UpdateAsync(RawMaterialOutbound outbound)
    {
        outbound.UpdatedAt = DateTime.Now;
        _context.RawMaterialOutbounds.Update(outbound);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var outbound = await _context.RawMaterialOutbounds.FindAsync(id);
        if (outbound != null)
        {
            _context.RawMaterialOutbounds.Remove(outbound);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.RawMaterialOutbounds
            .AnyAsync(i => i.Id == id);
    }

    public async Task<bool> ExistsAsync(string number)
    {
        return await _context.RawMaterialOutbounds
            .AnyAsync(i => i.OutboundNumber == number);
    }

    // 新增：获取分页数据
    public async Task<(IEnumerable<RawMaterialOutbound> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false)
    {
        var query = _context.RawMaterialOutbounds
            .Include(i => i.Items)
                .ThenInclude(item => item.RawMaterial)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.ToLower();
            query = query.Where(i => 
                i.OutboundNumber.ToLower().Contains(lowerKeyword) ||
                (i.Purpose != null && i.Purpose.ToLower().Contains(lowerKeyword)) ||
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
