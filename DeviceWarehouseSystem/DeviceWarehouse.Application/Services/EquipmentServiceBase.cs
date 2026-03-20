using AutoMapper;
using DeviceWarehouse.Domain.Interfaces;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Enums;

namespace DeviceWarehouse.Application.Services;

public abstract class EquipmentServiceBase<TEntity, TDto, TCreateDto, TUpdateDto, TSummaryDto>
    where TEntity : class
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
    where TSummaryDto : class
{
    protected readonly IInventoryRepository _inventoryRepository;
    protected readonly IMapper _mapper;
    protected readonly ICacheService _cacheService;

    protected EquipmentServiceBase(
        IInventoryRepository inventoryRepository,
        IMapper mapper,
        ICacheService cacheService)
    {
        _inventoryRepository = inventoryRepository;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public abstract Task<TDto> GetByIdAsync(int id);
    public abstract Task<IEnumerable<TDto>> GetAllAsync();
    public abstract Task<IEnumerable<TDto>> GetByTypeAsync(DeviceType type);
    public abstract Task<IEnumerable<TSummaryDto>> GetEquipmentSummaryAsync(DeviceType? type);
    public abstract Task<TDto> CreateAsync(TCreateDto dto);
    public abstract Task UpdateAsync(int id, TUpdateDto dto);
    public abstract Task DeleteAsync(int id);
    public abstract Task<IEnumerable<TDto>> SearchAsync(string keyword);
    public abstract Task<object> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false, DeviceStatus? deviceStatus = null, UsageStatus? useStatus = null, string? brand = null);
    public abstract Task<object> GetGroupedPagedAsync(int pageNumber, int pageSize, string? keyword = null);

    protected async Task<Inventory> CreateInventoryAsync(int equipmentId, int quantity, bool isSpecialEquipment)
    {
        var inventory = new Inventory
        {
            CurrentQuantity = quantity,
            AlertMinQuantity = 10,
            AlertMaxQuantity = 100,
            LastUpdated = DateTime.Now
        };

        if (isSpecialEquipment)
        {
            inventory.SpecialEquipmentId = equipmentId;
        }
        else
        {
            inventory.GeneralEquipmentId = equipmentId;
        }

        return await _inventoryRepository.AddAsync(inventory);
    }

    protected async Task UpdateInventoryAsync(int equipmentId, int quantity, bool isSpecialEquipment)
    {
        Inventory? inventory;
        if (isSpecialEquipment)
        {
            inventory = await _inventoryRepository.GetBySpecialEquipmentIdAsync(equipmentId);
        }
        else
        {
            inventory = await _inventoryRepository.GetByGeneralEquipmentIdAsync(equipmentId);
        }

        if (inventory != null)
        {
            inventory.CurrentQuantity = quantity;
            inventory.LastUpdated = DateTime.Now;
            await _inventoryRepository.UpdateAsync(inventory);
        }
    }

    protected string BuildCacheKey(string prefix, params object[] parameters)
    {
        var key = prefix;
        foreach (var param in parameters)
        {
            key += $":{param}";
        }
        return key;
    }
}