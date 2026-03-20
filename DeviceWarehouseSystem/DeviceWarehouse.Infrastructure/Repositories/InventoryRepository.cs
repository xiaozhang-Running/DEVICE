using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;
using DeviceWarehouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceWarehouse.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly ApplicationDbContext _context;

    public InventoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Inventory?> GetBySpecialEquipmentIdAsync(int specialEquipmentId)
    {
        return await _context.Inventories
            .Where(i => i.SpecialEquipmentId == specialEquipmentId)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<Inventory?> GetByGeneralEquipmentIdAsync(int generalEquipmentId)
    {
        return await _context.Inventories
            .Where(i => i.GeneralEquipmentId == generalEquipmentId)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<Inventory?> GetByConsumableIdAsync(int consumableId)
    {
        // 由于数据库表中没有ConsumableId列，返回null
        return await Task.FromResult<Inventory?>(null);
    }

    public async Task<Inventory?> GetByRawMaterialIdAsync(int rawMaterialId)
    {
        // 由于数据库表中没有RawMaterialId列，返回null
        return await Task.FromResult<Inventory?>(null);
    }

    public async Task<Inventory> AddAsync(Inventory inventory)
    {
        inventory.LastUpdated = DateTime.Now;
        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync();
        return inventory;
    }

    public async Task UpdateAsync(Inventory inventory)
    {
        // 使用ExecuteUpdateAsync直接更新，避免跟踪冲突
        await _context.Inventories
            .Where(i => i.Id == inventory.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(i => i.CurrentQuantity, inventory.CurrentQuantity)
                .SetProperty(i => i.AlertMinQuantity, inventory.AlertMinQuantity)
                .SetProperty(i => i.AlertMaxQuantity, inventory.AlertMaxQuantity)
                .SetProperty(i => i.LastUpdated, DateTime.Now)
            );
    }

    public async Task DeleteAsync(int id)
    {
        var inventory = await _context.Inventories.FindAsync(id);
        if (inventory != null)
        {
            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Inventory>> GetAllAsync()
    {
        return await _context.Inventories
            .Include(i => i.SpecialEquipment)
            .Include(i => i.GeneralEquipment)
            .OrderBy(i => i.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inventory>> GetLowStockAsync(int threshold)
    {
        return await _context.Inventories
            .Include(i => i.SpecialEquipment)
            .Include(i => i.GeneralEquipment)
            .Where(i => i.CurrentQuantity <= threshold)
            .OrderBy(i => i.CurrentQuantity)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inventory>> GetLowStockAsync()
    {
        return await _context.Inventories
            .Include(i => i.SpecialEquipment)
            .Include(i => i.GeneralEquipment)
            .Where(i => i.CurrentQuantity <= i.AlertMinQuantity)
            .OrderBy(i => i.CurrentQuantity)
            .ToListAsync();
    }

    public async Task<Inventory?> GetByIdAsync(int id)
    {
        return await _context.Inventories
            .Include(i => i.SpecialEquipment)
            .Include(i => i.GeneralEquipment)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task DeleteBySpecialEquipmentIdAsync(int specialEquipmentId)
    {
        var inventory = await _context.Inventories
            .Where(i => i.SpecialEquipmentId == specialEquipmentId)
            .AsNoTracking()
            .FirstOrDefaultAsync();
        if (inventory != null)
        {
            // 直接使用ID删除，避免跟踪冲突
            await _context.Inventories.Where(i => i.Id == inventory.Id).ExecuteDeleteAsync();
        }
    }

    public async Task CreateBySpecialEquipmentIdAsync(int specialEquipmentId, int quantity)
    {
        // 使用原始SQL语句插入数据，避免EF Core跟踪冲突
        await _context.Database.ExecuteSqlRawAsync(
            "INSERT INTO Inventories (SpecialEquipmentId, CurrentQuantity, AlertMinQuantity, AlertMaxQuantity, LastUpdated) " +
            "VALUES (@SpecialEquipmentId, @CurrentQuantity, @AlertMinQuantity, @AlertMaxQuantity, @LastUpdated)",
            new Microsoft.Data.SqlClient.SqlParameter("@SpecialEquipmentId", specialEquipmentId),
            new Microsoft.Data.SqlClient.SqlParameter("@CurrentQuantity", quantity),
            new Microsoft.Data.SqlClient.SqlParameter("@AlertMinQuantity", 1),
            new Microsoft.Data.SqlClient.SqlParameter("@AlertMaxQuantity", quantity * 2),
            new Microsoft.Data.SqlClient.SqlParameter("@LastUpdated", DateTime.Now)
        );
    }

    public async Task UpdateBySpecialEquipmentIdAsync(int specialEquipmentId, int quantity)
    {
        // 使用ExecuteUpdateAsync直接更新，避免跟踪冲突
        await _context.Inventories
            .Where(i => i.SpecialEquipmentId == specialEquipmentId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(i => i.CurrentQuantity, quantity)
                .SetProperty(i => i.LastUpdated, DateTime.Now)
            );
    }
}
