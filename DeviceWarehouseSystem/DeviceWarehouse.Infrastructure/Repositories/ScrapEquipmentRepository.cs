using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Enums;
using DeviceWarehouse.Domain.Interfaces;
using DeviceWarehouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceWarehouse.Infrastructure.Repositories;

public class ScrapEquipmentRepository : IScrapEquipmentRepository
{
    private readonly ApplicationDbContext _context;

    public ScrapEquipmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ScrapEquipment?> GetByIdAsync(int id)
    {
        return await _context.ScrapEquipments
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<ScrapEquipment?> GetByDeviceCodeAsync(string deviceCode)
    {
        return await _context.ScrapEquipments
            .FirstOrDefaultAsync(s => s.DeviceCode == deviceCode);
    }

    public async Task<IEnumerable<ScrapEquipment>> GetAllAsync()
    {
        return await _context.ScrapEquipments
            .OrderByDescending(s => s.ScrapDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ScrapEquipment>> GetByDeviceTypeAsync(DeviceType deviceType)
    {
        return await _context.ScrapEquipments
            .Where(s => s.DeviceType == deviceType)
            .OrderByDescending(s => s.ScrapDate)
            .ToListAsync();
    }

    public async Task<ScrapEquipment> AddAsync(ScrapEquipment scrapEquipment)
    {
        scrapEquipment.CreatedAt = DateTime.Now;
        _context.ScrapEquipments.Add(scrapEquipment);
        await _context.SaveChangesAsync();
        return scrapEquipment;
    }

    public async Task UpdateAsync(ScrapEquipment scrapEquipment)
    {
        scrapEquipment.UpdatedAt = DateTime.Now;
        _context.ScrapEquipments.Update(scrapEquipment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var scrapEquipment = await _context.ScrapEquipments.FindAsync(id);
        if (scrapEquipment != null)
        {
            _context.ScrapEquipments.Remove(scrapEquipment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsByDeviceCodeAsync(string deviceCode)
    {
        return await _context.ScrapEquipments
            .AnyAsync(s => s.DeviceCode == deviceCode);
    }

    public async Task<IEnumerable<ScrapEquipment>> SearchAsync(string keyword)
    {
        var lowerKeyword = keyword.ToLower();
        return await _context.ScrapEquipments
            .Where(s => 
                (s.DeviceName != null && s.DeviceName.ToLower().Contains(lowerKeyword)) ||
                (s.DeviceCode != null && s.DeviceCode.ToLower().Contains(lowerKeyword)) ||
                (s.Brand != null && s.Brand.ToLower().Contains(lowerKeyword)) ||
                (s.Model != null && s.Model.ToLower().Contains(lowerKeyword)) ||
                (s.ScrapReason != null && s.ScrapReason.ToLower().Contains(lowerKeyword)))
            .OrderByDescending(s => s.ScrapDate)
            .ToListAsync();
    }

    public async Task<(IEnumerable<ScrapEquipment> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false)
    {
        var query = _context.ScrapEquipments.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.ToLower();
            query = query.Where(s => 
                (s.DeviceName != null && s.DeviceName.ToLower().Contains(lowerKeyword)) ||
                (s.DeviceCode != null && s.DeviceCode.ToLower().Contains(lowerKeyword)) ||
                (s.Brand != null && s.Brand.ToLower().Contains(lowerKeyword)) ||
                (s.Model != null && s.Model.ToLower().Contains(lowerKeyword)) ||
                (s.ScrapReason != null && s.ScrapReason.ToLower().Contains(lowerKeyword)));
        }

        // 排序
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            query = sortBy switch
            {
                "deviceName" => sortDescending ? query.OrderByDescending(s => s.DeviceName) : query.OrderBy(s => s.DeviceName),
                "deviceCode" => sortDescending ? query.OrderByDescending(s => s.DeviceCode) : query.OrderBy(s => s.DeviceCode),
                "scrapDate" => sortDescending ? query.OrderByDescending(s => s.ScrapDate) : query.OrderBy(s => s.ScrapDate),
                _ => query.OrderByDescending(s => s.ScrapDate)
            };
        }
        else
        {
            query = query.OrderByDescending(s => s.ScrapDate);
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