using AutoMapper;
using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;

namespace DeviceWarehouse.Application.Services;

public class RawMaterialInboundService : IRawMaterialInboundService
{
    private readonly IRawMaterialInboundRepository _inboundRepository;
    private readonly IRawMaterialRepository _rawMaterialRepository;
    private readonly IMapper _mapper;

    public RawMaterialInboundService(
        IRawMaterialInboundRepository inboundRepository,
        IRawMaterialRepository rawMaterialRepository,
        IMapper mapper)
    {
        _inboundRepository = inboundRepository;
        _rawMaterialRepository = rawMaterialRepository;
        _mapper = mapper;
    }

    public async Task<RawMaterialInboundDto> GetByIdAsync(int id)
    {
        var inbound = await _inboundRepository.GetByIdAsync(id);
        if (inbound == null)
            throw new Exception("入库单不存在");
        
        var dto = _mapper.Map<RawMaterialInboundDto>(inbound);
        dto.TotalQuantity = inbound.Items.Sum(i => i.Quantity);
        return dto;
    }

    public async Task<IEnumerable<RawMaterialInboundDto>> GetAllAsync()
    {
        var inbounds = await _inboundRepository.GetAllAsync();
        var dtos = _mapper.Map<IEnumerable<RawMaterialInboundDto>>(inbounds);
        
        foreach (var dto in dtos)
        {
            var inbound = inbounds.First(i => i.Id == dto.Id);
            dto.TotalQuantity = inbound.Items?.Sum(i => i.Quantity) ?? 0;
        }
        
        return dtos;
    }

    private async Task<RawMaterial> GetOrCreateRawMaterialAsync(int? rawMaterialId, string? productName, string? specification)
    {
        RawMaterial? rawMaterial = null;
        
        // 如果提供了ID，先尝试通过ID查找
        if (rawMaterialId.HasValue && rawMaterialId.Value > 0)
        {
            rawMaterial = await _rawMaterialRepository.GetByIdAsync(rawMaterialId.Value);
            if (rawMaterial != null)
                return rawMaterial;
        }
        
        // 如果没有找到，尝试通过名称和规格查找
        if (!string.IsNullOrWhiteSpace(productName))
        {
            var allMaterials = await _rawMaterialRepository.GetAllAsync();
            // 过滤掉ID为0的记录，因为它可能是一个错误的记录
            var validMaterials = allMaterials.Where(r => r.Id > 0).ToList();
            
            // 优先查找名称和规格都匹配的记录
            rawMaterial = validMaterials.FirstOrDefault(r => 
                r.ProductName.Equals(productName, StringComparison.OrdinalIgnoreCase) &&
                (r.Specification == specification || (r.Specification != null && specification != null && r.Specification.Equals(specification, StringComparison.OrdinalIgnoreCase))));
            
            // 如果没有找到，再尝试只匹配名称
            if (rawMaterial == null)
            {
                rawMaterial = validMaterials.FirstOrDefault(r => 
                    r.ProductName.Equals(productName, StringComparison.OrdinalIgnoreCase));
            }
            
            if (rawMaterial != null)
                return rawMaterial;
            
            // 如果名称也不存在，创建新原材料
            rawMaterial = new RawMaterial
            {
                ProductName = productName.Trim(),
                Specification = specification,
                TotalQuantity = 0,
                UsedQuantity = 0,
                RemainingQuantity = 0,
                Unit = "件",
                CreatedAt = DateTime.Now
            };
            
            return await _rawMaterialRepository.AddAsync(rawMaterial);
        }
        
        throw new Exception("必须提供原材料ID或原材料名称");
    }

    public async Task<RawMaterialInboundDto> CreateAsync(CreateRawMaterialInboundDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            throw new Exception("入库明细不能为空");

        // 如果没有提供入库单号，自动生成
        if (string.IsNullOrEmpty(dto.InboundNumber))
        {
            var prefix = "RK";
            var date = DateTime.Now.ToString("yyyyMMdd");
            var random = new Random();
            var randomPart = random.Next(10000).ToString("D4");
            dto.InboundNumber = $"{prefix}{date}{randomPart}";
        }

        var inbound = _mapper.Map<RawMaterialInbound>(dto);
        
        foreach (var itemDto in dto.Items)
        {
            var rawMaterial = await GetOrCreateRawMaterialAsync(itemDto.RawMaterialId, itemDto.ProductName, itemDto.Specification);
            
            var item = new RawMaterialInboundItem
            {
                RawMaterialId = rawMaterial.Id,
                Quantity = itemDto.Quantity,
                Specification = itemDto.Specification,
                Remark = itemDto.Remark,
                CreatedAt = DateTime.Now
            };
            inbound.Items.Add(item);
        }
        
        var createdInbound = await _inboundRepository.AddAsync(inbound);
        var resultDto = _mapper.Map<RawMaterialInboundDto>(createdInbound);
        resultDto.TotalQuantity = createdInbound.Items.Sum(i => i.Quantity);
        return resultDto;
    }

