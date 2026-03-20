using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Domain.Enums;

namespace DeviceWarehouse.Application.Interfaces;

public interface IScrapEquipmentService
{
    Task<ScrapEquipmentDto> GetByIdAsync(int id);
    Task<IEnumerable<ScrapEquipmentDto>> GetAllAsync();
    Task<IEnumerable<ScrapEquipmentDto>> GetByDeviceTypeAsync(DeviceType deviceType);
    Task<ScrapEquipmentDto> CreateAsync(CreateScrapEquipmentDto dto);
    Task UpdateAsync(int id, UpdateScrapEquipmentDto dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<ScrapEquipmentDto>> SearchAsync(string keyword);
    Task<object> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false);
    Task<bool> ScrapDeviceAsync(string deviceCode, string scrapReason, string scrappedBy);
}