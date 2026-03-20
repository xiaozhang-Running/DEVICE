using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Domain.Interfaces;

public interface IInventoryRepository
{
    Task<Inventory?> GetByIdAsync(int id);
    Task<IEnumerable<Inventory>> GetAllAsync();
    Task<Inventory> AddAsync(Inventory inventory);
    Task UpdateAsync(Inventory inventory);
    Task DeleteAsync(int id);
    Task<Inventory?> GetBySpecialEquipmentIdAsync(int specialEquipmentId);
    Task<Inventory?> GetByGeneralEquipmentIdAsync(int generalEquipmentId);
    Task<Inventory?> GetByConsumableIdAsync(int consumableId);
    Task<Inventory?> GetByRawMaterialIdAsync(int rawMaterialId);
    Task<IEnumerable<Inventory>> GetLowStockAsync(int threshold);
    Task<IEnumerable<Inventory>> GetLowStockAsync();
    Task DeleteBySpecialEquipmentIdAsync(int specialEquipmentId);
    Task CreateBySpecialEquipmentIdAsync(int specialEquipmentId, int quantity);
    Task UpdateBySpecialEquipmentIdAsync(int specialEquipmentId, int quantity);
}