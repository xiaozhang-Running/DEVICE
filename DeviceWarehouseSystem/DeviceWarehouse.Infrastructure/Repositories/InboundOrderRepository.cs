using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;
using DeviceWarehouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceWarehouse.Infrastructure.Repositories;

public class InboundOrderRepository : IInboundOrderRepository
{
    private readonly ApplicationDbContext _context;

    public InboundOrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InboundOrder?> GetByIdAsync(int id)
    {
        return await _context.InboundOrders
            .Include(o => o.Items)
                .ThenInclude(i => i.SpecialEquipment)
            .Include(o => o.Items)
                .ThenInclude(i => i.GeneralEquipment)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<InboundOrder?> GetByCodeAsync(string code)
    {
        return await _context.InboundOrders
            .FirstOrDefaultAsync(o => o.OrderCode == code);
    }

    public async Task<IEnumerable<InboundOrder>> GetAllAsync()
    {
        return await _context.InboundOrders
            .Include(o => o.Items)
                .ThenInclude(i => i.SpecialEquipment)
            .Include(o => o.Items)
                .ThenInclude(i => i.GeneralEquipment)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<InboundOrder> AddAsync(InboundOrder order)
    {
        order.CreatedAt = DateTime.Now;
        _context.InboundOrders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateAsync(InboundOrder order)
    {
        order.UpdatedAt = DateTime.Now;
        _context.InboundOrders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var order = await _context.InboundOrders.FindAsync(id);
        if (order != null)
        {
            _context.InboundOrders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string code)
    {
        return await _context.InboundOrders
            .AnyAsync(o => o.OrderCode == code);
    }

    // 新增：获取分页数据
    public async Task<(IEnumerable<InboundOrder> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false)
    {
        var query = _context.InboundOrders
            .Include(o => o.Items)
                .ThenInclude(i => i.SpecialEquipment)
            .Include(o => o.Items)
                .ThenInclude(i => i.GeneralEquipment)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.ToLower();
            query = query.Where(o => 
                o.OrderCode.ToLower().Contains(lowerKeyword) ||
                (o.Supplier != null && o.Supplier.ToLower().Contains(lowerKeyword)) ||
                (o.Remark != null && o.Remark.ToLower().Contains(lowerKeyword)));
        }

        // 排序
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            switch (sortBy.ToLower())
            {
                case "ordercode":
                    query = sortDescending ? query.OrderByDescending(o => o.OrderCode) : query.OrderBy(o => o.OrderCode);
                    break;
                case "orderdate":
                    query = sortDescending ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate);
                    break;
                default:
                    query = query.OrderByDescending(o => o.OrderDate);
                    break;
            }
        }
        else
        {
            query = query.OrderByDescending(o => o.OrderDate);
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
