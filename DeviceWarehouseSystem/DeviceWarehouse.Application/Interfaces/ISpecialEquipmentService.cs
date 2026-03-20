using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Enums;

namespace DeviceWarehouse.Application.Interfaces;

public interface ISpecialEquipmentService : IService<SpecialEquipment, SpecialEquipmentDto, CreateSpecialEquipmentDto, UpdateSpecialEquipmentDto, SpecialEquipmentSummaryDto>
{
    Task<IEnumerable<SpecialEquipmentDto>> GetByTypeAsync(DeviceType type);
    Task<IEnumerable<SpecialEquipmentSummaryDto>> GetEquipmentSummaryAsync(DeviceType? type);
    Task<IEnumerable<SpecialEquipmentSummaryDto>> GetSpecialEquipmentSummaryAsync(DeviceType? type);
    Task<IEnumerable<SpecialEquipmentDto>> SearchAsync(string keyword);
    Task<object> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false, DeviceStatus? deviceStatus = null, UsageStatus? useStatus = null, string? brand = null);
    Task<object> GetGroupedPagedAsync(int pageNumber, int pageSize, string? keyword = null);
    Task<object> CreateBatchAsync(IEnumerable<CreateSpecialEquipmentDto> dtos);
    Task<object> DeleteAllAsync();
    Task<IEnumerable<object>> GetInventorySummaryAsync(int? deviceType, int? status);
    Task<IEnumerable<object>> GetOutboundReportAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<object>> GetDeviceUsageAnalysisAsync(int months);
    new Task<bool> ExistsAsync(string number);
}
