using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Domain.Interfaces;

public interface IInboundOrderRepository
{
    Task<InboundOrder?> GetByIdAsync(int id);
    Task<IEnumerable<InboundOrder>> GetAllAsync();
    Task<InboundOrder> AddAsync(InboundOrder order);
    Task UpdateAsync(InboundOrder order);
    Task DeleteAsync(int id);
    Task<(IEnumerable<InboundOrder> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false);
}