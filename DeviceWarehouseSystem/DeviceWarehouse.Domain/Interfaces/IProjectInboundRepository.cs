using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Domain.Interfaces;

public interface IProjectInboundRepository
{
    Task<ProjectInbound?> GetByIdAsync(int id);
    Task<ProjectInbound?> GetByNumberAsync(string number);
    Task<IEnumerable<ProjectInbound>> GetAllAsync();
    Task<ProjectInbound> AddAsync(ProjectInbound inbound);
    Task UpdateAsync(ProjectInbound inbound);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsAsync(string number);
    Task<(IEnumerable<ProjectInbound> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false);
}