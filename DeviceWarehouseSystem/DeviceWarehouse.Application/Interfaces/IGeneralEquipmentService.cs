using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Enums;

namespace DeviceWarehouse.Application.Interfaces;

public interface IGeneralEquipmentService : IService<GeneralEquipment, GeneralEquipmentDto, CreateGeneralEquipmentDto, UpdateGeneralEquipmentDto, GeneralEquipmentSummaryDto>
{
    Task<IEnumerable<GeneralEquipmentDto>> GetByTypeAsync(DeviceType type);
    Task<IEnumerable<GeneralEquipmentSummaryDto>> GetEquipmentSummaryAsync(DeviceType? type);
    Task<IEnumerable<GeneralEquipmentDto>> SearchAsync(string keyword);
    Task<object> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false, DeviceStatus? deviceStatus = null, UsageStatus? useStatus = null, string? brand = null);
    Task<object> GetGroupedPagedAsync(int pageNumber, int pageSize, string? keyword = null);
    Task<object> CreateBatchAsync(IEnumerable<CreateGeneralEquipmentDto> dtos);
    Task<object> DeleteBatchAsync(IEnumerable<int> ids);
    Task<object> DeleteAllAsync();
}
