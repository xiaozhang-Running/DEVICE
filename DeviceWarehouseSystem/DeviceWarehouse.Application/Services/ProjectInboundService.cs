using AutoMapper;
using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;

using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DeviceWarehouse.Application.Services;

public class ProjectInboundService : CachedServiceBase, IProjectInboundService
{
    private readonly IProjectInboundRepository _inboundRepository;
    private readonly IProjectOutboundRepository _outboundRepository;
    private readonly ISpecialEquipmentRepository _specialEquipmentRepository;
    private readonly IGeneralEquipmentRepository _generalEquipmentRepository;
    private readonly IConsumableRepository _consumableRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IMapper _mapper;

    public ProjectInboundService(
        IProjectInboundRepository inboundRepository,
        IProjectOutboundRepository outboundRepository,
        ISpecialEquipmentRepository specialEquipmentRepository,
        IGeneralEquipmentRepository generalEquipmentRepository,
        IConsumableRepository consumableRepository,
        IInventoryRepository inventoryRepository,
        IMapper mapper,
        ICacheService cache,
        ILogger<ProjectInboundService> logger)
        : base(cache, logger)
    {
        _inboundRepository = inboundRepository;
        _outboundRepository = outboundRepository;
        _specialEquipmentRepository = specialEquipmentRepository;
        _generalEquipmentRepository = generalEquipmentRepository;
        _consumableRepository = consumableRepository;
        _inventoryRepository = inventoryRepository;
        _mapper = mapper;
    }

    public async Task<ProjectInboundDto> GetByIdAsync(int id)
    {
        var cacheKey = BuildCacheKey("projectinbound", id);
        var cached = await GetFromCacheAsync<ProjectInboundDto>(cacheKey);
        if (cached != null) return cached;

        var inbound = await _inboundRepository.GetByIdAsync(id);
        if (inbound == null)
            throw new Exception("项目入库单不存在");
        
        var dto = _mapper.Map<ProjectInboundDto>(inbound);
        dto.TotalQuantity = inbound.Items.Sum(i => i.Quantity);
        
        // 从多对多关系中获取出库单编号和ID
        dto.ProjectOutboundNumbers = inbound.ProjectInboundOutbounds
            .Where(pi => pi.ProjectOutbound != null)
            .Select(pi => pi.ProjectOutbound!.OutboundNumber)
            .ToList();
        
        // 获取出库单ID列表
        dto.ProjectOutboundIds = inbound.ProjectInboundOutbounds
            .Select(pi => pi.ProjectOutboundId)
            .ToList();
        
        await SetCacheAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
        return dto;
    }

