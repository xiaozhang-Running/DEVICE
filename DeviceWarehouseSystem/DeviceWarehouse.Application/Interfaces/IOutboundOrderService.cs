using DeviceWarehouse.Application.DTOs;

namespace DeviceWarehouse.Application.Interfaces;

public interface IOutboundOrderService
{
    Task<IEnumerable<OutboundOrderDto>> GetAllAsync();
    Task<OutboundOrderDto> GetByIdAsync(int id);
    Task<OutboundOrderDto> CreateAsync(CreateOutboundOrderDto dto);
    Task UpdateAsync(int id, CreateOutboundOrderDto dto);
    Task DeleteAsync(int id);
    Task CompleteAsync(int id);
    Task<bool> ExistsAsync(string code);
}