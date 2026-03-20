using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Domain.Interfaces;

public interface IConsumableRepository
{
    Task<Consumable?> GetByIdAsync(int id);
    Task<IEnumerable<Consumable>> GetAllAsync();
    Task<Consumable> AddAsync(Consumable consumable);
    Task UpdateAsync(Consumable consumable);
    Task DeleteAsync(int id);
    Task<IEnumerable<Consumable>> SearchAsync(string keyword);
    Task<IEnumerable<Consumable>> GetAvailableAsync(string? keyword = null);
    Task<(IEnumerable<Consumable> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false);
    Task<List<dynamic>> ExecuteRawSqlAsync(string sql, object parameters);
    Task<(IEnumerable<Consumable> Items, int TotalCount)> GetGroupedPagedAsync(string? keyword, int pageNumber, int pageSize);
    Task<bool> ExistsAsync(string number);
}