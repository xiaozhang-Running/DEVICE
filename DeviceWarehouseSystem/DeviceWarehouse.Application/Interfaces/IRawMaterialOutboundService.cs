using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Application.Interfaces;

public interface IRawMaterialOutboundService : IService<RawMaterialOutbound, RawMaterialOutboundDto, CreateRawMaterialOutboundDto, UpdateRawMaterialOutboundDto, RawMaterialOutboundDto>
{
    new Task CompleteAsync(int id);
    Task<IEnumerable<AvailableItemDto>> GetAvailableItemsAsync(string? keyword = null);
    Task<AvailableItemsResponseDto> GetAvailableItemsPagedAsync(AvailableItemsRequestDto request);
    Task<IEnumerable<ProjectOutboundDto>> SearchOutboundsAsync(string keyword);
    Task<string> GenerateOutboundNumberAsync();
}
