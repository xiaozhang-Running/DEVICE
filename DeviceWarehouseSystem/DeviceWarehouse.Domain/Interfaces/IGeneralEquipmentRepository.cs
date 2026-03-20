using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Enums;

namespace DeviceWarehouse.Domain.Interfaces;

public interface IGeneralEquipmentRepository
{
    Task<GeneralEquipment?> GetByIdAsync(int id);
    Task<GeneralEquipment?> GetByCodeAsync(string code);
    Task<IEnumerable<GeneralEquipment>> GetAllAsync();
    Task<IEnumerable<GeneralEquipment>> GetByTypeAsync(DeviceType type);
    Task<GeneralEquipment> AddAsync(GeneralEquipment generalEquipment);
    Task UpdateAsync(GeneralEquipment generalEquipment);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(string code);
    Task<IEnumerable<GeneralEquipment>> SearchAsync(string keyword);
    Task<IEnumerable<GeneralEquipment>> GetAvailableAsync(string? keyword = null);
    Task<IEnumerable<GeneralEquipment>> GetByIdsAsync(IEnumerable<int> ids);
    Task<(IEnumerable<GeneralEquipment> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false, DeviceStatus? deviceStatus = null, UsageStatus? useStatus = null, string? brand = null);
    Task<(IEnumerable<GeneralEquipment> Items, int TotalCount)> GetAvailablePagedAsync(string? keyword, int pageNumber, int pageSize);
    Task<List<dynamic>> ExecuteRawSqlAsync(string sql, object parameters);
    Task<(IEnumerable<GeneralEquipment> Items, int TotalCount)> GetGroupedPagedAsync(string? keyword, int pageNumber, int pageSize);
    Task<int> CountByNameAsync(string deviceName);
    Task DeleteAllAsync();
}