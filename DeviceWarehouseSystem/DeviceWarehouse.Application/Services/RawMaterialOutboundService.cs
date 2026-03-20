using AutoMapper;
using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;
using System;

namespace DeviceWarehouse.Application.Services;

public class RawMaterialOutboundService : IRawMaterialOutboundService
{
    private readonly IRawMaterialOutboundRepository _outboundRepository;
    private readonly IRawMaterialRepository _rawMaterialRepository;
    private readonly IMapper _mapper;

    public RawMaterialOutboundService(
        IRawMaterialOutboundRepository outboundRepository,
        IRawMaterialRepository rawMaterialRepository,
        IMapper mapper)
    {
        _outboundRepository = outboundRepository;
        _rawMaterialRepository = rawMaterialRepository;
        _mapper = mapper;
    }

    public async Task<RawMaterialOutboundDto> GetByIdAsync(int id)
    {
        var outbound = await _outboundRepository.GetByIdAsync(id);
        if (outbound == null)
            throw new Exception("出库单不存在");
        
        var dto = _mapper.Map<RawMaterialOutboundDto>(outbound);
        dto.TotalQuantity = outbound.Items.Sum(i => i.Quantity);
        return dto;
    }

    public async Task<IEnumerable<RawMaterialOutboundDto>> GetAllAsync()
    {
        var outbounds = await _outboundRepository.GetAllAsync();
        var dtos = _mapper.Map<IEnumerable<RawMaterialOutboundDto>>(outbounds);
        
        foreach (var dto in dtos)
        {
            var outbound = outbounds.First(i => i.Id == dto.Id);
            dto.TotalQuantity = outbound.Items.Sum(i => i.Quantity);
        }
        
        return dtos;
    }

    private async Task<RawMaterial> GetRawMaterialAsync(int? rawMaterialId, string? productName)
    {
        RawMaterial? rawMaterial = null;
        
        // 如果提供了ID，先尝试通过ID查找
        if (rawMaterialId.HasValue && rawMaterialId.Value > 0)
        {
            rawMaterial = await _rawMaterialRepository.GetByIdAsync(rawMaterialId.Value);
            if (rawMaterial != null)
                return rawMaterial;
        }
        
        // 如果没有找到，尝试通过名称查找
        if (!string.IsNullOrWhiteSpace(productName))
        {
            var allMaterials = await _rawMaterialRepository.GetAllAsync();
            rawMaterial = allMaterials.FirstOrDefault(r => 
                r.ProductName.Equals(productName, StringComparison.OrdinalIgnoreCase));
            
            if (rawMaterial != null)
                return rawMaterial;
            
            // 如果通过名称也找不到，创建一个新的原材料记录
            // 获取当前最大的SortOrder值
            var maxSortOrder = allMaterials.Any() ? allMaterials.Max(m => m.SortOrder) : 0;
            
            rawMaterial = new RawMaterial
            {
                ProductName = productName,
                SortOrder = maxSortOrder + 1,
                TotalQuantity = 0,
                UsedQuantity = 0,
                RemainingQuantity = 0,
                CreatedAt = DateTime.Now
            };
            
            rawMaterial = await _rawMaterialRepository.AddAsync(rawMaterial);
            return rawMaterial;
        }
        
        throw new Exception("原材料不存在");
    }

