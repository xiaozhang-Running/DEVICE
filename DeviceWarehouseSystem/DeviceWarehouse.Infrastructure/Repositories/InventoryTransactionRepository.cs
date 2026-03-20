using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;
using DeviceWarehouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceWarehouse.Infrastructure.Repositories;

public class InventoryTransactionRepository : IInventoryTransactionRepository
{
    private readonly ApplicationDbContext _context;

    public InventoryTransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryTransaction> AddAsync(InventoryTransaction transaction)
    {
        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<IEnumerable<InventoryTransaction>> GetByInventoryIdAsync(int inventoryId)
    {
        return await _context.InventoryTransactions
            .Where(t => t.InventoryId == inventoryId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.InventoryTransactions
            .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryTransaction>> GetByTypeAsync(string transactionType)
    {
        return await _context.InventoryTransactions
            .Where(t => t.TransactionType == transactionType)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryTransaction>> GetAllAsync()
    {
        return await _context.InventoryTransactions
            .Include(t => t.Inventory)
                .ThenInclude(i => i.SpecialEquipment)
            .Include(t => t.Inventory)
                .ThenInclude(i => i.GeneralEquipment)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }
}
