using AutoMapper;
using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;

namespace DeviceWarehouse.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IInventoryTransactionRepository _transactionRepository;
    private readonly IConsumableRepository _consumableRepository;
    private readonly IRawMaterialRepository _rawMaterialRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    // 缓存键前缀
    private const string InventoryCachePrefix = "inventory:";
    // 默认预警阈值
    private const int DefaultLowStockThreshold = 10;
    // 默认最大库存
    private const int DefaultMaxStockThreshold = 100;

    public InventoryService(
        IInventoryRepository inventoryRepository,
        IInventoryTransactionRepository transactionRepository,
        IConsumableRepository consumableRepository,
        IRawMaterialRepository rawMaterialRepository,
        IMapper mapper,
        ICacheService cacheService)
    {
        _inventoryRepository = inventoryRepository;
        _transactionRepository = transactionRepository;
        _consumableRepository = consumableRepository;
        _rawMaterialRepository = rawMaterialRepository;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    /// <summary>
    /// 清除库存相关的缓存
    /// </summary>
    private async Task ClearInventoryCache(int? inventoryId = null)
    {
        // 清除指定库存的缓存
        if (inventoryId.HasValue)
        {
            await _cacheService.RemoveAsync($"{InventoryCachePrefix}{inventoryId}");
        }

        // 清除所有库存列表缓存
        await _cacheService.RemoveAsync($"{InventoryCachePrefix}all");
        await _cacheService.RemoveAsync($"{InventoryCachePrefix}all:1");
        await _cacheService.RemoveAsync($"{InventoryCachePrefix}all:2");
        await _cacheService.RemoveAsync($"{InventoryCachePrefix}all:3");
        await _cacheService.RemoveAsync($"{InventoryCachePrefix}all:4");

        // 清除预警相关缓存
        await _cacheService.RemoveAsync($"{InventoryCachePrefix}lowstock");
        await _cacheService.RemoveAsync($"{InventoryCachePrefix}lowstock:{DefaultLowStockThreshold}");
        await _cacheService.RemoveAsync($"{InventoryCachePrefix}zerostock");
    }

    public async Task<InventoryDto> GetByIdAsync(int id)
    {
        var cacheKey = $"{InventoryCachePrefix}{id}";
        var cachedData = await _cacheService.GetAsync<InventoryDto>(cacheKey);
        if (cachedData != null) return cachedData;

        var inventory = await _inventoryRepository.GetByIdAsync(id);
        if (inventory == null) throw new Exception("库存不存在");

        var result = _mapper.Map<InventoryDto>(inventory);
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
        return result;
    }

    public async Task<InventoryDto> GetBySpecialEquipmentIdAsync(int specialEquipmentId)
    {
        var cacheKey = $"{InventoryCachePrefix}special:{specialEquipmentId}";
        var cachedData = await _cacheService.GetAsync<InventoryDto>(cacheKey);
        if (cachedData != null) return cachedData;

        var inventory = await _inventoryRepository.GetBySpecialEquipmentIdAsync(specialEquipmentId);
        if (inventory == null) throw new Exception("库存不存在");

        var result = _mapper.Map<InventoryDto>(inventory);
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
        return result;
    }

    public async Task<InventoryDto> GetByGeneralEquipmentIdAsync(int generalEquipmentId)
    {
        var cacheKey = $"{InventoryCachePrefix}general:{generalEquipmentId}";
        var cachedData = await _cacheService.GetAsync<InventoryDto>(cacheKey);
        if (cachedData != null) return cachedData;

        var inventory = await _inventoryRepository.GetByGeneralEquipmentIdAsync(generalEquipmentId);
        if (inventory == null) throw new Exception("库存不存在");

        var result = _mapper.Map<InventoryDto>(inventory);
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
        return result;
    }

    public async Task<InventoryDto> GetByConsumableIdAsync(int consumableId)
    {
        var cacheKey = $"{InventoryCachePrefix}consumable:{consumableId}";
        var cachedData = await _cacheService.GetAsync<InventoryDto>(cacheKey);
        if (cachedData != null) return cachedData;

        // 由于数据库表中没有ConsumableId列，返回null
        throw new Exception("库存不存在");
    }

    public async Task<InventoryDto> GetByRawMaterialIdAsync(int rawMaterialId)
    {
        var cacheKey = $"{InventoryCachePrefix}raw:{rawMaterialId}";
        var cachedData = await _cacheService.GetAsync<InventoryDto>(cacheKey);
        if (cachedData != null) return cachedData;

        // 由于数据库表中没有RawMaterialId列，返回null
        throw new Exception("库存不存在");
    }

    public async Task<IEnumerable<InventoryDto>> GetAllAsync(int? category = null)
    {
        var cacheKey = category.HasValue ? $"{InventoryCachePrefix}all:{category}" : $"{InventoryCachePrefix}all";
        var cachedData = await _cacheService.GetAsync<IEnumerable<InventoryDto>>(cacheKey);
        if (cachedData != null) return cachedData;

        var result = new List<InventoryDto>();

        if (!category.HasValue || category.Value == 1 || category.Value == 2)
        {
            var inventories = await _inventoryRepository.GetAllAsync();
            
            if (category.HasValue)
            {
                switch (category.Value)
                {
                    case 1: // 专用设备
                        inventories = inventories.Where(i => i.SpecialEquipmentId != null);
                        break;
                    case 2: // 通用设备
                        inventories = inventories.Where(i => i.GeneralEquipmentId != null);
                        break;
                }
            }
            else
            {
                // 当category为null时，过滤掉没有关联设备的库存记录
                inventories = inventories.Where(i => i.SpecialEquipmentId != null || i.GeneralEquipmentId != null);
            }
            
            result.AddRange(_mapper.Map<IEnumerable<InventoryDto>>(inventories));
        }

        if (!category.HasValue || category.Value == 3)
        {
            // 获取耗材数据
            var consumables = await _consumableRepository.GetAllAsync();
            var consumableDtos = consumables.Select(c => new InventoryDto
            {
                Id = c.Id,
                SpecialEquipmentId = null,
                GeneralEquipmentId = null,
                CurrentQuantity = c.RemainingQuantity,
                AlertMinQuantity = DefaultLowStockThreshold, // 默认预警阈值
                AlertMaxQuantity = DefaultMaxStockThreshold, // 默认最大库存
                LastUpdated = c.UpdatedAt ?? c.CreatedAt,
                EquipmentName = c.Name,
                Brand = c.Brand,
                Model = c.ModelSpecification,
                Unit = c.Unit,
                Location = c.Location,
                Company = c.Company,
                Category = "耗材"
            });
            result.AddRange(consumableDtos);
        }

        if (!category.HasValue || category.Value == 4)
        {
            // 获取原材料数据
            var rawMaterials = await _rawMaterialRepository.GetAllAsync();
            var rawMaterialDtos = rawMaterials.Select(r => new InventoryDto
            {
                Id = r.Id,
                SpecialEquipmentId = null,
                GeneralEquipmentId = null,
                CurrentQuantity = r.RemainingQuantity,
                AlertMinQuantity = DefaultLowStockThreshold, // 默认预警阈值
                AlertMaxQuantity = DefaultMaxStockThreshold, // 默认最大库存
                LastUpdated = r.UpdatedAt ?? r.CreatedAt,
                EquipmentName = r.ProductName,
                Brand = r.Supplier,
                Model = r.Specification,
                Unit = r.Unit,
                Location = null,
                Company = r.Company,
                Category = "原材料"
            });
            result.AddRange(rawMaterialDtos);
        }
        
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        return result;
    }

    public async Task<InventoryDto> CreateAsync(CreateInventoryDto dto)
    {
        var inventory = _mapper.Map<Inventory>(dto);
        inventory.LastUpdated = DateTime.Now;
        var createdInventory = await _inventoryRepository.AddAsync(inventory);
        var result = _mapper.Map<InventoryDto>(createdInventory);

        // 清除缓存
        await ClearInventoryCache();
        return result;
    }

    public async Task UpdateAsync(int id, UpdateInventoryDto dto)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        if (inventory == null) throw new Exception("库存不存在");

        _mapper.Map(dto, inventory);
        inventory.LastUpdated = DateTime.Now;
        await _inventoryRepository.UpdateAsync(inventory);

        // 清除缓存
        await ClearInventoryCache(id);
    }

    public async Task DeleteAsync(int id)
    {
        await _inventoryRepository.DeleteAsync(id);

        // 清除缓存
        await ClearInventoryCache(id);
    }

    public async Task<IEnumerable<InventoryDto>> GetLowStockAsync(int threshold)
    {
        var cacheKey = $"{InventoryCachePrefix}lowstock:{threshold}";
        var cachedData = await _cacheService.GetAsync<IEnumerable<InventoryDto>>(cacheKey);
        if (cachedData != null) return cachedData;

        var inventories = await _inventoryRepository.GetLowStockAsync(threshold);
        var result = _mapper.Map<IEnumerable<InventoryDto>>(inventories);
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(3));
        return result;
    }

    public async Task<IEnumerable<InventoryDto>> GetLowStockAsync()
    {
        const string cacheKey = $"{InventoryCachePrefix}lowstock";
        var cachedData = await _cacheService.GetAsync<IEnumerable<InventoryDto>>(cacheKey);
        if (cachedData != null) return cachedData;

        var inventories = await _inventoryRepository.GetLowStockAsync();
        var result = _mapper.Map<IEnumerable<InventoryDto>>(inventories);
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(3));
        return result;
    }

    public async Task<IEnumerable<InventoryDto>> GetZeroStockAsync()
    {
        const string cacheKey = $"{InventoryCachePrefix}zerostock";
        var cachedData = await _cacheService.GetAsync<IEnumerable<InventoryDto>>(cacheKey);
        if (cachedData != null) return cachedData;

        var inventories = await _inventoryRepository.GetAllAsync()
            .ContinueWith(t => t.Result.Where(i => i.CurrentQuantity == 0));
        var result = _mapper.Map<IEnumerable<InventoryDto>>(inventories);
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(3));
        return result;
    }

    public async Task<InventoryDto> AddStockAsync(int inventoryId, int quantity, string reason, string reference)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(inventoryId);
        if (inventory == null) throw new Exception("库存不存在");

        inventory.CurrentQuantity += quantity;
        inventory.LastUpdated = DateTime.Now;
        await _inventoryRepository.UpdateAsync(inventory);

        // 记录交易
        var transaction = new InventoryTransaction
        {
            InventoryId = inventoryId,
            Inventory = inventory,
            Quantity = quantity,
            TransactionType = "Inbound",
            Reason = reason,
            Reference = reference,
            Operator = "System",
            TransactionDate = DateTime.Now
        };
        await _transactionRepository.AddAsync(transaction);

        // 清除缓存
        await ClearInventoryCache(inventoryId);

        return _mapper.Map<InventoryDto>(inventory);
    }

    public async Task<InventoryDto> RemoveStockAsync(int inventoryId, int quantity, string reason, string reference)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(inventoryId);
        if (inventory == null) throw new Exception("库存不存在");
        if (inventory.CurrentQuantity < quantity) throw new Exception("库存不足");

        inventory.CurrentQuantity -= quantity;
        inventory.LastUpdated = DateTime.Now;
        await _inventoryRepository.UpdateAsync(inventory);

        // 记录交易
        var transaction = new InventoryTransaction
        {
            InventoryId = inventoryId,
            Inventory = inventory,
            Quantity = -quantity,
            TransactionType = "Outbound",
            Reason = reason,
            Reference = reference,
            Operator = "System",
            TransactionDate = DateTime.Now
        };
        await _transactionRepository.AddAsync(transaction);

        // 清除缓存
        await ClearInventoryCache(inventoryId);

        return _mapper.Map<InventoryDto>(inventory);
    }

    public async Task<InventoryTransactionDto> CreateTransactionAsync(InventoryTransactionDto dto)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(dto.InventoryId);
        if (inventory == null) throw new Exception("库存不存在");

        var beforeQuantity = inventory.CurrentQuantity;
        
        if (dto.TransactionType == "Inbound" || dto.TransactionType == "入库")
        {
            inventory.CurrentQuantity += dto.Quantity;
        }
        else if (dto.TransactionType == "Outbound" || dto.TransactionType == "出库")
        {
            if (inventory.CurrentQuantity < dto.Quantity)
                throw new Exception("库存不足");
            inventory.CurrentQuantity -= dto.Quantity;
        }
        else
        {
            throw new Exception("无效的事务类型");
        }

        inventory.LastUpdated = DateTime.Now;
        await _inventoryRepository.UpdateAsync(inventory);

        var transaction = _mapper.Map<InventoryTransaction>(dto);
        transaction.TransactionDate = DateTime.Now;
        transaction.Operator = "System";
        
        var createdTransaction = await _transactionRepository.AddAsync(transaction);
        
        // 清除缓存
        await ClearInventoryCache(dto.InventoryId);

        return _mapper.Map<InventoryTransactionDto>(createdTransaction);
    }

    public async Task<IEnumerable<InventoryTransactionDto>> GetTransactionsAsync(int inventoryId)
    {
        var cacheKey = $"inventory:transactions:{inventoryId}";
        var cachedData = await _cacheService.GetAsync<IEnumerable<InventoryTransactionDto>>(cacheKey);
        if (cachedData != null) return cachedData;

        var transactions = await _transactionRepository.GetByInventoryIdAsync(inventoryId);
        var result = _mapper.Map<IEnumerable<InventoryTransactionDto>>(transactions);
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        return result;
    }

    public async Task<IEnumerable<InventoryTransactionDto>> GetTransactionsAsync(DateTime? startDate, DateTime? endDate)
    {
        var cacheKey = $"inventory:transactions:all:{startDate?.ToString("yyyyMMdd")}:{endDate?.ToString("yyyyMMdd")}";
        var cachedData = await _cacheService.GetAsync<IEnumerable<InventoryTransactionDto>>(cacheKey);
        if (cachedData != null) return cachedData;

        var transactions = await _transactionRepository.GetAllAsync();
        
        if (startDate.HasValue)
            transactions = transactions.Where(t => t.TransactionDate >= startDate.Value);
        
        if (endDate.HasValue)
            transactions = transactions.Where(t => t.TransactionDate <= endDate.Value);
        
        var result = _mapper.Map<IEnumerable<InventoryTransactionDto>>(transactions);
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(3));
        return result;
    }

    public async Task<IEnumerable<InventoryTransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var cacheKey = $"inventory:transactions:range:{startDate.ToString("yyyyMMdd")}:{endDate.ToString("yyyyMMdd")}";
        var cachedData = await _cacheService.GetAsync<IEnumerable<InventoryTransactionDto>>(cacheKey);
        if (cachedData != null) return cachedData;

        var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);
        var result = _mapper.Map<IEnumerable<InventoryTransactionDto>>(transactions);
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(3));
        return result;
    }

    public async Task<InventoryReportDto> GetInventoryReportAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var cacheKey = $"inventory:report:all:{startDate?.ToString("yyyyMMdd")}:{endDate?.ToString("yyyyMMdd")}";
        var cachedData = await _cacheService.GetAsync<InventoryReportDto>(cacheKey);
        if (cachedData != null) return cachedData;

        var inventories = await _inventoryRepository.GetAllAsync();
        var lowStockCount = inventories.Where(i => i.CurrentQuantity <= i.AlertMinQuantity).Count();
        var zeroStockCount = inventories.Where(i => i.CurrentQuantity == 0).Count();
        var totalQuantity = inventories.Sum(i => i.CurrentQuantity);

        var result = new InventoryReportDto
        {
            Category = "全部",
            TotalItems = inventories.Count(),
            LowStockItems = lowStockCount,
            ZeroStockItems = zeroStockCount,
            TotalQuantity = totalQuantity
        };

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        return result;
    }

    public async Task<IEnumerable<InventoryReportDto>> GetCategoryReportAsync()
    {
        var cacheKey = "inventory:report:category";
        var cachedData = await _cacheService.GetAsync<IEnumerable<InventoryReportDto>>(cacheKey);
        if (cachedData != null) return cachedData;

        var inventories = await _inventoryRepository.GetAllAsync();
        var reports = new List<InventoryReportDto>();

        // 按设备类型分组
        var specialEquipments = inventories.Where(i => i.SpecialEquipmentId != null);
        var generalEquipments = inventories.Where(i => i.GeneralEquipmentId != null);

        reports.Add(new InventoryReportDto
        {
            Category = "专用设备",
            TotalItems = specialEquipments.Count(),
            LowStockItems = specialEquipments.Where(i => i.CurrentQuantity <= i.AlertMinQuantity).Count(),
            ZeroStockItems = specialEquipments.Where(i => i.CurrentQuantity == 0).Count(),
            TotalQuantity = specialEquipments.Sum(i => i.CurrentQuantity)
        });

        reports.Add(new InventoryReportDto
        {
            Category = "通用设备",
            TotalItems = generalEquipments.Count(),
            LowStockItems = generalEquipments.Where(i => i.CurrentQuantity <= i.AlertMinQuantity).Count(),
            ZeroStockItems = generalEquipments.Where(i => i.CurrentQuantity == 0).Count(),
            TotalQuantity = generalEquipments.Sum(i => i.CurrentQuantity)
        });

        // 获取耗材数据
        var consumables = await _consumableRepository.GetAllAsync();
        reports.Add(new InventoryReportDto
        {
            Category = "耗材",
            TotalItems = consumables.Count(),
            LowStockItems = consumables.Where(c => c.RemainingQuantity <= DefaultLowStockThreshold).Count(), // 使用默认预警阈值常量
            ZeroStockItems = consumables.Where(c => c.RemainingQuantity == 0).Count(),
            TotalQuantity = consumables.Sum(c => c.RemainingQuantity)
        });

        // 获取原材料数据
        var rawMaterials = await _rawMaterialRepository.GetAllAsync();
        reports.Add(new InventoryReportDto
        {
            Category = "原材料",
            TotalItems = rawMaterials.Count(),
            LowStockItems = rawMaterials.Where(r => r.RemainingQuantity <= DefaultLowStockThreshold).Count(), // 使用默认预警阈值常量
            ZeroStockItems = rawMaterials.Where(r => r.RemainingQuantity == 0).Count(),
            TotalQuantity = rawMaterials.Sum(r => r.RemainingQuantity)
        });

        await _cacheService.SetAsync(cacheKey, reports, TimeSpan.FromMinutes(5));
        return reports;
    }
}
