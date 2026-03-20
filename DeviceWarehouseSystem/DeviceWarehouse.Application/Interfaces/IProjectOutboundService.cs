using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Application.Interfaces;

public interface IProjectOutboundService : IService<ProjectOutbound, ProjectOutboundDto, CreateProjectOutboundDto, UpdateProjectOutboundDto, ProjectOutboundDto>
{
    new Task CompleteAsync(int id);
    Task<IEnumerable<AvailableItemDto>> GetAvailableItemsAsync(string? keyword = null);
    Task<AvailableItemsResponseDto> GetAvailableItemsPagedAsync(AvailableItemsRequestDto request);
    Task<IEnumerable<ProjectOutboundDto>> SearchOutboundsAsync(string keyword);
}