    public async Task<IEnumerable<ProjectInboundDto>> GetAllAsync()
    {
        var cacheKey = BuildCacheKey("projectinbounds", "all");
        var cached = await GetFromCacheAsync<List<ProjectInboundDto>>(cacheKey);
        if (cached != null) return cached;

        try
        {
            var inbounds = await _inboundRepository.GetAllAsync();
            var dtos = _mapper.Map<List<ProjectInboundDto>>(inbounds);
            
            foreach (var dto in dtos)
            {
                var inbound = inbounds.FirstOrDefault(i => i.Id == dto.Id);
                if (inbound != null && inbound.Items != null)
                {
                    dto.TotalQuantity = inbound.Items.Sum(i => i.Quantity);
                    // 从多对多关系中获取出库单编号
                    dto.ProjectOutboundNumbers = inbound.ProjectInboundOutbounds
                        .Where(pi => pi.ProjectOutbound != null)
                        .Select(pi => pi.ProjectOutbound!.OutboundNumber)
                        .ToList();
                    // 获取出库单ID列表
                    dto.ProjectOutboundIds = inbound.ProjectInboundOutbounds
                        .Select(pi => pi.ProjectOutboundId)
                        .ToList();
                }
                else
                {
                    dto.TotalQuantity = 0;
                }
            }
            
            await SetCacheAsync(cacheKey, dtos, TimeSpan.FromMinutes(2));
            return dtos;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] GetAllAsync Service failed: {ex.Message}");
            throw new Exception("获取项目入库单列表失败: " + ex.Message);
        }
    }

    public async Task<IEnumerable<ProjectOutboundDto>> GetAvailableProjectOutbounds()
    {
        // 获取所有出库单
        var outbounds = await _outboundRepository.GetAllAsync();
        
        var dtos = _mapper.Map<List<ProjectOutboundDto>>(outbounds);
        
        // 获取所有已完成的入库单
        var completedInbounds = (await _inboundRepository.GetAllAsync())
            .Where(i => i.IsCompleted)
            .ToList();
        
        // 提取已完成入库的出库单ID
        var completedOutboundIds = completedInbounds
            .SelectMany(i => i.ProjectInboundOutbounds)
            .Select(pi => pi.ProjectOutboundId)
            .ToHashSet();
        
        // 获取所有部分入库的入库单
        var partialInbounds = (await _inboundRepository.GetAllAsync())
            .Where(i => !i.IsCompleted && i.Status == "部分入库")
            .ToList();
        
        // 提取部分入库的出库单ID
        var partialOutboundIds = partialInbounds
            .SelectMany(i => i.ProjectInboundOutbounds)
            .Select(pi => pi.ProjectOutboundId)
            .ToHashSet();
        
        foreach (var dto in dtos)
        {
            var outbound = outbounds.FirstOrDefault(o => o.Id == dto.Id);
            if (outbound != null && outbound.Items != null)
            {
                dto.TotalQuantity = outbound.Items.Sum(i => i.Quantity);
            }
            else
            {
                dto.TotalQuantity = 0;
            }
            
            // 设置入库状态
            if (completedOutboundIds.Contains(dto.Id))
            {
                dto.InboundStatus = "入库完成";
            }
            else if (partialOutboundIds.Contains(dto.Id))
            {
                dto.InboundStatus = "部分入库";
            }
            else
            {
                dto.InboundStatus = "未入库";
            }
        }
        
        // 过滤掉已入库的出库记录，只返回未入库和部分入库的记录
        return dtos.Where(dto => dto.InboundStatus != "入库完成");
    }

    public async Task<IEnumerable<ProjectOutboundItemDto>> GetUninboundItemsByOutboundIdAsync(int outboundId)
    {
        // 获取出库单
        var outbound = await _outboundRepository.GetByIdAsync(outboundId);
        if (outbound == null)
            throw new Exception("项目出库单不存在");

        // 获取该出库单对应的所有已完成入库单
        var completedInbounds = (await _inboundRepository.GetAllAsync())
            .Where(i => i.IsCompleted && i.ProjectInboundOutbounds.Any(pi => pi.ProjectOutboundId == outboundId))
            .ToList();

        // 提取已入库的物品 ID
        var inboundItemIds = new HashSet<string>();
        foreach (var inbound in completedInbounds)
        {
            foreach (var item in inbound.Items)
            {
                // 使用物品类型和物品 ID 作为唯一标识，处理DeviceCode为空的情况
                var key = $"{item.ItemType}_{item.ItemId}_{(item.DeviceCode ?? "")}";
                inboundItemIds.Add(key);
            }
        }

        // 筛选未入库的物品
        var uninboundItems = new List<ProjectOutboundItemDto>();
        foreach (var item in outbound.Items)
        {
            var key = $"{item.ItemType}_{item.ItemId}_{(item.DeviceCode ?? "")}";
            if (!inboundItemIds.Contains(key))
            {
                var dto = _mapper.Map<ProjectOutboundItemDto>(item);
                uninboundItems.Add(dto);
            }
        }

        return uninboundItems;
    }

    public async Task<ProjectOutboundDto> GetProjectOutboundByIdAsync(int id)
    {
        var outbound = await _outboundRepository.GetByIdAsync(id);
        if (outbound == null)
            throw new Exception("项目出库单不存在");
        
        var dto = _mapper.Map<ProjectOutboundDto>(outbound);
        dto.TotalQuantity = outbound.Items?.Sum(i => i.Quantity) ?? 0;
        return dto;
    }

    public async Task<ProjectInboundDto> CreateAsync(CreateProjectInboundDto dto)
    {
        Console.WriteLine($"[CreateAsync] 开始创建入库单");
        
        if (dto.Items == null || !dto.Items.Any())
            throw new Exception("入库明细不能为空");

        var inbound = _mapper.Map<ProjectInbound>(dto);
        Console.WriteLine($"[CreateAsync] AutoMapper映射后 Status: {inbound.Status}");
        
        inbound.CreatedAt = DateTime.Now;
        inbound.IsCompleted = false;
        inbound.Status = "待入库"; // 显式设置初始状态为待入库
        Console.WriteLine($"[CreateAsync] 显式设置后 Status: {inbound.Status}");
        
        if (dto.InboundImages != null && dto.InboundImages.Any())
        {
            inbound.InboundImages = System.Text.Json.JsonSerializer.Serialize(dto.InboundImages);
        }

        if (string.IsNullOrWhiteSpace(inbound.InboundNumber))
        {
            inbound.InboundNumber = await GenerateInboundNumberAsync();
        }

        if (dto.ProjectOutboundIds != null && dto.ProjectOutboundIds.Any())
        {
            foreach (var outboundId in dto.ProjectOutboundIds)
            {
                var outbound = await _outboundRepository.GetByIdAsync(outboundId);
                if (outbound == null)
                    throw new Exception($"项目出库单 {outboundId} 不存在");
                inbound.ProjectInboundOutbounds.Add(new ProjectInboundOutbound
                {
                    ProjectOutboundId = outboundId,
                    CreatedAt = DateTime.Now
                });
            }
        }

        foreach (var item in inbound.Items)
        {
            item.CreatedAt = DateTime.Now;
        }

        var createdInbound = await _inboundRepository.AddAsync(inbound);
        Console.WriteLine($"[CreateAsync] 保存到数据库后 Status: {createdInbound.Status}");
        
        await InvalidateCacheAsync(BuildCacheKey("projectinbounds", "all"));
        
        var resultDto = _mapper.Map<ProjectInboundDto>(createdInbound);
        Console.WriteLine($"[CreateAsync] 返回前 resultDto Status: {resultDto.Status}");
        resultDto.TotalQuantity = createdInbound.Items.Sum(i => i.Quantity);
        // 从多对多关系中获取出库单编号
        resultDto.ProjectOutboundNumbers = createdInbound.ProjectInboundOutbounds
            .Where(pi => pi.ProjectOutbound != null)
            .Select(pi => pi.ProjectOutbound!.OutboundNumber)
            .ToList();
        return resultDto;
    }

    public async Task<ProjectInboundDto> UpdateAsync(int id, UpdateProjectInboundDto dto)
    {
        var inbound = await _inboundRepository.GetByIdAsync(id);
        if (inbound == null)
            throw new Exception("项目入库单不存在");

        if (inbound.IsCompleted)
            throw new Exception("已完成的入库单不能编辑");

        if (dto.Items == null || !dto.Items.Any())
            throw new Exception("入库明细不能为空");

        inbound.Items.Clear();

        inbound.InboundNumber = dto.InboundNumber;
        inbound.InboundDate = dto.InboundDate;
        inbound.ProjectName = dto.ProjectName;
        inbound.ProjectCode = dto.ProjectCode;
        inbound.ProjectManager = dto.ProjectManager;
        inbound.Supplier = dto.Supplier;
        inbound.InboundType = dto.InboundType;
        inbound.ProjectTime = dto.ProjectTime;
        inbound.ContactPhone = dto.ContactPhone;
        inbound.StorageLocation = dto.StorageLocation;
        inbound.Handler = dto.Handler;
        inbound.WarehouseKeeper = dto.WarehouseKeeper;
        
        if (dto.InboundImages != null && dto.InboundImages.Any())
        {
            inbound.InboundImages = System.Text.Json.JsonSerializer.Serialize(dto.InboundImages);
        }
        else
        {
            inbound.InboundImages = null;
        }
        inbound.Remark = dto.Remark;
        inbound.UpdatedAt = DateTime.Now;

        // 更新出库单关联
        inbound.ProjectInboundOutbounds.Clear();
        if (dto.ProjectOutboundIds != null && dto.ProjectOutboundIds.Any())
        {
            foreach (var outboundId in dto.ProjectOutboundIds)
            {
                var outbound = await _outboundRepository.GetByIdAsync(outboundId);
                if (outbound == null)
                    throw new Exception($"项目出库单 {outboundId} 不存在");
                inbound.ProjectInboundOutbounds.Add(new ProjectInboundOutbound
                {
                    ProjectOutboundId = outboundId,
                    CreatedAt = DateTime.Now
                });
            }
        }

        foreach (var itemDto in dto.Items)
        {
            var item = new ProjectInboundItem
            {
                ItemType = itemDto.ItemType,
                ItemId = itemDto.ItemId,
                ItemName = itemDto.ItemName,
                DeviceCode = itemDto.DeviceCode,
                Brand = itemDto.Brand,
                Model = itemDto.Model,
                Quantity = itemDto.Quantity,
                Unit = itemDto.Unit,
                Accessories = itemDto.Accessories,
                Remark = itemDto.Remark,
                DeviceStatus = itemDto.DeviceStatus,
                IsInbound = itemDto.IsInbound,
                CreatedAt = DateTime.Now
            };
            inbound.Items.Add(item);
        }

        await _inboundRepository.UpdateAsync(inbound);
        
        await InvalidateCacheAsync(BuildCacheKey("projectinbound", id));
        await InvalidateCacheAsync(BuildCacheKey("projectinbounds", "all"));

        var resultDto = _mapper.Map<ProjectInboundDto>(inbound);
        resultDto.TotalQuantity = inbound.Items.Sum(i => i.Quantity);
        resultDto.ProjectOutboundNumbers = inbound.ProjectInboundOutbounds
            .Where(pi => pi.ProjectOutbound != null)
            .Select(pi => pi.ProjectOutbound!.OutboundNumber)
            .ToList();
        return resultDto;
    }

    public async Task<IEnumerable<ProjectInboundDto>> GetSummaryAsync()
    {
        return await GetAllAsync();
    }

    public async Task CompleteAsync(int id)
    {
        Console.WriteLine($"[CompleteAsync] 开始完成入库单: {id}");
        
        var inbound = await _inboundRepository.GetByIdAsync(id);
        if (inbound == null)
            throw new Exception("项目入库单不存在");

        if (inbound.IsCompleted)
            throw new Exception("入库单已完成，不能重复完成");

        if (inbound.Items == null || !inbound.Items.Any())
            throw new Exception("入库明细为空，无法完成");

        foreach (var item in inbound.Items)
        {
            await UpdateStockAsync(inbound.Id, new CreateProjectInboundItemDto
            {
                ItemType = item.ItemType,
                ItemId = item.ItemId,
                ItemName = item.ItemName,
                DeviceCode = item.DeviceCode,
                Brand = item.Brand,
                Model = item.Model,
                Quantity = item.Quantity,
                Unit = item.Unit,
                Accessories = item.Accessories,
                Remark = item.Remark,
                DeviceStatus = item.DeviceStatus
            });
        }

        // 标记入库单为已完成
        inbound.IsCompleted = true;
        inbound.Status = "已完成";
        inbound.UpdatedAt = DateTime.Now;
        inbound.CompletedAt = DateTime.Now;

        await _inboundRepository.UpdateAsync(inbound);
        
        await InvalidateCacheAsync(BuildCacheKey("projectinbound", id));
        await InvalidateCacheAsync(BuildCacheKey("projectinbounds", "all"));
        
        // 清除相关出库单的缓存，以更新入库状态
        foreach (var inboundOutbound in inbound.ProjectInboundOutbounds)
        {
            await InvalidateCacheAsync(BuildCacheKey("projectoutbound", inboundOutbound.ProjectOutboundId));
        }
        await InvalidateCacheAsync(BuildCacheKey("projectoutbounds", "all"));
    }

    public async Task PartialInboundAsync(int id)
    {
        Console.WriteLine($"[PartialInboundAsync] 开始部分入库: {id}");
        
        var inbound = await _inboundRepository.GetByIdAsync(id);
        if (inbound == null)
            throw new Exception("项目入库单不存在");

        if (inbound.Items == null || !inbound.Items.Any())
            throw new Exception("入库明细为空，无法进行部分入库");

        // 更新库存信息
        foreach (var item in inbound.Items)
        {
            await UpdateStockAsync(inbound.Id, new CreateProjectInboundItemDto
            {
                ItemType = item.ItemType,
                ItemId = item.ItemId,
                ItemName = item.ItemName,
                DeviceCode = item.DeviceCode,
                Brand = item.Brand,
                Model = item.Model,
                Quantity = item.Quantity,
                Unit = item.Unit,
                Accessories = item.Accessories,
                Remark = item.Remark,
                DeviceStatus = item.DeviceStatus
            });
        }

        // 设置状态为部分入库
        inbound.Status = "部分入库";
        inbound.UpdatedAt = DateTime.Now;

        await _inboundRepository.UpdateAsync(inbound);
        
        await InvalidateCacheAsync(BuildCacheKey("projectinbound", id));
        await InvalidateCacheAsync(BuildCacheKey("projectinbounds", "all"));
        
        // 清除相关出库单的缓存，以更新入库状态
        foreach (var inboundOutbound in inbound.ProjectInboundOutbounds)
        {
            await InvalidateCacheAsync(BuildCacheKey("projectoutbound", inboundOutbound.ProjectOutboundId));
        }
        await InvalidateCacheAsync(BuildCacheKey("projectoutbounds", "all"));
        
        Console.WriteLine($"[PartialInboundAsync] 部分入库完成: {id}");
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            var inbound = await _inboundRepository.GetByIdAsync(id);
            if (inbound == null)
                throw new Exception("项目入库单不存在");

            if (inbound.IsCompleted && inbound.Items != null && inbound.Items.Any())
            {
                var uniqueItems = inbound.Items
                    .GroupBy(i => new { i.ItemType, i.ItemId })
                    .Select(g => new ProjectInboundItem
                    {
                        ItemType = g.Key.ItemType,
                        ItemId = g.Key.ItemId,
                        Quantity = g.Sum(i => i.Quantity)
                    })
                    .ToList();

                foreach (var item in uniqueItems)
                {
                    try
                    {
                        await ReverseStockAsync(item);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DeleteAsync] 恢复库存失败: ItemType={item.ItemType}, ItemId={item.ItemId}, 错误: {ex.Message}");
                    }
                }
            }

            await _inboundRepository.DeleteAsync(id);
            
            await InvalidateCacheAsync(BuildCacheKey("projectinbound", id));
            await InvalidateCacheAsync(BuildCacheKey("projectinbounds", "all"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DeleteAsync] 删除入库单 {id} 失败: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string number)
    {
        return await _inboundRepository.ExistsAsync(number);
    }

    private async Task<string> GenerateInboundNumberAsync()
    {
        var dateStr = DateTime.Now.ToString("yyyyMMdd");
        var prefix = $"XMRK{dateStr}";
        
        var allInbounds = await _inboundRepository.GetAllAsync();
        var todayNumbers = allInbounds
            .Where(o => o.InboundNumber.StartsWith(prefix))
            .Select(o => o.InboundNumber)
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

    private async Task UpdateStockAsync(int inboundId, CreateProjectInboundItemDto itemDto)
    {
        var itemTypeInt = (int)itemDto.ItemType;
        
        switch (itemTypeInt)
        {
            case 1: // SpecialEquipment
                var specialEquipment = await _specialEquipmentRepository.GetByIdAsync(itemDto.ItemId);
                if (specialEquipment == null)
                    throw new Exception($"专用设备不存在: {itemDto.ItemName}");
                
                // 设备入库只更新使用状态，不增加数量（与出库逻辑保持一致）
                specialEquipment.UseStatus = DeviceWarehouse.Domain.Enums.UsageStatus.Unused; // 释放使用状态
                if (!string.IsNullOrEmpty(itemDto.DeviceStatus))
                {
                    try
                    {
                        // 处理前端传入的设备状态值
                        if (itemDto.DeviceStatus == "0")
                        {
                            specialEquipment.DeviceStatus = DeviceWarehouse.Domain.Enums.DeviceStatus.Normal;
                        }
                        else
                        {
                            specialEquipment.DeviceStatus = (DeviceWarehouse.Domain.Enums.DeviceStatus)Enum.Parse(typeof(DeviceWarehouse.Domain.Enums.DeviceStatus), itemDto.DeviceStatus);
                        }
                    }
                    catch (Exception)
                    {
                        // 如果解析失败，使用默认值
                        specialEquipment.DeviceStatus = DeviceWarehouse.Domain.Enums.DeviceStatus.Normal;
                    }
                }
                if (!string.IsNullOrEmpty(itemDto.Remark))
                {
                    specialEquipment.Remark = itemDto.Remark;
                }
                // 同步项目信息到设备记录
                var projectInbound = await _inboundRepository.GetByIdAsync(inboundId);
                if (projectInbound != null)
                {
                    specialEquipment.ProjectName = projectInbound.ProjectName;
                    specialEquipment.ProjectTime = projectInbound.ProjectTime;
                }
                // 如果设备状态为损坏，更新维修相关字段
                if (specialEquipment.DeviceStatus == DeviceWarehouse.Domain.Enums.DeviceStatus.Broken)
                {
                    specialEquipment.RepairStatus = 0; // 0表示待维修
                    specialEquipment.FaultReason = itemDto.Remark;
                }
                await _specialEquipmentRepository.UpdateAsync(specialEquipment);
                
                var specialInventory = await _inventoryRepository.GetBySpecialEquipmentIdAsync(itemDto.ItemId);
                if (specialInventory != null)
                {
                    specialInventory.CurrentQuantity = specialEquipment.Quantity;
                    specialInventory.LastUpdated = DateTime.Now;
                    await _inventoryRepository.UpdateAsync(specialInventory);
                }
                break;

            case 2: // GeneralEquipment
                var generalEquipment = await _generalEquipmentRepository.GetByIdAsync(itemDto.ItemId);
                if (generalEquipment == null)
                    throw new Exception($"通用设备不存在: {itemDto.ItemName}");
                
                // 设备入库只更新使用状态，不增加数量（与出库逻辑保持一致）
                generalEquipment.UseStatus = DeviceWarehouse.Domain.Enums.UsageStatus.Unused; // 释放使用状态
                if (!string.IsNullOrEmpty(itemDto.DeviceStatus))
                {
                    try
                    {
                        // 处理前端传入的设备状态值
                        if (itemDto.DeviceStatus == "0")
                        {
                            generalEquipment.DeviceStatus = DeviceWarehouse.Domain.Enums.DeviceStatus.Normal;
                        }
                        else
                        {
                            generalEquipment.DeviceStatus = (DeviceWarehouse.Domain.Enums.DeviceStatus)Enum.Parse(typeof(DeviceWarehouse.Domain.Enums.DeviceStatus), itemDto.DeviceStatus);
                        }
                    }
                    catch (Exception)
                    {
                        // 如果解析失败，使用默认值
                        generalEquipment.DeviceStatus = DeviceWarehouse.Domain.Enums.DeviceStatus.Normal;
                    }
                }
                if (!string.IsNullOrEmpty(itemDto.Remark))
                {
                    generalEquipment.Remark = itemDto.Remark;
                }
                // 同步项目信息到设备记录
                var projectInboundForGeneral = await _inboundRepository.GetByIdAsync(inboundId);
                if (projectInboundForGeneral != null)
                {
                    generalEquipment.ProjectName = projectInboundForGeneral.ProjectName;
                    generalEquipment.ProjectTime = projectInboundForGeneral.ProjectTime;
                }
                // 如果设备状态为损坏，更新维修相关字段
                if (generalEquipment.DeviceStatus == DeviceWarehouse.Domain.Enums.DeviceStatus.Broken)
                {
                    generalEquipment.RepairStatus = 0; // 0表示待维修
                    generalEquipment.FaultReason = itemDto.Remark;
                }
                await _generalEquipmentRepository.UpdateAsync(generalEquipment);
                
                var generalInventory = await _inventoryRepository.GetByGeneralEquipmentIdAsync(itemDto.ItemId);
                if (generalInventory != null)
                {
                    generalInventory.CurrentQuantity = generalEquipment.Quantity;
                    generalInventory.LastUpdated = DateTime.Now;
                    await _inventoryRepository.UpdateAsync(generalInventory);
                }
                break;

            case 3: // Consumable
                var consumable = await _consumableRepository.GetByIdAsync(itemDto.ItemId);
                if (consumable == null)
                    throw new Exception($"耗材不存在: {itemDto.ItemName}");
                
                // 耗材入库增加剩余数量
                consumable.RemainingQuantity += itemDto.Quantity;
                // 原始数量 = 剩余数量 + 使用数量
                consumable.TotalQuantity = consumable.RemainingQuantity + consumable.UsedQuantity;
                if (!string.IsNullOrEmpty(itemDto.Remark))
                {
                    consumable.Remark = itemDto.Remark;
                }
                await _consumableRepository.UpdateAsync(consumable);
                break;

            default:
                throw new Exception($"未知的物品类型: {itemDto.ItemType}");
        }
    }

    private async Task ReverseStockAsync(ProjectInboundItem item)
    {
        var itemTypeInt = (int)item.ItemType;
        
        switch (itemTypeInt)
        {
            case 1: // SpecialEquipment
                var specialEquipment = await _specialEquipmentRepository.GetByIdAsync(item.ItemId);
                if (specialEquipment != null)
                {
                    // 设备入库撤销只更新使用状态，不减少数量（与入库逻辑保持一致）
                    specialEquipment.UseStatus = DeviceWarehouse.Domain.Enums.UsageStatus.InUse; // 恢复使用状态
                    await _specialEquipmentRepository.UpdateAsync(specialEquipment);
                    
                    var specialInventory = await _inventoryRepository.GetBySpecialEquipmentIdAsync(item.ItemId);
                    if (specialInventory != null)
                    {
                        specialInventory.CurrentQuantity = specialEquipment.Quantity;
                        specialInventory.LastUpdated = DateTime.Now;
                        await _inventoryRepository.UpdateAsync(specialInventory);
                    }
                }
                break;

            case 2: // GeneralEquipment
                var generalEquipment = await _generalEquipmentRepository.GetByIdAsync(item.ItemId);
                if (generalEquipment != null)
                {
                    // 设备入库撤销只更新使用状态，不减少数量（与入库逻辑保持一致）
                    generalEquipment.UseStatus = DeviceWarehouse.Domain.Enums.UsageStatus.InUse; // 恢复使用状态
                    await _generalEquipmentRepository.UpdateAsync(generalEquipment);
                    
                    var generalInventory = await _inventoryRepository.GetByGeneralEquipmentIdAsync(item.ItemId);
                    if (generalInventory != null)
                    {
                        generalInventory.CurrentQuantity = generalEquipment.Quantity;
                        generalInventory.LastUpdated = DateTime.Now;
                        await _inventoryRepository.UpdateAsync(generalInventory);
                    }
                }
                break;

            case 3: // Consumable
                var consumable = await _consumableRepository.GetByIdAsync(item.ItemId);
                if (consumable != null)
                {
                    // 减少剩余数量
                    consumable.RemainingQuantity -= item.Quantity;
                    if (consumable.RemainingQuantity < 0)
                        consumable.RemainingQuantity = 0;
                    // 原始数量 = 剩余数量 + 使用数量
                    consumable.TotalQuantity = consumable.RemainingQuantity + consumable.UsedQuantity;
                    await _consumableRepository.UpdateAsync(consumable);
                }
                break;
        }
    }
}
