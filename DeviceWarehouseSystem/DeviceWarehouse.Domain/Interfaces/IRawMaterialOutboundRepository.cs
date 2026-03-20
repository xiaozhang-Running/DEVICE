using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Domain.Interfaces;

public interface IRawMaterialOutboundRepository
{
    Task<RawMaterialOutbound?> GetByIdAsync(int id);
    Task<RawMaterialOutbound?> GetByNumberAsync(string number);
    Task<IEnumerable<RawMaterialOutbound>> GetAllAsync();
    Task<RawMaterialOutbound> AddAsync(RawMaterialOutbound outbound);
    Task UpdateAsync(RawMaterialOutbound outbound);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsAsync(string number);
    Task<(IEnumerable<RawMaterialOutbound> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false);
}