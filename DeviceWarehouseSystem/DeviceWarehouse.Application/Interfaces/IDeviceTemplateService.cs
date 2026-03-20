using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;

namespace DeviceWarehouse.Application.Interfaces;

public interface IDeviceTemplateService
{
    Task<object> GetByIdAsync(int id);
    Task<IEnumerable<object>> GetAllAsync();
    Task<object> CreateAsync(object createDto);
    Task<object> UpdateAsync(int id, object updateDto);
    Task DeleteAsync(int id);
    Task<List<DeviceTemplateDto>> GetSpecialEquipmentTemplatesAsync();
    Task<List<DeviceTemplateDto>> GetGeneralEquipmentTemplatesAsync();
    Task<List<ConsumableTemplateDto>> GetConsumableTemplatesAsync();
}
