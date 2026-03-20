using AutoMapper;
using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Enums;
using DeviceWarehouse.Domain.Interfaces;

namespace DeviceWarehouse.Application.Services;

public class EquipmentInboundService : IEquipmentInboundService
{
    private readonly IEquipmentInboundRepository _inboundRepository;
    private readonly ISpecialEquipmentRepository _specialEquipmentRepository;
    private readonly IGeneralEquipmentRepository _generalEquipmentRepository;
    private readonly IConsumableRepository _consumableRepository;
    private readonly IMapper _mapper;

    public EquipmentInboundService(
        IEquipmentInboundRepository inboundRepository,
        ISpecialEquipmentRepository specialEquipmentRepository,
        IGeneralEquipmentRepository generalEquipmentRepository,
        IConsumableRepository consumableRepository,
        IMapper mapper)
    {
        _inboundRepository = inboundRepository;
        _specialEquipmentRepository = specialEquipmentRepository;
        _generalEquipmentRepository = generalEquipmentRepository;
        _consumableRepository = consumableRepository;
        _mapper = mapper;
    }

    public async Task<EquipmentInboundDto> GetByIdAsync(int id)
    {
        var inbound = await _inboundRepository.GetByIdAsync(id);
        if (inbound == null)
            throw new Exception("入库单不存在");
        
        var dto = _mapper.Map<EquipmentInboundDto>(inbound);
        dto.TotalQuantity = inbound.Items.Sum(i => i.Quantity);
        dto.EquipmentTypeName = GetEquipmentTypeName(inbound.EquipmentType);
        return dto;
    }

