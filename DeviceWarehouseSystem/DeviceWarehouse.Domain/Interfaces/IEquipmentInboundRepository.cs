using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Domain.Interfaces;

public interface IEquipmentInboundRepository
{
    Task<EquipmentInbound?> GetByIdAsync(int id);
    Task<EquipmentInbound?> GetByNumberAsync(string number);
    Task<IEnumerable<EquipmentInbound>> GetAllAsync();
    Task<EquipmentInbound> AddAsync(EquipmentInbound inbound);
    Task UpdateAsync(EquipmentInbound inbound);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsAsync(string number);
    Task<(IEnumerable<EquipmentInbound> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false);
}