using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Application.Interfaces;

public interface IRawMaterialInboundService : IService<RawMaterialInbound, RawMaterialInboundDto, CreateRawMaterialInboundDto, UpdateRawMaterialInboundDto, RawMaterialInboundDto>
{
}