    public async Task<IEnumerable<EquipmentInboundDto>> GetAllAsync()
    {
        try
        {
            var inbounds = await _inboundRepository.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<EquipmentInboundDto>>(inbounds);
            
            foreach (var dto in dtos)
            {
                var inbound = inbounds.First(i => i.Id == dto.Id);
                dto.TotalQuantity = inbound.Items.Sum(i => i.Quantity);
                dto.EquipmentTypeName = GetEquipmentTypeName(inbound.EquipmentType);
                dto.Status = inbound.Status;
            }
            
            return dtos;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetAllAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<EquipmentInboundDto> CreateAsync(CreateEquipmentInboundDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            throw new Exception("入库明细不能为空");

        var inbound = _mapper.Map<EquipmentInbound>(dto);
        // 设置初始状态为待完成
        inbound.Status = "待完成";
        
        foreach (var itemDto in dto.Items)
        {
            // 尝试从 itemDto 中获取 equipmentType 属性
            DeviceType equipmentType = inbound.EquipmentType;
            
            // 尝试通过反射获取 itemDto 的 EquipmentType 属性
            var equipmentTypeProperty = itemDto.GetType().GetProperty("EquipmentType");
            if (equipmentTypeProperty != null)
            {
                var value = equipmentTypeProperty.GetValue(itemDto);
                if (value != null)
                {
                    if (value is DeviceType)
                    {
                        equipmentType = (DeviceType)value;
                    }
                    else if (value is int)
                    {
                        equipmentType = (DeviceType)value;
                    }
                }
            }
            
            var item = new EquipmentInboundItem
            {
                DeviceName = itemDto.DeviceName,
                DeviceCode = itemDto.DeviceCode,
                Brand = itemDto.Brand,
                Model = itemDto.Model,
                SerialNumber = itemDto.SerialNumber,
                Specification = itemDto.Specification,
                Quantity = itemDto.Quantity,
                Unit = itemDto.Unit,
                ImageUrl = itemDto.ImageUrl,
                Status = itemDto.Status,
                Remark = itemDto.Remark,
                EquipmentType = equipmentType,
                CreatedAt = DateTime.Now
            };
            inbound.Items.Add(item);
            
            // 暂不创建设备记录，等到完成时再创建
        }
        
        var createdInbound = await _inboundRepository.AddAsync(inbound);
        var resultDto = _mapper.Map<EquipmentInboundDto>(createdInbound);
        resultDto.TotalQuantity = createdInbound.Items.Sum(i => i.Quantity);
        resultDto.EquipmentTypeName = GetEquipmentTypeName(createdInbound.EquipmentType);
        resultDto.Status = createdInbound.Status;
        return resultDto;
    }

    public async Task<EquipmentInboundDto> UpdateAsync(int id, UpdateEquipmentInboundDto dto)
    {
        var inbound = await _inboundRepository.GetByIdAsync(id);
        if (inbound == null)
            throw new Exception("入库单不存在");

        if (dto.Items == null || !dto.Items.Any())
            throw new Exception("入库明细不能为空");

        // 如果入库单已完成，则不能修改
        if (inbound.Status == "已完成")
            throw new Exception("入库单已完成，不能修改");

        inbound.Items.Clear();

        inbound.InboundNumber = dto.InboundNumber;
        inbound.InboundDate = dto.InboundDate;
        inbound.EquipmentType = dto.EquipmentType;
        inbound.DeliveryPerson = dto.DeliveryPerson;
        inbound.Remark = dto.Remark;
        inbound.Operator = dto.Operator;

        foreach (var itemDto in dto.Items)
        {
            // 尝试从 itemDto 中获取 equipmentType 属性
            DeviceType equipmentType = inbound.EquipmentType;
            
            // 尝试通过反射获取 itemDto 的 EquipmentType 属性
            var equipmentTypeProperty = itemDto.GetType().GetProperty("EquipmentType");
            if (equipmentTypeProperty != null)
            {
                var value = equipmentTypeProperty.GetValue(itemDto);
                if (value != null)
                {
                    if (value is DeviceType)
                    {
                        equipmentType = (DeviceType)value;
                    }
                    else if (value is int)
                    {
                        equipmentType = (DeviceType)value;
                    }
                }
            }
            
            var item = new EquipmentInboundItem
            {
                DeviceName = itemDto.DeviceName,
                DeviceCode = itemDto.DeviceCode,
                Brand = itemDto.Brand,
                Model = itemDto.Model,
                SerialNumber = itemDto.SerialNumber,
                Specification = itemDto.Specification,
                Quantity = itemDto.Quantity,
                Unit = itemDto.Unit,
                ImageUrl = itemDto.ImageUrl,
                Status = itemDto.Status,
                Remark = itemDto.Remark,
                EquipmentType = equipmentType,
                CreatedAt = DateTime.Now
            };
            inbound.Items.Add(item);
            
            // 暂不创建设备记录，等到完成时再创建
        }
        
        await _inboundRepository.UpdateAsync(inbound);
        var resultDto = _mapper.Map<EquipmentInboundDto>(inbound);
        resultDto.TotalQuantity = inbound.Items.Sum(i => i.Quantity);
        resultDto.EquipmentTypeName = GetEquipmentTypeName(inbound.EquipmentType);
        resultDto.Status = inbound.Status;
        return resultDto;
    }

    public async Task CompleteAsync(int id)
    {
        var inbound = await _inboundRepository.GetByIdAsync(id);
        if (inbound == null)
            throw new Exception("入库单不存在");

        if (inbound.Status == "已完成")
            throw new Exception("入库单已完成，不能重复完成");

        foreach (var item in inbound.Items)
        {
            var itemDto = new CreateEquipmentInboundItemDto
            {
                DeviceName = item.DeviceName,
                DeviceCode = item.DeviceCode,
                Brand = item.Brand,
                Model = item.Model,
                SerialNumber = item.SerialNumber,
                Specification = item.Specification,
                Quantity = item.Quantity,
                Unit = item.Unit,
                // 不再设置 ImageUrl 字段，使用 Images 集合
                Status = item.Status,
                Remark = item.Remark
            };
            
            // 尝试从 item 中获取 equipmentType 属性
            // 如果没有，则使用入库单的 EquipmentType
            DeviceType equipmentType = inbound.EquipmentType;
            
            // 尝试通过反射获取 item 的 EquipmentType 属性
            var equipmentTypeProperty = item.GetType().GetProperty("EquipmentType");
            if (equipmentTypeProperty != null)
            {
                var value = equipmentTypeProperty.GetValue(item);
                if (value != null)
                {
                    if (value is DeviceType)
                    {
                        equipmentType = (DeviceType)value;
                    }
                    else if (value is int)
                    {
                        equipmentType = (DeviceType)value;
                    }
                }
            }
            
            await CreateEquipmentRecord(equipmentType, itemDto, null);
        }

        inbound.Status = "已完成";
        inbound.CompletedAt = DateTime.Now;
        inbound.UpdatedAt = DateTime.Now;
        
        await _inboundRepository.UpdateAsync(inbound);
    }

    public async Task<IEnumerable<EquipmentInboundDto>> GetSummaryAsync()
    {
        var inbounds = await _inboundRepository.GetAllAsync();
        var dtos = _mapper.Map<IEnumerable<EquipmentInboundDto>>(inbounds);
        
        foreach (var dto in dtos)
        {
            var inbound = inbounds.First(i => i.Id == dto.Id);
            dto.TotalQuantity = inbound.Items.Sum(i => i.Quantity);
            dto.EquipmentTypeName = GetEquipmentTypeName(inbound.EquipmentType);
        }
        
        return dtos;
    }

    public async Task DeleteAsync(int id)
    {
        var inbound = await _inboundRepository.GetByIdAsync(id);
        if (inbound == null)
            throw new Exception("入库单不存在");

        // 如果入库单已完成，则需要删除对应的设备记录
        if (inbound.Status == "已完成")
        {
            foreach (var item in inbound.Items)
            {
                await DeleteEquipmentRecord(inbound.EquipmentType, item.DeviceCode);
            }
        }

        await _inboundRepository.DeleteAsync(id);
    }

    public async Task<bool> ExistsAsync(string number)
    {
        return await _inboundRepository.ExistsAsync(number);
    }

    private async Task CreateEquipmentRecord(DeviceType equipmentType, CreateEquipmentInboundItemDto item, string? company)
    {
        switch (equipmentType)
        {
            case DeviceType.SpecialDevice:
                // 检查是否已存在相同名称、品牌、型号的专用设备
                var existingSpecialEquipments = await _specialEquipmentRepository.GetAllAsync();
                var existingSpecialEquipment = existingSpecialEquipments.FirstOrDefault(e => 
                    e.DeviceName == item.DeviceName && 
                    e.Brand == item.Brand && 
                    e.Model == item.Model
                );
                
                if (existingSpecialEquipment != null)
                {
                    // 如果已存在，更新数量
                    existingSpecialEquipment.Quantity += item.Quantity;
                    existingSpecialEquipment.UpdatedAt = DateTime.Now;
                    await _specialEquipmentRepository.UpdateAsync(existingSpecialEquipment);
                }
                else
                {
                    // 否则，创建新记录
                    var specialEquipment = new SpecialEquipment
                    {
                        DeviceName = item.DeviceName,
                        DeviceCode = item.DeviceCode,
                        Brand = item.Brand,
                        Model = item.Model,
                        SerialNumber = item.SerialNumber,
                        Specification = item.Specification,
                        Quantity = item.Quantity,
                        Unit = item.Unit ?? "台",
                        // 不再设置 ImageUrl 字段，使用 Images 集合
                        Status = item.Status,
                        Remark = item.Remark,
                        DeviceType = DeviceType.SpecialDevice,
                        DeviceStatus = DeviceStatus.Normal,
                        UseStatus = UsageStatus.Unused,
                        CreatedAt = DateTime.Now
                    };
                    await _specialEquipmentRepository.AddAsync(specialEquipment);
                }
                break;
                
            case DeviceType.GeneralDevice:
                // 检查是否已存在相同名称、品牌、型号的通用设备
                var existingGeneralEquipments = await _generalEquipmentRepository.GetAllAsync();
                var existingGeneralEquipment = existingGeneralEquipments.FirstOrDefault(e => 
                    e.DeviceName == item.DeviceName && 
                    e.Brand == item.Brand && 
                    e.Model == item.Model
                );
                
                if (existingGeneralEquipment != null)
                {
                    // 如果已存在，更新数量
                    existingGeneralEquipment.Quantity += item.Quantity;
                    existingGeneralEquipment.UpdatedAt = DateTime.Now;
                    await _generalEquipmentRepository.UpdateAsync(existingGeneralEquipment);
                }
                else
                {
                    // 否则，创建新记录
                    var generalEquipment = new GeneralEquipment
                    {
                        DeviceName = item.DeviceName,
                        DeviceCode = item.DeviceCode,
                        Brand = item.Brand,
                        Model = item.Model,
                        SerialNumber = item.SerialNumber,
                        Specification = item.Specification,
                        Quantity = item.Quantity,
                        Unit = item.Unit ?? "台",
                        // 不再设置 ImageUrl 字段，使用 Images 集合
                        Status = item.Status,
                        Remark = item.Remark,
                        DeviceType = DeviceType.GeneralDevice,
                        DeviceStatus = DeviceStatus.Normal,
                        UseStatus = UsageStatus.Unused,
                        CreatedAt = DateTime.Now
                    };
                    await _generalEquipmentRepository.AddAsync(generalEquipment);
                }
                break;
                
            case DeviceType.Consumable:
                // 检查是否是相同的耗材
                var existingConsumables = await _consumableRepository.GetAllAsync();
                var existingConsumable = existingConsumables.FirstOrDefault(c => 
                    c.Name == item.DeviceName && 
                    c.Brand == item.Brand && 
                    c.ModelSpecification == $"{item.Model} {item.Specification}".Trim()
                );
                
                if (existingConsumable != null)
                {
                    // 如果是相同的耗材，直接更新剩余数量
                    existingConsumable.TotalQuantity += item.Quantity;
                    existingConsumable.RemainingQuantity += item.Quantity;
                    existingConsumable.UpdatedAt = DateTime.Now;
                    await _consumableRepository.UpdateAsync(existingConsumable);
                }
                else
                {
                    // 否则，增加一条新的耗材记录
                    var consumable = new Consumable
                    {
                        Name = item.DeviceName,
                        Brand = item.Brand,
                        ModelSpecification = $"{item.Model} {item.Specification}".Trim(),
                        TotalQuantity = item.Quantity,
                        UsedQuantity = 0,
                        RemainingQuantity = item.Quantity,
                        Unit = item.Unit ?? "件",
                        Status = item.Status,
                        Remark = item.Remark,
                        Image = item.ImageUrl,
                        CreatedAt = DateTime.Now
                    };
                    await _consumableRepository.AddAsync(consumable);
                }
                break;
        }
    }

    private async Task DeleteEquipmentRecord(DeviceType equipmentType, string deviceCode)
    {
        switch (equipmentType)
        {
            case DeviceType.SpecialDevice:
                var specialEquipments = await _specialEquipmentRepository.GetAllAsync();
                var specialEquipment = specialEquipments.FirstOrDefault(e => e.DeviceCode == deviceCode);
                if (specialEquipment != null)
                    await _specialEquipmentRepository.DeleteAsync(specialEquipment.Id);
                break;
                
            case DeviceType.GeneralDevice:
                var generalEquipments = await _generalEquipmentRepository.GetAllAsync();
                var generalEquipment = generalEquipments.FirstOrDefault(e => e.DeviceCode == deviceCode);
                if (generalEquipment != null)
                    await _generalEquipmentRepository.DeleteAsync(generalEquipment.Id);
                break;
                
            case DeviceType.Consumable:
                var consumables = await _consumableRepository.GetAllAsync();
                var consumable = consumables.FirstOrDefault(c => c.Name == deviceCode);
                if (consumable != null)
                    await _consumableRepository.DeleteAsync(consumable.Id);
                break;
        }
    }

    private string GetEquipmentTypeName(DeviceType equipmentType)
    {
        return equipmentType switch
        {
            DeviceType.SpecialDevice => "专用设备",
            DeviceType.GeneralDevice => "通用设备",
            DeviceType.Consumable => "耗材",
            _ => "未知"
        };
    }
}
