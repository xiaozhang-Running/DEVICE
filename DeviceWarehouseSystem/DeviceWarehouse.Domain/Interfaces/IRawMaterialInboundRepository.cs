using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Domain.Interfaces;

public interface IRawMaterialInboundRepository
{
    Task<RawMaterialInbound?> GetByIdAsync(int id);
    Task<RawMaterialInbound?> GetByNumberAsync(string number);
    Task<IEnumerable<RawMaterialInbound>> GetAllAsync();
    Task<RawMaterialInbound> AddAsync(RawMaterialInbound inbound);
    Task UpdateAsync(RawMaterialInbound inbound);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsAsync(string number);
    Task<(IEnumerable<RawMaterialInbound> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false);
}