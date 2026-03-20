using AutoMapper;
using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;

using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Enums;
using DeviceWarehouse.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DeviceWarehouse.Application.Services;

public class ProjectOutboundService(
    IProjectOutboundRepository outboundRepository,
    IProjectInboundRepository inboundRepository,
    ISpecialEquipmentRepository specialEquipmentRepository,
    IGeneralEquipmentRepository generalEquipmentRepository,
    IConsumableRepository consumableRepository,
    IRawMaterialRepository rawMaterialRepository,
    IInventoryRepository inventoryRepository,
    IMapper mapper,
    ICacheService cache,
    ILogger<ProjectOutboundService> logger) : CachedServiceBase(cache, logger), IProjectOutboundService
{

    public async Task<ProjectOutboundDto> GetByIdAsync(int id)
    {
        var cacheKey = BuildCacheKey("projectoutbound", id);
        var cached = await GetFromCacheAsync<ProjectOutboundDto>(cacheKey);
        if (cached != null) return cached;

        var outbound = await outboundRepository.GetByIdAsync(id);
        if (outbound == null)
            throw new Exception("项目出库单不存在");
        
        var dto = mapper.Map<ProjectOutboundDto>(outbound);
        dto.TotalQuantity = outbound.Items.Sum(i => i.Quantity);
        
        // 检查入库状态
        var allInbounds = await inboundRepository.GetAllAsync();
        var hasCompletedInbound = allInbounds.Any(inbound => 
            inbound.ProjectInboundOutbounds.Any(pi => pi.ProjectOutboundId == dto.Id) && inbound.IsCompleted);
        var hasPartialInbound = allInbounds.Any(inbound => 
            inbound.ProjectInboundOutbounds.Any(pi => pi.ProjectOutboundId == dto.Id) && !inbound.IsCompleted && inbound.Status == "部分入库");
        
        if (hasCompletedInbound)
        {
            dto.InboundStatus = "入库完成";
        }
        else if (hasPartialInbound)
        {
            dto.InboundStatus = "部分入库";
        }
        else
        {
            dto.InboundStatus = "未入库";
        }
        
        await SetCacheAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
        return dto;
    }

    public async Task<IEnumerable<ProjectOutboundDto>> GetAllAsync()
    {
        var cacheKey = BuildCacheKey("projectoutbounds", "all");
        var cached = await GetFromCacheAsync<List<ProjectOutboundDto>>(cacheKey);
        if (cached != null) return cached;

        try
        {
            var outbounds = await outboundRepository.GetAllAsync();
            Console.WriteLine($"[DEBUG] GetAllAsync retrieved {outbounds?.Count() ?? 0} outbounds");
            
            // 处理outbounds为null的情况
            if (outbounds is null)
            {
                var emptyList = new List<ProjectOutboundDto>();
                await SetCacheAsync(cacheKey, emptyList, TimeSpan.FromMinutes(2));
                return emptyList;
            }
            
            var dtos = mapper.Map<List<ProjectOutboundDto>>(outbounds);
            Console.WriteLine($"[DEBUG] Mapped to {dtos?.Count ?? 0} DTOs");
            
            // 处理dtos为null的情况
            if (dtos is null)
            {
                var emptyList = new List<ProjectOutboundDto>();
                await SetCacheAsync(cacheKey, emptyList, TimeSpan.FromMinutes(2));
                return emptyList;
            }
            
            // 获取所有入库单，用于检查出库单的入库状态
            var allInbounds = await inboundRepository.GetAllAsync();
            
            foreach (var dto in dtos)
            {
                var outbound = outbounds.FirstOrDefault(i => i.Id == dto.Id);
                if (outbound != null && outbound.Items != null)
                {
                    dto.TotalQuantity = outbound.Items.Sum(i => i.Quantity);
                }
                else
                {
                    dto.TotalQuantity = 0;
                }
                
                // 检查入库状态
                var hasCompletedInbound = allInbounds.Any(inbound => 
                    inbound.ProjectInboundOutbounds.Any(pi => pi.ProjectOutboundId == dto.Id) && inbound.IsCompleted);
                var hasPartialInbound = allInbounds.Any(inbound => 
                    inbound.ProjectInboundOutbounds.Any(pi => pi.ProjectOutboundId == dto.Id) && !inbound.IsCompleted && inbound.Status == "部分入库");
                
                if (hasCompletedInbound)
                {
                    dto.InboundStatus = "入库完成";
                }
                else if (hasPartialInbound)
                {
                    dto.InboundStatus = "部分入库";
                }
                else
                {
                    dto.InboundStatus = "未入库";
                }
            }
            
            await SetCacheAsync(cacheKey, dtos, TimeSpan.FromMinutes(2));
            return dtos;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] GetAllAsync Service failed: {ex.Message}");
            Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[ERROR] InnerException: {ex.InnerException.Message}");
            }
            throw new Exception("获取项目出库单列表失败: " + ex.Message);
        }
    }

    private static string GetDeviceStatusText(DeviceStatus? status)
    {
        if (!status.HasValue)
            return "";
            
        return status.Value switch
        {
            DeviceStatus.Normal => "正常",
            DeviceStatus.Broken => "损坏",
            DeviceStatus.Scrap => "报废",
            _ => status.Value.ToString()
        };
    }

    public async Task<IEnumerable<AvailableItemDto>> GetAvailableItemsAsync(string? keyword = null)
    {
        // 移除缓存逻辑，确保每次都返回最新的设备列表
        var availableItems = new List<AvailableItemDto>();

        // 串行获取四种类型的数据（DbContext 不是线程安全的）
        // 如果有keyword，使用带keyword的查询；否则获取所有可用设备
        var specialEquipments = await specialEquipmentRepository.GetAvailableAsync(keyword);
        var generalEquipments = await generalEquipmentRepository.GetAvailableAsync(keyword);
        var consumables = await consumableRepository.GetAvailableAsync(keyword);
        var rawMaterials = await rawMaterialRepository.GetAvailableAsync(keyword);

        Console.WriteLine($"[DEBUG] GetAvailableItemsAsync - 专用设备数量: {specialEquipments.Count()}");
        Console.WriteLine($"[DEBUG] GetAvailableItemsAsync - 通用设备数量: {generalEquipments.Count()}");
        Console.WriteLine($"[DEBUG] GetAvailableItemsAsync - 耗材数量: {consumables.Count()}");
        Console.WriteLine($"[DEBUG] GetAvailableItemsAsync - 原材料数量: {rawMaterials.Count()}");

        // 处理专用设备 - 不分组，返回所有详细设备
        // 对于数量大于1的设备，创建多个实例
        var specialEquipmentItems = new List<AvailableItemDto>();
        foreach (var e in specialEquipments)
        {
            // 为每个数量创建一个实例
            for (int i = 0; i < e.Quantity; i++)
            {
                specialEquipmentItems.Add(new AvailableItemDto
                {
                    id = e.Id, // 使用真实的设备ID
                    itemType = ItemType.SpecialEquipment,
                    itemTypeName = "专用设备",
                    name = e.DeviceName,
                    brand = e.Brand,
                    model = e.Model,
                    availableQuantity = 1, // 每个实例的数量为1
                    unit = e.Unit,
                    location = e.Location,
                    company = e.Company,
                    deviceCode = e.DeviceCode,
                    accessories = e.Accessories,
                    remark = e.Remark,
                    deviceStatus = GetDeviceStatusText(e.DeviceStatus)
                });
            }
        }
        availableItems.AddRange(specialEquipmentItems);

        // 处理通用设备 - 不分组，返回所有详细设备
        // 对于数量大于1的设备，创建多个实例
        var generalEquipmentItems = new List<AvailableItemDto>();
        foreach (var e in generalEquipments)
        {
            // 为每个数量创建一个实例
            for (int i = 0; i < e.Quantity; i++)
            {
                generalEquipmentItems.Add(new AvailableItemDto
                {
                    id = e.Id, // 使用真实的设备ID
                    itemType = ItemType.GeneralEquipment,
                    itemTypeName = "通用设备",
                    name = e.DeviceName,
                    brand = e.Brand,
                    model = e.Model,
                    availableQuantity = 1, // 每个实例的数量为1
                    unit = e.Unit,
                    location = e.Location,
                    company = e.Company,
                    deviceCode = e.DeviceCode,
                    accessories = e.Accessories,
                    remark = e.Remark,
                    deviceStatus = GetDeviceStatusText(e.DeviceStatus)
                });
            }
        }
        availableItems.AddRange(generalEquipmentItems);

        // 处理耗材 - 只返回一条记录，包含可用数量的总数
        var consumableItems = consumables
            .Select(c => new AvailableItemDto
            {
                id = c.Id,
                itemType = ItemType.Consumable,
                itemTypeName = "耗材",
                name = c.Name,
                brand = c.Brand,
                model = c.ModelSpecification,
                availableQuantity = c.RemainingQuantity, // 显示总数量
                unit = c.Unit,
                location = c.Location,
                company = c.Company,
                accessories = c.Accessories,
                remark = c.Remark
            });
        availableItems.AddRange(consumableItems);

        // 处理原材料 - 只返回一条记录，包含可用数量的总数
        var rawMaterialItems = rawMaterials
            .Select(m => new AvailableItemDto
            {
                id = m.Id,
                itemType = ItemType.RawMaterial,
                itemTypeName = "原材料",
                name = m.ProductName,
                brand = m.Supplier,
                model = m.Specification,
                availableQuantity = m.RemainingQuantity, // 显示总数量
                unit = m.Unit,
                location = null,
                company = m.Company,
                remark = m.Remark
            });
        availableItems.AddRange(rawMaterialItems);

        Console.WriteLine($"[DEBUG] GetAvailableItemsAsync - 总设备数量: {availableItems.Count}");

        return availableItems;
    }

    public async Task<AvailableItemsResponseDto> GetAvailableItemsPagedAsync(AvailableItemsRequestDto request)
    {
        Console.WriteLine($"[GetAvailableItemsPagedAsync] Received request: ItemType={request.ItemType}, Keyword={request.Keyword}, PageNumber={request.PageNumber}, PageSize={request.PageSize}");
        var cacheKey = BuildCacheKey("availableitems_paged", $"{request.Keyword ?? "all"}_{request.PageNumber}_{request.PageSize}_{request.ItemType}");
        var cached = await GetFromCacheAsync<AvailableItemsResponseDto>(cacheKey);
        if (cached != null) return cached;

        var keyword = request.Keyword ?? "";
        var allItems = new List<AvailableItemDto>();
        int totalCount = 0;

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // 根据ItemType选择查询策略
        if (request.ItemType.HasValue)
        {
            // 特定类型查询 - 使用数据库层面分页
            Console.WriteLine($"[GetAvailableItemsPagedAsync] Processing item type: {request.ItemType.Value}");
            switch (request.ItemType.Value)
            {
                case 1: // 专用设备
                    Console.WriteLine("[GetAvailableItemsPagedAsync] Calling GetSpecialEquipmentPagedAsync");
                    (allItems, totalCount) = await GetSpecialEquipmentPagedAsync(keyword, request.PageNumber, request.PageSize);
                    break;
                case 2: // 通用设备
                    Console.WriteLine("[GetAvailableItemsPagedAsync] Calling GetGeneralEquipmentPagedAsync");
                    (allItems, totalCount) = await GetGeneralEquipmentPagedAsync(keyword, request.PageNumber, request.PageSize);
                    break;
                case 3: // 耗材
                    Console.WriteLine("[GetAvailableItemsPagedAsync] Calling GetConsumablesPagedAsync");
                    (allItems, totalCount) = await GetConsumablesPagedAsync(keyword, request.PageNumber, request.PageSize);
                    break;
                case 4: // 原材料
                    Console.WriteLine("[GetAvailableItemsPagedAsync] Calling GetRawMaterialsPagedAsync");
                    (allItems, totalCount) = await GetRawMaterialsPagedAsync(keyword, request.PageNumber, request.PageSize);
                    break;
                default:
                    Console.WriteLine($"[GetAvailableItemsPagedAsync] Unknown item type: {request.ItemType.Value}");
                    break;
            }
        }
        else
        {
            // 查询所有类型 - 先获取总数，再分页查询
            Console.WriteLine("[GetAvailableItemsPagedAsync] Calling GetAllItemsPagedAsync");
            (allItems, totalCount) = await GetAllItemsPagedAsync(keyword, request.PageNumber, request.PageSize);
        }

        stopwatch.Stop();
        Console.WriteLine($"[Performance] GetAvailableItemsPagedAsync total time: {stopwatch.ElapsedMilliseconds}ms");

        var response = new AvailableItemsResponseDto
        {
            items = allItems,
            totalCount = totalCount,
            pageNumber = request.PageNumber,
            pageSize = request.PageSize
        };

        await SetCacheAsync(cacheKey, response, TimeSpan.FromMinutes(1));
        return response;
    }

    private async Task<(List<AvailableItemDto> Items, int TotalCount)> GetSpecialEquipmentPagedAsync(string keyword, int pageNumber, int pageSize)
    {
        // 使用优化的SQL查询，避免复杂的字符串连接
        var countSql = $@"SELECT COUNT(*) AS TotalCount FROM (
            SELECT DISTINCT DeviceName, ISNULL(Brand, '') AS Brand, ISNULL(Model, '') AS Model
            FROM SpecialEquipment
            WHERE Quantity > 0 AND UseStatus != 1
            {(string.IsNullOrEmpty(keyword) ? "" : $"AND (DeviceName LIKE '%{keyword}%' OR Brand LIKE '%{keyword}%' OR Model LIKE '%{keyword}%')")}
        ) AS DistinctItems";

        var dataSql = $@"SELECT 
            MIN(Id) AS Id,
            DeviceName AS Name, 
            MAX(Brand) AS Brand, 
            MAX(Model) AS Model, 
            SUM(Quantity) AS AvailableQuantity, 
            MAX(Unit) AS Unit, 
            MAX(Location) AS Location, 
            MAX(Company) AS Company,
            MAX(DeviceStatus) AS DeviceStatus
        FROM SpecialEquipment
        WHERE Quantity > 0 AND UseStatus != 1
        {(string.IsNullOrEmpty(keyword) ? "" : $"AND (DeviceName LIKE '%{keyword}%' OR Brand LIKE '%{keyword}%' OR Model LIKE '%{keyword}%')")}
        GROUP BY DeviceName, ISNULL(Brand, ''), ISNULL(Model, '')
        ORDER BY DeviceName
        OFFSET {(pageNumber - 1) * pageSize} ROWS
        FETCH NEXT {pageSize} ROWS ONLY";

        var countResult = await specialEquipmentRepository.ExecuteRawSqlAsync(countSql, new { });
        var items = await specialEquipmentRepository.ExecuteRawSqlAsync(dataSql, new { });
        
        int totalCount = 0;
        if (countResult.Count > 0)
        {
            var first = countResult.First();
            totalCount = (int)(first.TotalCount ?? first.totalCount ?? 0);
        }

        var result = items.Select((item, index) => new AvailableItemDto
        {
            id = item.Id, // 使用分组中的第一个设备ID
            itemType = ItemType.SpecialEquipment,
            itemTypeName = "专用设备",
            name = item.Name,
            brand = item.Brand,
            model = item.Model,
            availableQuantity = item.AvailableQuantity,
            unit = item.Unit,
            location = item.Location,
            company = item.Company,
            deviceCode = null,
            deviceStatus = GetDeviceStatusText(Enum.TryParse<DeviceStatus>(item.DeviceStatus?.ToString(), out DeviceStatus status) ? status : null)
        }).ToList();

        return (result, totalCount);
    }

    private async Task<(List<AvailableItemDto> Items, int TotalCount)> GetGeneralEquipmentPagedAsync(string keyword, int pageNumber, int pageSize)
    {
        // 使用优化的SQL查询，避免复杂的字符串连接
        var countSql = $@"SELECT COUNT(*) AS TotalCount FROM (
            SELECT DISTINCT DeviceName, ISNULL(Brand, '') AS Brand, ISNULL(Model, '') AS Model
            FROM GeneralEquipment
            WHERE Quantity > 0 AND UseStatus != 1
            {(string.IsNullOrEmpty(keyword) ? "" : $"AND (DeviceName LIKE '%{keyword}%' OR Brand LIKE '%{keyword}%' OR Model LIKE '%{keyword}%')")}
        ) AS DistinctItems";

        var dataSql = $@"SELECT 
            MIN(Id) AS Id,
            DeviceName AS Name, 
            MAX(Brand) AS Brand, 
            MAX(Model) AS Model, 
            SUM(Quantity) AS AvailableQuantity, 
            MAX(Unit) AS Unit, 
            MAX(Location) AS Location, 
            MAX(Company) AS Company,
            MAX(DeviceStatus) AS DeviceStatus
        FROM GeneralEquipment
        WHERE Quantity > 0 AND UseStatus != 1
        {(string.IsNullOrEmpty(keyword) ? "" : $"AND (DeviceName LIKE '%{keyword}%' OR Brand LIKE '%{keyword}%' OR Model LIKE '%{keyword}%')")}
        GROUP BY DeviceName, ISNULL(Brand, ''), ISNULL(Model, '')
        ORDER BY DeviceName
        OFFSET {(pageNumber - 1) * pageSize} ROWS
        FETCH NEXT {pageSize} ROWS ONLY";

        var countResult = await generalEquipmentRepository.ExecuteRawSqlAsync(countSql, new { });
        var items = await generalEquipmentRepository.ExecuteRawSqlAsync(dataSql, new { });
        
        int totalCount = 0;
        if (countResult.Count > 0)
        {
            var first = countResult.First();
            totalCount = (int)(first.TotalCount ?? first.totalCount ?? 0);
        }

        var result = items.Select((item, index) => new AvailableItemDto
        {
            id = item.Id, // 使用分组中的第一个设备ID
            itemType = ItemType.GeneralEquipment,
            itemTypeName = "通用设备",
            name = item.Name,
            brand = item.Brand,
            model = item.Model,
            availableQuantity = item.AvailableQuantity,
            unit = item.Unit,
            location = item.Location,
            company = item.Company,
            deviceCode = null,
            deviceStatus = GetDeviceStatusText(Enum.TryParse<DeviceStatus>(item.DeviceStatus?.ToString(), out DeviceStatus status) ? status : null)
        }).ToList();

        return (result, totalCount);
    }

    private async Task<(List<AvailableItemDto> Items, int TotalCount)> GetConsumablesPagedAsync(string keyword, int pageNumber, int pageSize)
    {
        // 使用优化的SQL查询，避免复杂的字符串连接
        var countSql = $@"SELECT COUNT(*) AS TotalCount FROM (
            SELECT DISTINCT Name, ISNULL(Brand, '') AS Brand, ISNULL(ModelSpecification, '') AS ModelSpecification
            FROM Consumables
            WHERE RemainingQuantity > 0
            {(string.IsNullOrEmpty(keyword) ? "" : $"AND (Name LIKE '%{keyword}%' OR Brand LIKE '%{keyword}%' OR ModelSpecification LIKE '%{keyword}%')")}
        ) AS DistinctItems";

        var dataSql = $@"SELECT 
            MIN(Id) AS Id,
            Name, 
            MAX(Brand) AS Brand, 
            MAX(ModelSpecification) AS Model, 
            SUM(RemainingQuantity) AS AvailableQuantity, 
            MAX(Unit) AS Unit, 
            MAX(Location) AS Location, 
            MAX(Company) AS Company
        FROM Consumables
        WHERE RemainingQuantity > 0
        {(string.IsNullOrEmpty(keyword) ? "" : $"AND (Name LIKE '%{keyword}%' OR Brand LIKE '%{keyword}%' OR ModelSpecification LIKE '%{keyword}%')")}
        GROUP BY Name, ISNULL(Brand, ''), ISNULL(ModelSpecification, '')
        ORDER BY Name
        OFFSET {(pageNumber - 1) * pageSize} ROWS
        FETCH NEXT {pageSize} ROWS ONLY";

        var countResult = await consumableRepository.ExecuteRawSqlAsync(countSql, new { });
        var items = await consumableRepository.ExecuteRawSqlAsync(dataSql, new { });
        
        int totalCount = 0;
        if (countResult.Count > 0)
        {
            var first = countResult.First();
            totalCount = (int)(first.TotalCount ?? first.totalCount ?? 0);
        }

        var result = items.Select((item, index) => new AvailableItemDto
        {
            id = item.Id, // 使用分组中的第一个耗材ID
            itemType = ItemType.Consumable,
            itemTypeName = "耗材",
            name = item.Name,
            brand = item.Brand,
            model = item.Model,
            availableQuantity = item.AvailableQuantity,
            unit = item.Unit,
            location = item.Location,
            company = item.Company,
            deviceCode = null
        }).ToList();

        return (result, totalCount);
    }

    private async Task<(List<AvailableItemDto> Items, int TotalCount)> GetRawMaterialsPagedAsync(string keyword, int pageNumber, int pageSize)
    {
        // 使用优化的SQL查询，避免复杂的字符串连接
        var countSql = $@"SELECT COUNT(*) AS TotalCount FROM (
            SELECT DISTINCT ProductName, ISNULL(Supplier, '') AS Supplier, ISNULL(Specification, '') AS Specification
            FROM RawMaterials
            WHERE RemainingQuantity > 0
            {(string.IsNullOrEmpty(keyword) ? "" : $"AND (ProductName LIKE '%{keyword}%' OR Supplier LIKE '%{keyword}%' OR Specification LIKE '%{keyword}%')")}
        ) AS DistinctItems";

        var dataSql = $@"SELECT 
            MIN(Id) AS Id,
            ProductName AS Name, 
            MAX(Supplier) AS Brand, 
            MAX(Specification) AS Model, 
            SUM(RemainingQuantity) AS AvailableQuantity, 
            MAX(Unit) AS Unit, 
            MAX(Company) AS Company
        FROM RawMaterials
        WHERE RemainingQuantity > 0
        {(string.IsNullOrEmpty(keyword) ? "" : $"AND (ProductName LIKE '%{keyword}%' OR Supplier LIKE '%{keyword}%' OR Specification LIKE '%{keyword}%')")}
        GROUP BY ProductName, ISNULL(Supplier, ''), ISNULL(Specification, '')
        ORDER BY ProductName
        OFFSET {(pageNumber - 1) * pageSize} ROWS
        FETCH NEXT {pageSize} ROWS ONLY";

        var countResult = await rawMaterialRepository.ExecuteRawSqlAsync(countSql, new { });
        var items = await rawMaterialRepository.ExecuteRawSqlAsync(dataSql, new { });
        
        int totalCount = 0;
        if (countResult.Count > 0)
        {
            var first = countResult.First();
            totalCount = (int)(first.TotalCount ?? first.totalCount ?? 0);
        }

        var result = items.Select((item, index) => new AvailableItemDto
        {
            id = item.Id, // 使用分组中的第一个原材料ID
            itemType = ItemType.RawMaterial,
            itemTypeName = "原材料",
            name = item.Name,
            brand = item.Brand,
            model = item.Model,
            availableQuantity = item.AvailableQuantity,
            unit = item.Unit,
            location = null,
            company = item.Company,
            deviceCode = null
        }).ToList();

        return (result, totalCount);
    }

    private async Task<(List<AvailableItemDto> Items, int TotalCount)> GetAllItemsPagedAsync(string keyword, int pageNumber, int pageSize)
    {
        // 对于"全部"类型，使用临时表方式优化
        var specialEquipments = new List<dynamic>();
        var generalEquipments = new List<dynamic>();
        var consumables = new List<dynamic>();
        var rawMaterials = new List<dynamic>();

        // 并行查询所有类型
        var searchKeyword = keyword ?? "";
        
        // 使用原始SQL查询获取专用设备
        var specialSql = $@"SELECT 
            MIN(Id) AS Id,
            DeviceName AS Name, 
            Brand, 
            Model, 
            SUM(Quantity) AS AvailableQuantity, 
            MAX(Unit) AS Unit, 
            MAX(Location) AS Location, 
            MAX(Company) AS Company,
            MAX(DeviceStatus) AS DeviceStatus
        FROM 
            SpecialEquipment
        WHERE 
            Quantity > 0 AND UseStatus != 1
            {(string.IsNullOrEmpty(searchKeyword) ? "" : $"AND (DeviceName LIKE '%{searchKeyword}%' OR Brand LIKE '%{searchKeyword}%' OR Model LIKE '%{searchKeyword}%')")}
        GROUP BY 
            DeviceName, Brand, Model
        ORDER BY 
            DeviceName";

        specialEquipments = await specialEquipmentRepository.ExecuteRawSqlAsync(specialSql, new { });

        // 使用原始SQL查询获取通用设备
        var generalSql = $@"SELECT 
            MIN(Id) AS Id,
            DeviceName AS Name, 
            Brand, 
            Model, 
            SUM(Quantity) AS AvailableQuantity, 
            MAX(Unit) AS Unit, 
            MAX(Location) AS Location, 
            MAX(Company) AS Company,
            MAX(DeviceStatus) AS DeviceStatus
        FROM 
            GeneralEquipment
        WHERE 
            Quantity > 0 AND UseStatus != 1
            {(string.IsNullOrEmpty(searchKeyword) ? "" : $"AND (DeviceName LIKE '%{searchKeyword}%' OR Brand LIKE '%{searchKeyword}%' OR Model LIKE '%{searchKeyword}%')")}
        GROUP BY 
            DeviceName, Brand, Model
        ORDER BY 
            DeviceName";

        generalEquipments = await generalEquipmentRepository.ExecuteRawSqlAsync(generalSql, new { });

        // 使用原始SQL查询获取耗材
        var consumableSql = $@"SELECT 
            MIN(Id) AS Id,
            Name, 
            Brand, 
            ModelSpecification AS Model, 
            SUM(RemainingQuantity) AS AvailableQuantity, 
            MAX(Unit) AS Unit, 
            MAX(Location) AS Location, 
            MAX(Company) AS Company
        FROM 
            Consumables
        WHERE 
            RemainingQuantity > 0
            {(string.IsNullOrEmpty(searchKeyword) ? "" : $"AND (Name LIKE '%{searchKeyword}%' OR Brand LIKE '%{searchKeyword}%' OR ModelSpecification LIKE '%{searchKeyword}%')")}
        GROUP BY 
            Name, Brand, ModelSpecification
        ORDER BY 
            Name";

        consumables = await consumableRepository.ExecuteRawSqlAsync(consumableSql, new { });

        // 使用原始SQL查询获取原材料
        var rawMaterialSql = $@"SELECT 
            MIN(Id) AS Id,
            ProductName AS Name, 
            Supplier AS Brand, 
            Specification AS Model, 
            SUM(RemainingQuantity) AS AvailableQuantity, 
            MAX(Unit) AS Unit, 
            MAX(Company) AS Company
        FROM 
            RawMaterials
        WHERE 
            RemainingQuantity > 0
            {(string.IsNullOrEmpty(searchKeyword) ? "" : $"AND (ProductName LIKE '%{searchKeyword}%' OR Supplier LIKE '%{searchKeyword}%' OR Specification LIKE '%{searchKeyword}%')")}
        GROUP BY 
            ProductName, Supplier, Specification
        ORDER BY 
            ProductName";

        rawMaterials = await rawMaterialRepository.ExecuteRawSqlAsync(rawMaterialSql, new { });

        // 合并所有结果
        var allItems = new List<AvailableItemDto>();

        // 添加专用设备
        allItems.AddRange(specialEquipments.Select((item, index) => new AvailableItemDto
        {
            id = item.Id, // 使用分组中的第一个设备ID
            itemType = ItemType.SpecialEquipment,
            itemTypeName = "专用设备",
            name = item.Name,
            brand = item.Brand,
            model = item.Model,
            availableQuantity = item.AvailableQuantity,
            unit = item.Unit,
            location = item.Location,
            company = item.Company,
            deviceCode = null,
            deviceStatus = GetDeviceStatusText(Enum.TryParse<DeviceStatus>(item.DeviceStatus?.ToString(), out DeviceStatus status) ? status : null)
        }));

        // 添加通用设备
        allItems.AddRange(generalEquipments.Select((item, index) => new AvailableItemDto
        {
            id = item.Id, // 使用分组中的第一个设备ID
            itemType = ItemType.GeneralEquipment,
            itemTypeName = "通用设备",
            name = item.Name,
            brand = item.Brand,
            model = item.Model,
            availableQuantity = item.AvailableQuantity,
            unit = item.Unit,
            location = item.Location,
            company = item.Company,
            deviceCode = null,
            deviceStatus = GetDeviceStatusText(Enum.TryParse<DeviceStatus>(item.DeviceStatus?.ToString(), out DeviceStatus status) ? status : null)
        }));

        // 添加耗材
        allItems.AddRange(consumables.Select((item, index) => new AvailableItemDto
        {
            id = item.Id, // 使用分组中的第一个耗材ID
            itemType = ItemType.Consumable,
            itemTypeName = "耗材",
            name = item.Name,
            brand = item.Brand,
            model = item.Model,
            availableQuantity = item.AvailableQuantity,
            unit = item.Unit,
            location = item.Location,
            company = item.Company,
            deviceCode = null
        }));

        // 添加原材料
        allItems.AddRange(rawMaterials.Select((item, index) => new AvailableItemDto
        {
            id = item.Id, // 使用分组中的第一个原材料ID
            itemType = ItemType.RawMaterial,
            itemTypeName = "原材料",
            name = item.Name,
            brand = item.Brand,
            model = item.Model,
            availableQuantity = item.AvailableQuantity,
            unit = item.Unit,
            location = null,
            company = item.Company,
            deviceCode = null
        }));

        // 对合并后的数据进行去重，按名称、品牌、型号分组，将数量相加
        allItems = allItems
            .GroupBy(item => new { 
                name = item.name, 
                brand = item.brand ?? "", 
                model = item.model ?? ""
            })
            .Select(group => {
                var firstItem = group.First();
                return new AvailableItemDto
                {
                    id = firstItem.id,
                    itemType = firstItem.itemType,
                    itemTypeName = firstItem.itemTypeName,
                    name = firstItem.name,
                    brand = group.Key.brand == "" ? null : group.Key.brand,
                    model = group.Key.model == "" ? null : group.Key.model,
                    availableQuantity = group.Sum(item => item.availableQuantity), // 相加数量
                    unit = firstItem.unit,
                    location = firstItem.location,
                    company = firstItem.company,
                    deviceCode = firstItem.deviceCode,
                    accessories = firstItem.accessories,
                    remark = firstItem.remark,
                    deviceStatus = firstItem.deviceStatus
                };
            })
            .ToList();

        // 对合并后的数据进行分页
        int totalCount = allItems.Count;
        var pagedItems = allItems
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (pagedItems, totalCount);
    }

    public async Task<ProjectOutboundDto> CreateAsync(CreateProjectOutboundDto dto)
    {
        if (dto.Items is null || dto.Items.Count == 0)
            throw new Exception("出库明细不能为空");

        // 创建出库单时不更新库存，只保存记录
        // 库存将在点击"完成"按钮时更新
        var outbound = mapper.Map<ProjectOutbound>(dto);
        outbound.CreatedAt = DateTime.Now;
        outbound.IsCompleted = false;
        
        // 处理图片列表
        if (dto.OutboundImages is not null && dto.OutboundImages.Count > 0)
        {
            outbound.OutboundImages = System.Text.Json.JsonSerializer.Serialize(dto.OutboundImages);
        }

        // 自动生成出库单号（如果未提供）
        if (string.IsNullOrWhiteSpace(outbound.OutboundNumber))
        {
            outbound.OutboundNumber = await GenerateOutboundNumberAsync();
        }

        // 设置每个 item 的创建时间（AutoMapper 已经映射了 Items，不需要手动添加）
        foreach (var item in outbound.Items)
        {
            item.CreatedAt = DateTime.Now;
        }

        var createdOutbound = await outboundRepository.AddAsync(outbound);
        
        // 清除相关缓存
        await InvalidateCacheAsync(BuildCacheKey("projectoutbounds", "all"));
        await InvalidateCacheAsync(BuildCacheKey("availableitems", "all"));
        
        var resultDto = mapper.Map<ProjectOutboundDto>(createdOutbound);
        resultDto.TotalQuantity = createdOutbound.Items.Sum(i => i.Quantity);
        return resultDto;
    }

    public async Task<ProjectOutboundDto> UpdateAsync(int id, UpdateProjectOutboundDto dto)
    {
        var outbound = await outboundRepository.GetByIdAsync(id);
        if (outbound == null)
            throw new Exception("项目出库单不存在");

        if (outbound.IsCompleted)
            throw new Exception("已完成的出库单不能编辑");

        if (dto.Items is null || dto.Items.Count == 0)
            throw new Exception("出库明细不能为空");

        // 更新出库单时不更新库存，只保存记录
        // 库存将在点击"完成"按钮时更新

        // 清除旧明细
        outbound.Items.Clear();

        // 更新出库单基本信息
        outbound.OutboundNumber = dto.OutboundNumber;
        outbound.OutboundDate = dto.OutboundDate;
        outbound.ProjectName = dto.ProjectName;
        outbound.ProjectCode = dto.ProjectCode;
        outbound.ProjectManager = dto.ProjectManager;
        outbound.Recipient = dto.Recipient;
        outbound.OutboundType = dto.OutboundType;
        outbound.ProjectTime = dto.ProjectTime;
        outbound.ContactPhone = dto.ContactPhone;
        outbound.UsageLocation = dto.UsageLocation;
        outbound.ReturnDate = dto.ReturnDate;
        outbound.Handler = dto.Handler;
        outbound.WarehouseKeeper = dto.WarehouseKeeper;
        // 处理图片列表
        if (dto.OutboundImages is not null && dto.OutboundImages.Count > 0)
        {
            outbound.OutboundImages = System.Text.Json.JsonSerializer.Serialize(dto.OutboundImages);
        }
        else
        {
            outbound.OutboundImages = null;
        }
        outbound.Remark = dto.Remark;
        outbound.UpdatedAt = DateTime.Now;

        // 添加新明细（不更新库存）
        foreach (var itemDto in dto.Items)
        {
            var item = new ProjectOutboundItem
            {
                OutboundId = outbound.Id,
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
                CreatedAt = DateTime.Now
            };
            outbound.Items.Add(item);
        }

        await outboundRepository.UpdateAsync(outbound);
        
        // 清除相关缓存
        await InvalidateCacheAsync(BuildCacheKey("projectoutbound", id));
        await InvalidateCacheAsync(BuildCacheKey("projectoutbounds", "all"));
        await InvalidateCacheAsync(BuildCacheKey("availableitems", "all"));

        var resultDto = mapper.Map<ProjectOutboundDto>(outbound);
        resultDto.TotalQuantity = outbound.Items.Sum(i => i.Quantity);
        return resultDto;
    }

    public async Task<IEnumerable<ProjectOutboundDto>> GetSummaryAsync()
    {
        return await GetAllAsync();
    }

    public async Task CompleteAsync(int id)
    {
        Console.WriteLine($"[CompleteAsync] 开始完成出库单: {id}");
        
        try
        {
            var outbound = await outboundRepository.GetByIdAsync(id);
            if (outbound == null)
                throw new Exception("项目出库单不存在");

            Console.WriteLine($"[CompleteAsync] 出库单找到: {id}, IsCompleted={outbound.IsCompleted}, Items.Count={outbound.Items?.Count ?? 0}");

            if (outbound.IsCompleted)
                throw new Exception("出库单已完成，不能重复完成");

            if (outbound.Items is null || outbound.Items.Count == 0)
                throw new Exception("出库明细为空，无法完成");

            Console.WriteLine($"[CompleteAsync] 开始处理 {outbound.Items.Count} 个物品");
            
            // 验证并更新库存
            foreach (var item in outbound.Items)
            {
                Console.WriteLine($"[CompleteAsync] 处理物品: ItemType={item.ItemType}, ItemType值={(int)item.ItemType}, ItemId={item.ItemId}, ItemName={item.ItemName}, Quantity={item.Quantity}");
                await ValidateAndUpdateStockAsync(new CreateProjectOutboundItemDto
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
                }, outbound.ProjectName, outbound.ProjectTime);
                Console.WriteLine($"[CompleteAsync] 物品处理完成: ItemId={item.ItemId}");
            }

            // 更新出库单状态为已完成
            outbound.IsCompleted = true;
            outbound.CompletedAt = DateTime.Now;
            outbound.UpdatedAt = DateTime.Now;

            Console.WriteLine($"[CompleteAsync] 准备更新出库单状态");
            await outboundRepository.UpdateAsync(outbound);
            Console.WriteLine($"[CompleteAsync] 出库单状态已更新为完成: {id}");
            
            // 清除相关缓存
            await InvalidateCacheAsync(BuildCacheKey("projectoutbound", id));
            await InvalidateCacheAsync(BuildCacheKey("projectoutbounds", "all"));
            await InvalidateCacheAsync(BuildCacheKey("availableitems", "all"));
            Console.WriteLine($"[CompleteAsync] 缓存已清除");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] CompleteAsync 失败: {ex.Message}");
            Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[ERROR] InnerException: {ex.InnerException.Message}");
                Console.WriteLine($"[ERROR] InnerException StackTrace: {ex.InnerException.StackTrace}");
            }
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            var outbound = await outboundRepository.GetByIdAsync(id);
            if (outbound == null)
                throw new Exception("项目出库单不存在");

            Console.WriteLine($"[DeleteAsync] 开始删除出库单 {id}, 包含 {outbound.Items?.Count ?? 0} 个物品, 已完成: {outbound.IsCompleted}");

            // 直接删除出库单，不恢复库存
            // 库存恢复逻辑暂时注释掉，以解决删除失败的问题
            /*
            // 只有已完成的出库单才需要恢复库存
            if (outbound.IsCompleted && outbound.Items is not null && outbound.Items.Count > 0)
            {
                // 使用字典去重，避免同一设备被恢复多次库存
                var uniqueItems = outbound.Items
                    .GroupBy(i => new { i.ItemType, i.ItemId })
                    .Select(g => new ProjectOutboundItem
                    {
                        ItemType = g.Key.ItemType,
                        ItemId = g.Key.ItemId,
                        Quantity = g.Sum(i => i.Quantity)
                    })
                    .ToList();

                Console.WriteLine($"[DeleteAsync] 去重后剩余 {uniqueItems.Count} 个唯一物品");

                foreach (var item in uniqueItems)
                {
                    try
                    {
                        await RestoreStockAsync(item);
                        Console.WriteLine($"[DeleteAsync] 恢复库存成功: ItemType={item.ItemType}, ItemId={item.ItemId}, Quantity={item.Quantity}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DeleteAsync] 恢复库存失败: ItemType={item.ItemType}, ItemId={item.ItemId}, 错误: {ex.Message}");
                        // 继续处理其他物品，不中断删除流程
                    }
                }
            }
            else if (!outbound.IsCompleted)
            {
                Console.WriteLine($"[DeleteAsync] 出库单未完成，不需要恢复库存");
            }
            */

            await outboundRepository.DeleteAsync(id);
            Console.WriteLine($"[DeleteAsync] 出库单 {id} 删除成功");
            
            // 清除相关缓存
            await InvalidateCacheAsync(BuildCacheKey("projectoutbound", id));
            await InvalidateCacheAsync(BuildCacheKey("projectoutbounds", "all"));
            await InvalidateCacheAsync(BuildCacheKey("availableitems", "all"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DeleteAsync] 删除出库单 {id} 失败: {ex.Message}");
            Console.WriteLine($"[DeleteAsync] 堆栈跟踪: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string number)
    {
        return await outboundRepository.ExistsAsync(number);
    }

    public async Task<IEnumerable<ProjectOutboundDto>> SearchOutboundsAsync(string keyword)
    {
        // 只获取出库单基本信息，不加载Items
        var outbounds = await outboundRepository.SearchAsync(keyword);
        var dtos = mapper.Map<List<ProjectOutboundDto>>(outbounds ?? new List<ProjectOutbound>());
        
        // 批量获取所有入库单，用于检查出库单的入库状态
        var allInbounds = await inboundRepository.GetAllAsync();
        
        // 构建出库单ID到入库状态的映射，优化查询性能
        var outboundStatusMap = new Dictionary<int, string>();
        foreach (var dto in dtos)
        {
            var hasCompletedInbound = allInbounds.Any(inbound => 
                inbound.ProjectInboundOutbounds.Any(pi => pi.ProjectOutboundId == dto.Id) && inbound.IsCompleted);
            var hasPartialInbound = allInbounds.Any(inbound => 
                inbound.ProjectInboundOutbounds.Any(pi => pi.ProjectOutboundId == dto.Id) && !inbound.IsCompleted && inbound.Status == "部分入库");
            
            if (hasCompletedInbound)
            {
                outboundStatusMap[dto.Id] = "入库完成";
            }
            else if (hasPartialInbound)
            {
                outboundStatusMap[dto.Id] = "部分入库";
            }
            else
            {
                outboundStatusMap[dto.Id] = "未入库";
            }
        }
        
        foreach (var dto in dtos)
        {
            var outbound = outbounds?.FirstOrDefault(o => o.Id == dto.Id);
            if (outbound != null && outbound.Items != null)
            {
                dto.TotalQuantity = outbound.Items.Sum(i => i.Quantity);
            }
            else
            {
                dto.TotalQuantity = 0;
            }
            
            // 从映射中获取入库状态
            if (outboundStatusMap.TryGetValue(dto.Id, out var status))
            {
                dto.InboundStatus = status;
            }
            else
            {
                dto.InboundStatus = "未入库";
            }
        }
        
        return dtos;
    }

    /// <summary>
    /// 生成出库单号，格式：XMCK + 年月日 + 4位序号
    /// 例如：XMCK202503080001
    /// </summary>
    private async Task<string> GenerateOutboundNumberAsync()
    {
        var dateStr = DateTime.Now.ToString("yyyyMMdd");
        var prefix = $"XMCK{dateStr}";
        
        // 获取当天最大的出库单号
        var allOutbounds = await outboundRepository.GetAllAsync();
        var todayNumbers = allOutbounds
            .Where(o => o.OutboundNumber.StartsWith(prefix))
            .Select(o => o.OutboundNumber)
            .ToList();
        
        int sequence = 1;
        if (todayNumbers.Count > 0)
        {
            var maxSequence = todayNumbers
                .Select(n => 
                {
                    var seqStr = n[prefix.Length..];
                    return int.TryParse(seqStr, out var seq) ? seq : 0;
                })
                .DefaultIfEmpty(0)
                .Max();
            sequence = maxSequence + 1;
        }
        
        return $"{prefix}{sequence:D4}";
    }

    private async Task ValidateAndUpdateStockAsync(CreateProjectOutboundItemDto itemDto, string projectName, string? projectTime)
    {
        Console.WriteLine($"[ValidateAndUpdateStockAsync] 开始处理: ItemType={itemDto.ItemType}, ItemId={itemDto.ItemId}, ItemName={itemDto.ItemName}, Quantity={itemDto.Quantity}");
        
        // 根据物品类型验证库存
        var itemTypeInt = (int)itemDto.ItemType;
        Console.WriteLine($"[ValidateAndUpdateStockAsync] ItemType转换为int: {itemTypeInt}");
        
        switch (itemTypeInt)
         {
             case 1: // SpecialEquipment
                Console.WriteLine($"[ValidateAndUpdateStockAsync] 处理专用设备: ItemId={itemDto.ItemId}");
                try
                {
                    var specialEquipment = await specialEquipmentRepository.GetByIdAsync(itemDto.ItemId);
                    if (specialEquipment == null)
                    {
                        Console.WriteLine($"[ValidateAndUpdateStockAsync] 专用设备不存在: {itemDto.ItemName}, ItemId={itemDto.ItemId}");
                        break; // 跳过不存在的设备
                    }
                    
                    // 专用设备只更新使用状态，不减少数量
                    specialEquipment.UseStatus = UsageStatus.InUse;
                    // 更新项目信息
                    specialEquipment.ProjectName = projectName;
                    specialEquipment.ProjectTime = projectTime;
                    // 清除导航属性，避免 EF Core 尝试更新它们
                    specialEquipment.Inventory = null;
                    // 不要将 Images 设置为 null，因为它是一个非可空的集合类型
                    // 清空集合而不是设置为 null
                    if (specialEquipment.Images != null)
                    {
                        specialEquipment.Images.Clear();
                    }
                    Console.WriteLine($"[ValidateAndUpdateStockAsync] 专用设备使用状态更新为: 使用中，项目名称: {projectName}，项目时间: {projectTime}");
                    await specialEquipmentRepository.UpdateAsync(specialEquipment);
                    Console.WriteLine($"[ValidateAndUpdateStockAsync] 专用设备已更新");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] 更新专用设备失败: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[ERROR] InnerException: {ex.InnerException.Message}");
                    }
                    // 继续处理其他物品，不中断流程
                }
                break;

            case 2: // GeneralEquipment
                Console.WriteLine($"[ValidateAndUpdateStockAsync] 处理通用设备: ItemId={itemDto.ItemId}");
                try
                {
                    var generalEquipment = await generalEquipmentRepository.GetByIdAsync(itemDto.ItemId);
                    if (generalEquipment == null)
                    {
                        Console.WriteLine($"[ValidateAndUpdateStockAsync] 通用设备不存在: {itemDto.ItemName}, ItemId={itemDto.ItemId}");
                        break; // 跳过不存在的设备
                    }
                    
                    // 通用设备只更新使用状态，不减少数量
                    generalEquipment.UseStatus = UsageStatus.InUse;
                    // 更新项目信息
                    generalEquipment.ProjectName = projectName;
                    generalEquipment.ProjectTime = projectTime;
                    // 清除 Inventory 导航属性，避免 EF Core 尝试更新它
                    generalEquipment.Inventory = null;
                    Console.WriteLine($"[ValidateAndUpdateStockAsync] 通用设备使用状态更新为: 使用中，项目名称: {projectName}，项目时间: {projectTime}");
                    await generalEquipmentRepository.UpdateAsync(generalEquipment);
                    Console.WriteLine($"[ValidateAndUpdateStockAsync] 通用设备已更新");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] 更新通用设备失败: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[ERROR] InnerException: {ex.InnerException.Message}");
                    }
                    // 继续处理其他物品，不中断流程
                }
                break;

            case 3: // Consumable
                Console.WriteLine($"[ValidateAndUpdateStockAsync] 处理耗材: ItemId={itemDto.ItemId}");
                try
                {
                    var consumable = await consumableRepository.GetByIdAsync(itemDto.ItemId);
                    if (consumable == null)
                    {
                        Console.WriteLine($"[ValidateAndUpdateStockAsync] 耗材不存在: {itemDto.ItemName}, ItemId={itemDto.ItemId}");
                        break; // 跳过不存在的耗材
                    }
                    Console.WriteLine($"[ValidateAndUpdateStockAsync] 耗材当前剩余数量: {consumable.RemainingQuantity}, 需要使用: {itemDto.Quantity}");
                    if (consumable.RemainingQuantity < itemDto.Quantity)
                    {
                        Console.WriteLine($"[ValidateAndUpdateStockAsync] 耗材库存不足: {itemDto.ItemName}, 可用: {consumable.RemainingQuantity}, 需要: {itemDto.Quantity}");
                        break; // 跳过库存不足的耗材
                    }
                    
                    consumable.UsedQuantity += itemDto.Quantity;
                    consumable.RemainingQuantity = consumable.TotalQuantity - consumable.UsedQuantity;
                    await consumableRepository.UpdateAsync(consumable);
                    Console.WriteLine($"[ValidateAndUpdateStockAsync] 耗材库存已更新，剩余: {consumable.RemainingQuantity}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] 更新耗材失败: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[ERROR] InnerException: {ex.InnerException.Message}");
                    }
                    // 继续处理其他物品，不中断流程
                }
                break;

            case 4: // RawMaterial
                Console.WriteLine($"[ValidateAndUpdateStockAsync] 处理原材料: ItemId={itemDto.ItemId}");
                try
                {
                    var rawMaterial = await rawMaterialRepository.GetByIdAsync(itemDto.ItemId);
                    if (rawMaterial == null)
                    {
                        Console.WriteLine($"[ValidateAndUpdateStockAsync] 原材料不存在: {itemDto.ItemName}, ItemId={itemDto.ItemId}");
                        break; // 跳过不存在的原材料
                    }
                    Console.WriteLine($"[ValidateAndUpdateStockAsync] 原材料当前剩余数量: {rawMaterial.RemainingQuantity}, 需要使用: {itemDto.Quantity}");
                    if (rawMaterial.RemainingQuantity < itemDto.Quantity)
                    {
                        Console.WriteLine($"[ValidateAndUpdateStockAsync] 原材料库存不足: {itemDto.ItemName}, 可用: {rawMaterial.RemainingQuantity}, 需要: {itemDto.Quantity}");
                        break; // 跳过库存不足的原材料
                    }
                    
                    rawMaterial.UsedQuantity += itemDto.Quantity;
                    rawMaterial.RemainingQuantity = rawMaterial.TotalQuantity - rawMaterial.UsedQuantity;
                    await rawMaterialRepository.UpdateAsync(rawMaterial);
                    Console.WriteLine($"[ValidateAndUpdateStockAsync] 原材料库存已更新，剩余: {rawMaterial.RemainingQuantity}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] 更新原材料失败: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[ERROR] InnerException: {ex.InnerException.Message}");
                    }
                    // 继续处理其他物品，不中断流程
                }
                break;

            default:
                Console.WriteLine($"[ValidateAndUpdateStockAsync] 未知的物品类型: {itemDto.ItemType}");
                break; // 跳过未知类型
        }
        
        Console.WriteLine($"[ValidateAndUpdateStockAsync] 处理完成: ItemType={itemDto.ItemType}, ItemId={itemDto.ItemId}");
    }

    private async Task RestoreStockAsync(ProjectOutboundItem item)
    {
        // 根据物品类型恢复库存
        switch (item.ItemType)
        {
            case ItemType.SpecialEquipment:
                var specialEquipment = await specialEquipmentRepository.GetByIdAsync(item.ItemId);
                if (specialEquipment != null)
                {
                    specialEquipment.Quantity += item.Quantity;
                    // 确保库存不为负数
                    if (specialEquipment.Quantity < 0)
                        specialEquipment.Quantity = 0;
                    await specialEquipmentRepository.UpdateAsync(specialEquipment);
                    
                    // 更新库存表
                    var specialInventory = await inventoryRepository.GetBySpecialEquipmentIdAsync(item.ItemId);
                    if (specialInventory != null)
                    {
                        specialInventory.CurrentQuantity = specialEquipment.Quantity;
                        specialInventory.LastUpdated = DateTime.Now;
                        await inventoryRepository.UpdateAsync(specialInventory);
                    }
                }
                break;

            case ItemType.GeneralEquipment:
                var generalEquipment = await generalEquipmentRepository.GetByIdAsync(item.ItemId);
                if (generalEquipment != null)
                {
                    generalEquipment.Quantity += item.Quantity;
                    // 确保库存不为负数
                    if (generalEquipment.Quantity < 0)
                        generalEquipment.Quantity = 0;
                    await generalEquipmentRepository.UpdateAsync(generalEquipment);
                    
                    // 更新库存表
                    var generalInventory = await inventoryRepository.GetByGeneralEquipmentIdAsync(item.ItemId);
                    if (generalInventory != null)
                    {
                        generalInventory.CurrentQuantity = generalEquipment.Quantity;
                        generalInventory.LastUpdated = DateTime.Now;
                        await inventoryRepository.UpdateAsync(generalInventory);
                    }
                }
                break;

            case ItemType.Consumable:
                var consumable = await consumableRepository.GetByIdAsync(item.ItemId);
                if (consumable != null)
                {
                    consumable.UsedQuantity -= item.Quantity;
                    // 确保UsedQuantity不为负数
                    if (consumable.UsedQuantity < 0)
                        consumable.UsedQuantity = 0;
                    consumable.RemainingQuantity = consumable.TotalQuantity - consumable.UsedQuantity;
                    // 确保RemainingQuantity不为负数
                    if (consumable.RemainingQuantity < 0)
                        consumable.RemainingQuantity = 0;
                    await consumableRepository.UpdateAsync(consumable);
                }
                break;
        }
    }
}
