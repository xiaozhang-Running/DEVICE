using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Application.Interfaces;

public interface IEquipmentInboundService : IService<EquipmentInbound, EquipmentInboundDto, CreateEquipmentInboundDto, UpdateEquipmentInboundDto, EquipmentInboundDto>
{
    new Task CompleteAsync(int id);
}
