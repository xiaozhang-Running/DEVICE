using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Application.Interfaces;

public interface IConsumableService : IService<Consumable, ConsumableDto, CreateConsumableDto, UpdateConsumableDto, ConsumableDto>
{
    Task<IEnumerable<ConsumableDto>> SearchAsync(string keyword);
    Task<object> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false);
    Task<object> DeleteAllAsync();
    Task<object> CreateBatchAsync(IEnumerable<CreateConsumableDto> dtos);
}