    public async Task<RawMaterialOutboundDto> CreateAsync(CreateRawMaterialOutboundDto dto)
    {
        if (dto.Items == null || !dto.Items.Any())
            throw new Exception("出库明细不能为空");

        var outbound = _mapper.Map<RawMaterialOutbound>(dto);
        
        // 设置初始状态为待完成
        outbound.Status = "待完成";
        outbound.CreatedAt = DateTime.Now;
        
        foreach (var itemDto in dto.Items)
        {
            var rawMaterial = await GetRawMaterialAsync(itemDto.RawMaterialId, itemDto.ProductName);
            
            // 检查库存是否足够，但允许新创建的原材料出库
            // 新创建的原材料RemainingQuantity为0
            if (rawMaterial.RemainingQuantity > 0 && rawMaterial.RemainingQuantity < itemDto.Quantity)
            {
                throw new Exception($"原材料 {rawMaterial.ProductName} 库存不足，当前剩余: {rawMaterial.RemainingQuantity}");
            }
            
            var item = new RawMaterialOutboundItem
            {
                RawMaterialId = rawMaterial.Id,
                Specification = itemDto.Specification,
                Quantity = itemDto.Quantity,
                Remark = itemDto.Remark,
                CreatedAt = DateTime.Now
            };
            outbound.Items.Add(item);
            
            // 暂时不更新库存，只在完成操作时更新
        }
        
        var createdOutbound = await _outboundRepository.AddAsync(outbound);
        var resultDto = _mapper.Map<RawMaterialOutboundDto>(createdOutbound);
        resultDto.TotalQuantity = createdOutbound.Items.Sum(i => i.Quantity);
        return resultDto;
    }

    public async Task<RawMaterialOutboundDto> UpdateAsync(int id, UpdateRawMaterialOutboundDto dto)
    {
        var outbound = await _outboundRepository.GetByIdAsync(id);
        if (outbound == null)
            throw new Exception("出库单不存在");

        if (dto.Items == null || !dto.Items.Any())
            throw new Exception("出库明细不能为空");

        // 恢复原有库存
        foreach (var oldItem in outbound.Items)
        {
            var rawMaterial = await _rawMaterialRepository.GetByIdAsync(oldItem.RawMaterialId);
            if (rawMaterial != null)
            {
                rawMaterial.UsedQuantity -= oldItem.Quantity;
                rawMaterial.RemainingQuantity = rawMaterial.TotalQuantity - rawMaterial.UsedQuantity;
                await _rawMaterialRepository.UpdateAsync(rawMaterial);
            }
        }

        // 清除旧明细
        outbound.Items.Clear();

        // 更新出库单基本信息
        outbound.OutboundNumber = dto.OutboundNumber;
        outbound.OutboundDate = dto.OutboundDate;
        outbound.Recipient = dto.Recipient;
        outbound.Remark = dto.Remark;
        outbound.Operator = dto.Operator;

        // 添加新明细并更新库存
        foreach (var itemDto in dto.Items)
        {
            var rawMaterial = await GetRawMaterialAsync(itemDto.RawMaterialId, itemDto.ProductName);
            
            // 检查库存是否足够
            if (rawMaterial.RemainingQuantity < itemDto.Quantity)
            {
                throw new Exception($"原材料 {rawMaterial.ProductName} 库存不足，当前剩余: {rawMaterial.RemainingQuantity}");
            }
            
            var item = new RawMaterialOutboundItem
            {
                RawMaterialId = rawMaterial.Id,
                Specification = itemDto.Specification,
                Quantity = itemDto.Quantity,
                Remark = itemDto.Remark,
                CreatedAt = DateTime.Now
            };
            outbound.Items.Add(item);
            
            // 更新库存
            rawMaterial.UsedQuantity += itemDto.Quantity;
            rawMaterial.RemainingQuantity = rawMaterial.TotalQuantity - rawMaterial.UsedQuantity;
            await _rawMaterialRepository.UpdateAsync(rawMaterial);
        }
        
        await _outboundRepository.UpdateAsync(outbound);
        var resultDto = _mapper.Map<RawMaterialOutboundDto>(outbound);
        resultDto.TotalQuantity = outbound.Items.Sum(i => i.Quantity);
        return resultDto;
    }

