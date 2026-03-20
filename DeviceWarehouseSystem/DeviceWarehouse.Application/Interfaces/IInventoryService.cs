using DeviceWarehouse.Application.DTOs;

namespace DeviceWarehouse.Application.Interfaces;

public interface IInventoryService
{
    Task<InventoryDto> GetByIdAsync(int id);
    Task<InventoryDto> GetBySpecialEquipmentIdAsync(int specialEquipmentId);
    Task<InventoryDto> GetByGeneralEquipmentIdAsync(int generalEquipmentId);
    Task<InventoryDto> GetByConsumableIdAsync(int consumableId);
    Task<InventoryDto> GetByRawMaterialIdAsync(int rawMaterialId);
    Task<IEnumerable<InventoryDto>> GetAllAsync(int? category = null);
    Task<IEnumerable<InventoryDto>> GetLowStockAsync(int threshold = 10);
    Task<IEnumerable<InventoryDto>> GetZeroStockAsync();
    Task<InventoryDto> CreateAsync(CreateInventoryDto dto);
    Task UpdateAsync(int id, UpdateInventoryDto dto);
    Task DeleteAsync(int id);
    Task<InventoryDto> AddStockAsync(int inventoryId, int quantity, string reason, string reference);
    Task<InventoryDto> RemoveStockAsync(int inventoryId, int quantity, string reason, string reference);
    Task<InventoryTransactionDto> CreateTransactionAsync(InventoryTransactionDto dto);
    Task<IEnumerable<InventoryTransactionDto>> GetTransactionsAsync(int inventoryId);
    Task<IEnumerable<InventoryTransactionDto>> GetTransactionsAsync(DateTime? startDate, DateTime? endDate);
    Task<IEnumerable<InventoryTransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<InventoryReportDto> GetInventoryReportAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<InventoryReportDto>> GetCategoryReportAsync();
}