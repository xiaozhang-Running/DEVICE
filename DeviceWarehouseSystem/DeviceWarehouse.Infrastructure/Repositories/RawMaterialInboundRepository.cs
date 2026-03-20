using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;
using DeviceWarehouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceWarehouse.Infrastructure.Repositories;

public class RawMaterialInboundRepository : IRawMaterialInboundRepository
{
    private readonly ApplicationDbContext _context;

    public RawMaterialInboundRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RawMaterialInbound?> GetByIdAsync(int id)
    {
        return await _context.RawMaterialInbounds
            .Include(i => i.Items)
                .ThenInclude(item => item.RawMaterial)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<RawMaterialInbound?> GetByNumberAsync(string number)
    {
        return await _context.RawMaterialInbounds
            .Include(i => i.Items)
                .ThenInclude(item => item.RawMaterial)
            .FirstOrDefaultAsync(i => i.InboundNumber == number);
    }

    public async Task<IEnumerable<RawMaterialInbound>> GetAllAsync()
    {
        return await _context.RawMaterialInbounds
            .Include(i => i.Items)
                .ThenInclude(item => item.RawMaterial)
            .OrderByDescending(i => i.InboundDate)
            .ToListAsync();
    }

    public async Task<RawMaterialInbound> AddAsync(RawMaterialInbound inbound)
    {
        inbound.CreatedAt = DateTime.Now;
        _context.RawMaterialInbounds.Add(inbound);
        await _context.SaveChangesAsync();
        return inbound;
    }

    public async Task UpdateAsync(RawMaterialInbound inbound)
    {
        inbound.UpdatedAt = DateTime.Now;
        _context.RawMaterialInbounds.Update(inbound);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var inbound = await _context.RawMaterialInbounds.FindAsync(id);
        if (inbound != null)
        {
            _context.RawMaterialInbounds.Remove(inbound);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.RawMaterialInbounds
            .AnyAsync(i => i.Id == id);
    }

    public async Task<bool> ExistsAsync(string number)
    {
        return await _context.RawMaterialInbounds
            .AnyAsync(i => i.InboundNumber == number);
    }

    // 新增：获取分页数据
    public async Task<(IEnumerable<RawMaterialInbound> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false)
    {
        var query = _context.RawMaterialInbounds
            .Include(i => i.Items)
                .ThenInclude(item => item.RawMaterial)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.ToLower();
            query = query.Where(i => 
                i.InboundNumber.ToLower().Contains(lowerKeyword) ||
                (i.Supplier != null && i.Supplier.ToLower().Contains(lowerKeyword)) ||
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
                case "inbounddate":
                    query = sortDescending ? query.OrderByDescending(i => i.InboundDate) : query.OrderBy(i => i.InboundDate);
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
