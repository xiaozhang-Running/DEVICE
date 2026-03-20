using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Enums;
using DeviceWarehouse.Domain.Interfaces;
using AutoMapper;

namespace DeviceWarehouse.Application.Services;

public class OutboundOrderService : IOutboundOrderService
{
    private readonly IOutboundOrderRepository _outboundOrderRepository;
    private readonly ISpecialEquipmentRepository _specialEquipmentRepository;
    private readonly IGeneralEquipmentRepository _generalEquipmentRepository;
    private readonly IMapper _mapper;

    public OutboundOrderService(
        IOutboundOrderRepository outboundOrderRepository,
        ISpecialEquipmentRepository specialEquipmentRepository,
        IGeneralEquipmentRepository generalEquipmentRepository,
        IMapper mapper)
    {
        _outboundOrderRepository = outboundOrderRepository;
        _specialEquipmentRepository = specialEquipmentRepository;
        _generalEquipmentRepository = generalEquipmentRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OutboundOrderDto>> GetAllAsync()
    {
        var orders = await _outboundOrderRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<OutboundOrderDto>>(orders);
    }

    public async Task<OutboundOrderDto> GetByIdAsync(int id)
    {
        var order = await _outboundOrderRepository.GetByIdAsync(id);
        return _mapper.Map<OutboundOrderDto>(order);
    }

    public async Task<OutboundOrderDto> CreateAsync(CreateOutboundOrderDto dto)
    {
        var order = _mapper.Map<OutboundOrder>(dto);
        order.CreatedAt = DateTime.Now;
        order.Status = OrderStatus.Pending;

        foreach (var item in order.Items)
        {
            if (item.SpecialEquipmentId > 0)
            {
                var equipment = await _specialEquipmentRepository.GetByIdAsync(item.SpecialEquipmentId);
                if (equipment != null && equipment.Quantity >= item.Quantity)
                {
                    equipment.Quantity -= item.Quantity;
                    await _specialEquipmentRepository.UpdateAsync(equipment);
                }
            }
            else if (item.GeneralEquipmentId != null && item.GeneralEquipmentId > 0)
            {
                var equipment = await _generalEquipmentRepository.GetByIdAsync(item.GeneralEquipmentId.Value);
                if (equipment != null && equipment.Quantity >= item.Quantity)
                {
                    equipment.Quantity -= item.Quantity;
                    await _generalEquipmentRepository.UpdateAsync(equipment);
                }
            }
        }

        order.TotalQuantity = order.Items.Sum(item => item.Quantity);
        var createdOrder = await _outboundOrderRepository.AddAsync(order);
        return _mapper.Map<OutboundOrderDto>(createdOrder);
    }

    public async Task UpdateAsync(int id, CreateOutboundOrderDto dto)
    {
        var existingOrder = await _outboundOrderRepository.GetByIdAsync(id);
        if (existingOrder == null)
        {
            return;
        }

        // 恢复库存
        foreach (var item in existingOrder.Items)
        {
            if (item.SpecialEquipmentId > 0)
            {
                var equipment = await _specialEquipmentRepository.GetByIdAsync(item.SpecialEquipmentId);
                if (equipment != null)
                {
                    equipment.Quantity += item.Quantity;
                    await _specialEquipmentRepository.UpdateAsync(equipment);
                }
            }
            else if (item.GeneralEquipmentId != null && item.GeneralEquipmentId > 0)
            {
                var equipment = await _generalEquipmentRepository.GetByIdAsync(item.GeneralEquipmentId.Value);
                if (equipment != null)
                {
                    equipment.Quantity += item.Quantity;
                    await _generalEquipmentRepository.UpdateAsync(equipment);
                }
            }
        }

        // 更新订单
        _mapper.Map(dto, existingOrder);
        existingOrder.TotalQuantity = existingOrder.Items.Sum(item => item.Quantity);

        // 扣减库存
        foreach (var item in existingOrder.Items)
        {
            if (item.SpecialEquipmentId > 0)
            {
                var equipment = await _specialEquipmentRepository.GetByIdAsync(item.SpecialEquipmentId);
                if (equipment != null && equipment.Quantity >= item.Quantity)
                {
                    equipment.Quantity -= item.Quantity;
                    await _specialEquipmentRepository.UpdateAsync(equipment);
                }
            }
            else if (item.GeneralEquipmentId != null && item.GeneralEquipmentId > 0)
            {
                var equipment = await _generalEquipmentRepository.GetByIdAsync(item.GeneralEquipmentId.Value);
                if (equipment != null && equipment.Quantity >= item.Quantity)
                {
                    equipment.Quantity -= item.Quantity;
                    await _generalEquipmentRepository.UpdateAsync(equipment);
                }
            }
        }

        await _outboundOrderRepository.UpdateAsync(existingOrder);
    }

    public async Task DeleteAsync(int id)
    {
        var order = await _outboundOrderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return;
        }

        // 恢复库存
        foreach (var item in order.Items)
        {
            if (item.SpecialEquipmentId > 0)
            {
                var equipment = await _specialEquipmentRepository.GetByIdAsync(item.SpecialEquipmentId);
                if (equipment != null)
                {
                    equipment.Quantity += item.Quantity;
                    await _specialEquipmentRepository.UpdateAsync(equipment);
                }
            }
            else if (item.GeneralEquipmentId != null && item.GeneralEquipmentId > 0)
            {
                var equipment = await _generalEquipmentRepository.GetByIdAsync(item.GeneralEquipmentId.Value);
                if (equipment != null)
                {
                    equipment.Quantity += item.Quantity;
                    await _generalEquipmentRepository.UpdateAsync(equipment);
                }
            }
        }

        await _outboundOrderRepository.DeleteAsync(id);
    }

    public async Task CompleteAsync(int id)
    {
        var order = await _outboundOrderRepository.GetByIdAsync(id);
        if (order == null || order.Status != OrderStatus.Pending)
        {
            return;
        }

        order.Status = OrderStatus.Completed;
        await _outboundOrderRepository.UpdateAsync(order);
    }

    public async Task<bool> ExistsAsync(string code)
    {
        return await _outboundOrderRepository.ExistsAsync(code);
    }
}