    public async Task<IEnumerable<RawMaterialOutboundDto>> GetSummaryAsync()
    {
        var outbounds = await _outboundRepository.GetAllAsync();
        var dtos = _mapper.Map<IEnumerable<RawMaterialOutboundDto>>(outbounds);
        
        foreach (var dto in dtos)
        {
            var outbound = outbounds.First(i => i.Id == dto.Id);
            dto.TotalQuantity = outbound.Items.Sum(i => i.Quantity);
        }
        
        return dtos;
    }

    public async Task DeleteAsync(int id)
    {
        var outbound = await _outboundRepository.GetByIdAsync(id);
        if (outbound == null)
            throw new Exception("出库单不存在");

        // 恢复库存
        foreach (var item in outbound.Items)
        {
            var rawMaterial = await _rawMaterialRepository.GetByIdAsync(item.RawMaterialId);
            if (rawMaterial != null)
            {
                rawMaterial.UsedQuantity -= item.Quantity;
                rawMaterial.RemainingQuantity = rawMaterial.TotalQuantity - rawMaterial.UsedQuantity;
                await _rawMaterialRepository.UpdateAsync(rawMaterial);
            }
        }

        await _outboundRepository.DeleteAsync(id);
    }

    public async Task<bool> ExistsAsync(string number)
    {
        return await _outboundRepository.ExistsAsync(number);
    }

    /// <summary>
    /// 生成出库单号，格式：YCK + 年月日 + 4位序号
    /// 例如：YCK202503080001
    /// </summary>
    public async Task<string> GenerateOutboundNumberAsync()
    {
        var dateStr = DateTime.Now.ToString("yyyyMMdd");
        var prefix = $"YCK{dateStr}";
        
        // 获取当天最大的出库单号
        var allOutbounds = await _outboundRepository.GetAllAsync();
        var todayNumbers = allOutbounds
            .Where(o => o.OutboundNumber.StartsWith(prefix))
            .Select(o => o.OutboundNumber)
            .ToList();
        
        int sequence = 1;
        if (todayNumbers.Any())
        {
            var maxSequence = todayNumbers
                .Select(n => 
                {
                    var seqStr = n.Substring(prefix.Length);
                    return int.TryParse(seqStr, out var seq) ? seq : 0;
                })
                .DefaultIfEmpty(0)
                .Max();
            sequence = maxSequence + 1;
        }
        
        return $"{prefix}{sequence:D4}";
    }

    public async Task CompleteAsync(int id)
    {
        var outbound = await _outboundRepository.GetByIdAsync(id);
        if (outbound == null)
            throw new Exception("出库单不存在");

        // 检查出库单状态
        if (outbound.Status == "已完成")
            throw new Exception("出库单已经完成");

        // 设置状态为已完成
        outbound.Status = "已完成";
        outbound.UpdatedAt = DateTime.Now;

        // 处理库存更新
        foreach (var item in outbound.Items)
        {
            var rawMaterial = await _rawMaterialRepository.GetByIdAsync(item.RawMaterialId);
            if (rawMaterial != null)
            {
                // 检查库存是否足够
                if (rawMaterial.RemainingQuantity < item.Quantity)
                {
                    throw new Exception($"原材料 {rawMaterial.ProductName} 库存不足，当前剩余: {rawMaterial.RemainingQuantity}");
                }

                // 更新库存
                rawMaterial.UsedQuantity += item.Quantity;
                rawMaterial.RemainingQuantity = rawMaterial.TotalQuantity - rawMaterial.UsedQuantity;
                await _rawMaterialRepository.UpdateAsync(rawMaterial);
            }
        }

        await _outboundRepository.UpdateAsync(outbound);
    }

    public Task<IEnumerable<AvailableItemDto>> GetAvailableItemsAsync(string? keyword = null)
    {
        throw new NotImplementedException();
    }

    public Task<AvailableItemsResponseDto> GetAvailableItemsPagedAsync(AvailableItemsRequestDto request)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ProjectOutboundDto>> SearchOutboundsAsync(string keyword)
    {
        throw new NotImplementedException();
    }
}
