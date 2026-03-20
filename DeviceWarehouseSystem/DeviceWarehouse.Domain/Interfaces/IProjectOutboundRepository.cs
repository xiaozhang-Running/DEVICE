using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Domain.Interfaces;

public interface IProjectOutboundRepository
{
    Task<ProjectOutbound?> GetByIdAsync(int id);
    Task<ProjectOutbound?> GetByNumberAsync(string number);
    Task<IEnumerable<ProjectOutbound>> GetAllAsync();
    Task<ProjectOutbound> AddAsync(ProjectOutbound outbound);
    Task UpdateAsync(ProjectOutbound outbound);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsAsync(string number);
    Task<IEnumerable<ProjectOutbound>> SearchAsync(string keyword);
    Task<(IEnumerable<ProjectOutbound> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false);
}