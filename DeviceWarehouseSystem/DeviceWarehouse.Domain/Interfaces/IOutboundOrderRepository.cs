using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Domain.Interfaces;

public interface IOutboundOrderRepository
{
    Task<OutboundOrder?> GetByIdAsync(int id);
    Task<OutboundOrder?> GetByCodeAsync(string code);
    Task<IEnumerable<OutboundOrder>> GetAllAsync();
    Task<OutboundOrder> AddAsync(OutboundOrder order);
    Task UpdateAsync(OutboundOrder order);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsAsync(string code);
    Task<(IEnumerable<OutboundOrder> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false);
}