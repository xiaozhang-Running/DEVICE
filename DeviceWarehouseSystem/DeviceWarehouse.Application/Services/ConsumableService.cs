using AutoMapper;
using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;

using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DeviceWarehouse.Application.Services;

public class ConsumableService : CachedServiceBase, IConsumableService
{
    private readonly IConsumableRepository _consumableRepository;
    private readonly IMapper _mapper;

    public ConsumableService(
        IConsumableRepository consumableRepository, 
        IMapper mapper,
        ICacheService cacheService,
        ILogger<ConsumableService> logger)
        : base(cacheService, logger)
    {
        _consumableRepository = consumableRepository;
        _mapper = mapper;
    }

    public async Task<ConsumableDto> GetByIdAsync(int id)
    {
        var cacheKey = BuildCacheKey("consumable", id);
        var cachedData = await GetFromCacheAsync<ConsumableDto>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        var consumable = await _consumableRepository.GetByIdAsync(id);
        if (consumable == null)
            throw new Exception("耗材不存在");
        var result = _mapper.Map<ConsumableDto>(consumable);
        await SetCacheAsync(cacheKey, result, TimeSpan.FromMinutes(10));
        return result;
    }

    public async Task<IEnumerable<ConsumableDto>> GetAllAsync()
    {
        var cacheKey = "consumable_all";
        var cachedData = await GetFromCacheAsync<IEnumerable<ConsumableDto>>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        var consumables = await _consumableRepository.GetAllAsync();
        var result = _mapper.Map<IEnumerable<ConsumableDto>>(consumables);
        await SetCacheAsync(cacheKey, result, TimeSpan.FromMinutes(10));
        return result;
    }

    public async Task<ConsumableDto> CreateAsync(CreateConsumableDto dto)
    {
        var consumable = _mapper.Map<Consumable>(dto);
        
        // 如果没有提供总数或已使用数，根据剩余数量计算
        if (consumable.TotalQuantity == 0 && consumable.UsedQuantity == 0 && consumable.RemainingQuantity > 0)
        {
            consumable.TotalQuantity = consumable.RemainingQuantity;
            consumable.UsedQuantity = 0;
        }
        // 否则确保剩余数量等于总数减去已使用数
        else
        {
            consumable.RemainingQuantity = consumable.TotalQuantity - consumable.UsedQuantity;
        }
        
        var createdConsumable = await _consumableRepository.AddAsync(consumable);
        
        // 清除缓存
        await InvalidateCacheAsync("consumable_all");
        
        return _mapper.Map<ConsumableDto>(createdConsumable);
    }

    public async Task<ConsumableDto> UpdateAsync(int id, UpdateConsumableDto dto)
    {
        var consumable = await _consumableRepository.GetByIdAsync(id);
        if (consumable == null)
            throw new Exception("耗材不存在");

        _mapper.Map(dto, consumable);
        
        // 如果提供了剩余数量，根据剩余数量计算总数和已使用数
        if (dto.RemainingQuantity.HasValue)
        {
            consumable.TotalQuantity = dto.RemainingQuantity.Value;
            consumable.UsedQuantity = 0;
            consumable.RemainingQuantity = dto.RemainingQuantity.Value;
        }
        // 否则确保剩余数量等于总数减去已使用数
        else
        {
            consumable.RemainingQuantity = consumable.TotalQuantity - consumable.UsedQuantity;
        }
        
        await _consumableRepository.UpdateAsync(consumable);
        
        // 清除缓存
        await InvalidateCacheAsync(BuildCacheKey("consumable", id));
        await InvalidateCacheAsync("consumable_all");

        return _mapper.Map<ConsumableDto>(consumable);
    }

    public async Task<IEnumerable<ConsumableDto>> GetSummaryAsync()
    {
        var consumables = await _consumableRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ConsumableDto>>(consumables);
    }

    public async Task DeleteAsync(int id)
    {
        await _consumableRepository.DeleteAsync(id);
        
        // 清除缓存
        await InvalidateCacheAsync(BuildCacheKey("consumable", id));
        await InvalidateCacheAsync("consumable_all");
    }

    public async Task<IEnumerable<ConsumableDto>> SearchAsync(string keyword)
    {
        var consumables = await _consumableRepository.SearchAsync(keyword);
        return _mapper.Map<IEnumerable<ConsumableDto>>(consumables);
    }

    public async Task<object> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false)
    {
        var (items, totalCount, pageNumberResult, pageSizeResult, totalPages) = await _consumableRepository.GetPagedAsync(pageNumber, pageSize, keyword, sortBy, sortDescending);
        var mappedItems = _mapper.Map<IEnumerable<ConsumableDto>>(items);
        return new {
            items = mappedItems,
            totalCount,
            pageNumber = pageNumberResult,
            pageSize = pageSizeResult,
            totalPages
        };
    }

    public async Task<bool> ExistsAsync(string number)
    {
        return await _consumableRepository.ExistsAsync(number);
    }

    public async Task<object> DeleteAllAsync()
    {
        var allConsumables = await _consumableRepository.GetAllAsync();
        var ids = allConsumables.Select(c => c.Id).ToList();
        
        if (!ids.Any())
        {
            return new
            {
                successCount = 0,
                errorCount = 0,
                message = "没有可删除的耗材"
            };
        }

        var successCount = 0;
        var errorCount = 0;
        var errors = new List<string>();

        foreach (var id in ids)
        {
            try
            {
                await _consumableRepository.DeleteAsync(id);
                successCount++;
            }
            catch (Exception ex)
            {
                errorCount++;
                errors.Add($"耗材ID {id}: {ex.Message}");
            }
        }

        try
        {
            await InvalidateCacheAsync("consumable_all");
        }
        catch
        {
        }

        return new
        {
            successCount,
            errorCount,
            errors
        };
    }

    public async Task<object> CreateBatchAsync(IEnumerable<CreateConsumableDto> dtos)
    {
        if (dtos == null || !dtos.Any())
            throw new Exception("耗材列表不能为空");

        var successCount = 0;
        var errorCount = 0;
        var errors = new List<string>();
        var totalCount = dtos.Count();

        Console.WriteLine($"开始批量导入耗材，总记录数: {totalCount}");

        foreach (var dto in dtos)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    throw new Exception("耗材名称不能为空");
                }

                var consumable = _mapper.Map<Consumable>(dto);
                
                // 如果没有提供总数或已使用数，根据剩余数量计算
                if (consumable.TotalQuantity == 0 && consumable.UsedQuantity == 0 && consumable.RemainingQuantity > 0)
                {
                    consumable.TotalQuantity = consumable.RemainingQuantity;
                    consumable.UsedQuantity = 0;
                }
                // 否则确保剩余数量等于总数减去已使用数
                else
                {
                    consumable.RemainingQuantity = consumable.TotalQuantity - consumable.UsedQuantity;
                }
                
                await _consumableRepository.AddAsync(consumable);
                successCount++;
            }
            catch (Exception ex)
            {
                errorCount++;
                errors.Add($"耗材: {dto.Name} - {ex.Message}");
            }
        }

        Console.WriteLine($"批量导入完成，成功: {successCount}, 失败: {errorCount}");

        // 清除缓存
        try
        {
            await InvalidateCacheAsync("consumable_all");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"清除缓存失败: {ex.Message}");
        }

        return new
        {
            successCount,
            errorCount,
            errors
        };
    }
}
