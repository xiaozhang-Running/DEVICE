using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Domain.Interfaces;

public interface IInventoryTransactionRepository
{
    Task<InventoryTransaction> AddAsync(InventoryTransaction transaction);
    Task<IEnumerable<InventoryTransaction>> GetByInventoryIdAsync(int inventoryId);
    Task<IEnumerable<InventoryTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<InventoryTransaction>> GetByTypeAsync(string transactionType);
    Task<IEnumerable<InventoryTransaction>> GetAllAsync();
}
