using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Domain.Interfaces;

public interface IRawMaterialRepository
{
    Task<RawMaterial?> GetByIdAsync(int id);
    Task<IEnumerable<RawMaterial>> GetAllAsync();
    Task<RawMaterial> AddAsync(RawMaterial rawMaterial);
    Task UpdateAsync(RawMaterial rawMaterial);
    Task DeleteAsync(int id);
    Task<IEnumerable<RawMaterial>> SearchAsync(string keyword);
    Task<IEnumerable<RawMaterial>> GetAvailableAsync(string? keyword = null);
    Task<(IEnumerable<RawMaterial> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false);
    Task<List<dynamic>> ExecuteRawSqlAsync(string sql, object parameters);
    Task<bool> ExistsAsync(string number);
}