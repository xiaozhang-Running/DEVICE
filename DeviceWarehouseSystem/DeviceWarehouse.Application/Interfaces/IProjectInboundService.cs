using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Application.Interfaces;

public interface IProjectInboundService : IService<ProjectInbound, ProjectInboundDto, CreateProjectInboundDto, UpdateProjectInboundDto, ProjectInboundDto>
{
    Task<IEnumerable<ProjectOutboundDto>> GetAvailableProjectOutbounds();
    Task<IEnumerable<ProjectOutboundItemDto>> GetUninboundItemsByOutboundIdAsync(int outboundId);
    Task<ProjectOutboundDto> GetProjectOutboundByIdAsync(int id);
    new Task CompleteAsync(int id);
    Task PartialInboundAsync(int id);
}
