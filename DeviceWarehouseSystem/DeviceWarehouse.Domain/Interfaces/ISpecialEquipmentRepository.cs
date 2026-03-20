using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Enums;

namespace DeviceWarehouse.Domain.Interfaces;

public interface ISpecialEquipmentRepository
{
    Task<SpecialEquipment?> GetByIdAsync(int id);
    Task<SpecialEquipment?> GetByCodeAsync(string code);
    Task<IEnumerable<SpecialEquipment>> GetAllAsync();
    Task<IEnumerable<SpecialEquipment>> GetByTypeAsync(DeviceType type);
    Task<SpecialEquipment> AddAsync(SpecialEquipment specialEquipment);
    Task UpdateAsync(SpecialEquipment specialEquipment);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(string code);
    Task<IEnumerable<SpecialEquipment>> SearchAsync(string keyword);
    Task<IEnumerable<SpecialEquipment>> GetAvailableAsync(string? keyword = null);
    Task<IEnumerable<SpecialEquipment>> GetByIdsAsync(IEnumerable<int> ids);
    Task<(IEnumerable<SpecialEquipment> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false, DeviceStatus? deviceStatus = null, UsageStatus? useStatus = null, string? brand = null);
    Task<(IEnumerable<SpecialEquipment> Items, int TotalCount)> GetAvailablePagedAsync(string? keyword, int pageNumber, int pageSize);
    Task<List<dynamic>> ExecuteRawSqlAsync(string sql, object parameters);
    Task<(IEnumerable<SpecialEquipment> Items, int TotalCount)> GetGroupedPagedAsync(string? keyword, int pageNumber, int pageSize);
    Task<int> CountByNameAsync(string deviceName);
    Task DeleteAllAsync();
}