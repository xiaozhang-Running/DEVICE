using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Application.Interfaces;

public interface IRawMaterialService : IService<RawMaterial, RawMaterialDto, CreateRawMaterialDto, UpdateRawMaterialDto, RawMaterialDto>
{
    Task<IEnumerable<RawMaterialDto>> SearchAsync(string keyword);
    Task<object> DeleteAllAsync();
    Task<object> CreateBatchAsync(IEnumerable<CreateRawMaterialDto> dtos);
}
