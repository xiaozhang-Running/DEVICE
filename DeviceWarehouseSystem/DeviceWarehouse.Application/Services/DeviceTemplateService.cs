using System;
using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Domain.Interfaces;

namespace DeviceWarehouse.Application.Services;

public class DeviceTemplateService : IDeviceTemplateService
{
    private readonly IDeviceTemplateRepository _repository;

    public DeviceTemplateService(IDeviceTemplateRepository repository)
    {
        _repository = repository;
    }

    public Task<object> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<object>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<object> CreateAsync(object createDto)
    {
        throw new NotImplementedException();
    }

    public Task<object> UpdateAsync(int id, object updateDto)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<List<DeviceTemplateDto>> GetSpecialEquipmentTemplatesAsync()
    {
        return await _repository.GetSpecialEquipmentTemplatesAsync();
    }

    public async Task<List<DeviceTemplateDto>> GetGeneralEquipmentTemplatesAsync()
    {
        return await _repository.GetGeneralEquipmentTemplatesAsync();
    }

    public async Task<List<ConsumableTemplateDto>> GetConsumableTemplatesAsync()
    {
        return await _repository.GetConsumableTemplatesAsync();
    }
}