    public async Task<RawMaterialInboundDto> UpdateAsync(int id, UpdateRawMaterialInboundDto dto)
    {
        var inbound = await _inboundRepository.GetByIdAsync(id);
        if (inbound == null)
            throw new Exception("入库单不存在");

        if (dto.Items == null || !dto.Items.Any())
            throw new Exception("入库明细不能为空");

        // 恢复原有库存
        foreach (var oldItem in inbound.Items)
        {
            var rawMaterial = await _rawMaterialRepository.GetByIdAsync(oldItem.RawMaterialId);
            if (rawMaterial != null)
            {
                rawMaterial.TotalQuantity -= oldItem.Quantity;
                rawMaterial.RemainingQuantity = rawMaterial.TotalQuantity - rawMaterial.UsedQuantity;
                await _rawMaterialRepository.UpdateAsync(rawMaterial);
            }
        }

        // 清除旧明细
        inbound.Items.Clear();

        // 更新入库单基本信息
        inbound.InboundNumber = dto.InboundNumber;
        inbound.InboundDate = dto.InboundDate;
        inbound.DeliveryPerson = dto.DeliveryPerson;
        inbound.Remark = dto.Remark;
        inbound.Operator = dto.Operator;

        // 添加新明细并更新库存
        foreach (var itemDto in dto.Items)
        {
            var rawMaterial = await GetOrCreateRawMaterialAsync(itemDto.RawMaterialId, itemDto.ProductName, itemDto.Specification);
            
            var item = new RawMaterialInboundItem
            {
                RawMaterialId = rawMaterial.Id,
                Quantity = itemDto.Quantity,
                Specification = itemDto.Specification,
                Remark = itemDto.Remark,
                CreatedAt = DateTime.Now
            };
            inbound.Items.Add(item);
            
            rawMaterial.TotalQuantity += itemDto.Quantity;
            rawMaterial.RemainingQuantity = rawMaterial.TotalQuantity - rawMaterial.UsedQuantity;
            await _rawMaterialRepository.UpdateAsync(rawMaterial);
        }
        
        await _inboundRepository.UpdateAsync(inbound);
        var resultDto = _mapper.Map<RawMaterialInboundDto>(inbound);
        resultDto.TotalQuantity = inbound.Items.Sum(i => i.Quantity);
        return resultDto;
    }

    public async Task<IEnumerable<RawMaterialInboundDto>> GetSummaryAsync()
    {
        var inbounds = await _inboundRepository.GetAllAsync();
        var dtos = _mapper.Map<IEnumerable<RawMaterialInboundDto>>(inbounds);
        
        foreach (var dto in dtos)
        {
            var inbound = inbounds.First(i => i.Id == dto.Id);
            dto.TotalQuantity = inbound.Items.Sum(i => i.Quantity);
        }
        
        return dtos;
    }

    public async Task DeleteAsync(int id)
    {
        var inbound = await _inboundRepository.GetByIdAsync(id);
        if (inbound == null)
            throw new Exception("入库单不存在");

        // 恢复库存
        foreach (var item in inbound.Items)
        {
            var rawMaterial = await _rawMaterialRepository.GetByIdAsync(item.RawMaterialId);
            if (rawMaterial != null)
            {
                rawMaterial.TotalQuantity -= item.Quantity;
                rawMaterial.RemainingQuantity = rawMaterial.TotalQuantity - rawMaterial.UsedQuantity;
                await _rawMaterialRepository.UpdateAsync(rawMaterial);
            }
        }

        await _inboundRepository.DeleteAsync(id);
    }

    public async Task<bool> ExistsAsync(string number)
    {
        return await _inboundRepository.ExistsAsync(number);
    }

    public async Task CompleteAsync(int id)
    {
        var inbound = await _inboundRepository.GetByIdAsync(id);
        if (inbound == null)
            throw new Exception("入库单不存在");

        // 设置状态为已完成
        inbound.Status = "completed";
        inbound.UpdatedAt = DateTime.Now;

        // 更新库存
        foreach (var item in inbound.Items)
        {
            var rawMaterial = await _rawMaterialRepository.GetByIdAsync(item.RawMaterialId);
            if (rawMaterial != null)
            {
                // 增加库存
                rawMaterial.TotalQuantity += item.Quantity;
                rawMaterial.RemainingQuantity = rawMaterial.TotalQuantity - rawMaterial.UsedQuantity;
                await _rawMaterialRepository.UpdateAsync(rawMaterial);
            }
        }

        await _inboundRepository.UpdateAsync(inbound);
    }
}
