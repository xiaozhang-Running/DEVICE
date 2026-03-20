using AutoMapper;
using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;

using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Enums;
using DeviceWarehouse.Domain.Interfaces;

namespace DeviceWarehouse.Application.Services;

public class GeneralEquipmentService(IGeneralEquipmentRepository generalEquipmentRepository, IInventoryRepository inventoryRepository, IMapper mapper, ICacheService cacheService) : EquipmentServiceBase<GeneralEquipment, GeneralEquipmentDto, CreateGeneralEquipmentDto, UpdateGeneralEquipmentDto, GeneralEquipmentSummaryDto>(inventoryRepository, mapper, cacheService), IGeneralEquipmentService
{
    private const string CacheKeyPrefix = "general_equipment";
    private readonly IGeneralEquipmentRepository _generalEquipmentRepository = generalEquipmentRepository;

    public override async Task<GeneralEquipmentDto> GetByIdAsync(int id)
    {
        var cacheKey = $"{CacheKeyPrefix}:id:{id}";
        
        // 尝试从缓存获取
        var cachedData = await _cacheService.GetAsync<GeneralEquipmentDto>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }
        
        // 从数据库获取
        var equipment = await _generalEquipmentRepository.GetByIdAsync(id);
        if (equipment is null)
            throw new Exception("设备不存在");
        
        // 确保 Images 属性不为 null
        equipment.Images ??= [];
        
        // 手动创建 GeneralEquipmentDto 对象，避免 AutoMapper 尝试映射 Images 属性
        var dto = new GeneralEquipmentDto
        {
            Id = equipment.Id,
            SortOrder = equipment.SortOrder,
            DeviceType = equipment.DeviceType,
            DeviceName = equipment.DeviceName,
            DeviceCode = equipment.DeviceCode,
            Brand = equipment.Brand,
            Model = equipment.Model,
            SerialNumber = equipment.SerialNumber,
            Quantity = equipment.Quantity,
            Unit = equipment.Unit,
            DeviceStatus = equipment.DeviceStatus,
            UseStatus = equipment.UseStatus,
            Location = equipment.Location,
            ProjectName = equipment.ProjectName,
            ProjectTime = equipment.ProjectTime,
            Company = equipment.Company,
            Accessories = equipment.Accessories,
            Remark = equipment.Remark,
            RepairStatus = equipment.RepairStatus,
            RepairPerson = equipment.RepairPerson,
            RepairDate = equipment.RepairDate,
            FaultReason = equipment.FaultReason,
            CreatedAt = equipment.CreatedAt,
            // 初始化 Images 数组
            Images = []
        };
        
        // 设置主图片 URL（使用第一张图片）
        if (equipment.Images != null && equipment.Images.Count > 0)
        {
            var firstImage = equipment.Images.FirstOrDefault();
            if (firstImage != null)
            {
                dto.ImageUrl = firstImage.Url;
            }
        }
        
        // 手动映射 Image 实体到 ImageDto
        if (equipment.Images != null && equipment.Images.Count > 0)
        {
            foreach (var image in equipment.Images)
            {
                var imageDto = new ImageDto
                {
                    Name = image.Name,
                    Type = image.Type,
                    Url = image.Url,
                    ImageData = image.Data,
                    ImageContentType = image.Type,
                    ImageUrl = image.Url
                };
                dto.Images.Add(imageDto);
            }
        }
        
        // 处理图片数据，将ImageData转换为base64字符串
        if (dto.Images != null && dto.Images.Count > 0)
        {
            foreach (var imageDto in dto.Images)
            {
                // 如果ImageData存在，转换为base64字符串
                if (imageDto.ImageData != null)
                {
                    var base64ImageData = System.Convert.ToBase64String(imageDto.ImageData);
                    imageDto.ImageDataBase64 = base64ImageData;
                    // 如果没有ImageUrl，创建一个
                    if (string.IsNullOrWhiteSpace(imageDto.ImageUrl) && !string.IsNullOrWhiteSpace(imageDto.ImageContentType))
                    {
                        imageDto.ImageUrl = $"data:{imageDto.ImageContentType};base64,{base64ImageData}";
                    }
                }
            }
        }
        
        // 缓存结果，设置过期时间为10分钟
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10));
        
        return dto;
    }

    public override async Task<IEnumerable<GeneralEquipmentDto>> GetAllAsync()
    {
        var cacheKey = $"{CacheKeyPrefix}:all";
        
        // 尝试从缓存获取
        var cachedData = await _cacheService.GetAsync<IEnumerable<GeneralEquipmentDto>>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }
        
        // 从数据库获取
        var equipments = await _generalEquipmentRepository.GetAllAsync();
        
        // 手动创建 GeneralEquipmentDto 对象，避免 AutoMapper 尝试映射 Images 属性
        var dtos = equipments.Select(item => {
            // 确保 Images 属性不为 null
            item.Images ??= [];
            
            // 创建 GeneralEquipmentDto 对象
            var dto = new GeneralEquipmentDto
            {
                Id = item.Id,
                SortOrder = item.SortOrder,
                DeviceType = item.DeviceType,
                DeviceName = item.DeviceName,
                DeviceCode = item.DeviceCode,
                Brand = item.Brand,
                Model = item.Model,
                SerialNumber = item.SerialNumber,
                Quantity = item.Quantity,
                Unit = item.Unit,
                DeviceStatus = item.DeviceStatus,
                UseStatus = item.UseStatus,
                Location = item.Location,
                ProjectName = item.ProjectName,
                ProjectTime = item.ProjectTime,
                Company = item.Company,
                Accessories = item.Accessories,
                Remark = item.Remark,
                RepairStatus = item.RepairStatus,
                RepairPerson = item.RepairPerson,
                RepairDate = item.RepairDate,
                FaultReason = item.FaultReason,
                CreatedAt = item.CreatedAt,
                // 初始化 Images 数组
                Images = new List<ImageDto>()
            };
            
            // 设置主图片 URL（使用第一张图片）
            if (item.Images != null && item.Images.Count > 0)
            {
                var firstImage = item.Images.FirstOrDefault();
                if (firstImage != null)
                {
                    dto.ImageUrl = firstImage.Url;
                }
            }
            
            // 手动映射 Image 实体到 ImageDto
            if (item.Images != null && item.Images.Count > 0)
            {
                foreach (var image in item.Images)
                {
                    var imageDto = new ImageDto
                    {
                        Name = image.Name,
                        Type = image.Type,
                        Url = image.Url,
                        ImageData = image.Data,
                        ImageContentType = image.Type,
                        ImageUrl = image.Url
                    };
                    dto.Images.Add(imageDto);
                }
            }
            
            return dto;
        }).ToList();
        
        // 处理图片数据，将ImageData转换为base64字符串
        foreach (var dto in dtos)
        {
            if (dto.Images != null && dto.Images.Count > 0)
            {
                foreach (var imageDto in dto.Images)
                {
                    // 如果ImageData存在，转换为base64字符串
                    if (imageDto.ImageData != null)
                    {
                        var base64ImageData = System.Convert.ToBase64String(imageDto.ImageData);
                        imageDto.ImageDataBase64 = base64ImageData;
                        // 如果没有ImageUrl，创建一个
                        if (string.IsNullOrWhiteSpace(imageDto.ImageUrl) && !string.IsNullOrWhiteSpace(imageDto.ImageContentType))
                        {
                            imageDto.ImageUrl = $"data:{imageDto.ImageContentType};base64,{base64ImageData}";
                        }
                    }
                }
            }
        }
        
        // 缓存结果，设置过期时间为5分钟
        await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(5));
        
        return dtos;
    }

    public override async Task<IEnumerable<GeneralEquipmentDto>> GetByTypeAsync(DeviceType type)
    {
        var cacheKey = $"{CacheKeyPrefix}:type:{type}";
        
        // 尝试从缓存获取
        var cachedData = await _cacheService.GetAsync<IEnumerable<GeneralEquipmentDto>>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }
        
        // 从数据库获取
        var equipments = await _generalEquipmentRepository.GetByTypeAsync(type);
        
        // 手动创建 GeneralEquipmentDto 对象，避免 AutoMapper 尝试映射 Images 属性
        var dtos = equipments.Select(item => {
            // 确保 Images 属性不为 null
            item.Images ??= [];
            
            // 创建 GeneralEquipmentDto 对象
            var dto = new GeneralEquipmentDto
            {
                Id = item.Id,
                SortOrder = item.SortOrder,
                DeviceType = item.DeviceType,
                DeviceName = item.DeviceName,
                DeviceCode = item.DeviceCode,
                Brand = item.Brand,
                Model = item.Model,
                SerialNumber = item.SerialNumber,
                Quantity = item.Quantity,
                Unit = item.Unit,
                DeviceStatus = item.DeviceStatus,
                UseStatus = item.UseStatus,
                Location = item.Location,
                ProjectName = item.ProjectName,
                ProjectTime = item.ProjectTime,
                Company = item.Company,
                Accessories = item.Accessories,
                Remark = item.Remark,
                RepairStatus = item.RepairStatus,
                RepairPerson = item.RepairPerson,
                RepairDate = item.RepairDate,
                FaultReason = item.FaultReason,
                CreatedAt = item.CreatedAt,
                // 初始化 Images 数组
                Images = new List<ImageDto>()
            };
            
            // 设置主图片 URL（使用第一张图片）
            if (item.Images != null && item.Images.Count > 0)
            {
                var firstImage = item.Images.FirstOrDefault();
                if (firstImage != null)
                {
                    dto.ImageUrl = firstImage.Url;
                }
            }
            
            // 手动映射 Image 实体到 ImageDto
            if (item.Images != null && item.Images.Count > 0)
            {
                foreach (var image in item.Images)
                {
                    var imageDto = new ImageDto
                    {
                        Name = image.Name,
                        Type = image.Type,
                        Url = image.Url,
                        ImageData = image.Data,
                        ImageContentType = image.Type,
                        ImageUrl = image.Url
                    };
                    dto.Images.Add(imageDto);
                }
            }
            
            return dto;
        }).ToList();
        
        // 处理图片数据，将ImageData转换为base64字符串
        foreach (var dto in dtos)
        {
            if (dto.Images != null && dto.Images.Count > 0)
            {
                foreach (var imageDto in dto.Images)
                {
                    // 如果ImageData存在，转换为base64字符串
                    if (imageDto.ImageData != null)
                    {
                        var base64ImageData = System.Convert.ToBase64String(imageDto.ImageData);
                        imageDto.ImageDataBase64 = base64ImageData;
                        // 如果没有ImageUrl，创建一个
                        if (string.IsNullOrWhiteSpace(imageDto.ImageUrl) && !string.IsNullOrWhiteSpace(imageDto.ImageContentType))
                        {
                            imageDto.ImageUrl = $"data:{imageDto.ImageContentType};base64,{base64ImageData}";
                        }
                    }
                }
            }
        }
        
        // 缓存结果，设置过期时间为8分钟
        await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(8));
        
        return dtos;
    }

    public override async Task<IEnumerable<GeneralEquipmentSummaryDto>> GetEquipmentSummaryAsync(DeviceType? type)
    {
        var equipments = type.HasValue
            ? await _generalEquipmentRepository.GetByTypeAsync(type.Value)
            : await _generalEquipmentRepository.GetAllAsync();

        var summary = equipments
            .GroupBy(d => new { d.DeviceName, d.Brand, d.Model })
            .Select(g => new GeneralEquipmentSummaryDto
            {
                DeviceName = g.Key.DeviceName,
                Brand = g.Key.Brand,
                Model = g.Key.Model,
                Count = g.Sum(d => d.Quantity),
                Unit = g.First().Unit,
                DeviceType = g.First().DeviceType
            })
            .OrderByDescending(s => s.Count)
            .ToList();

        return summary;
    }

    public override async Task<GeneralEquipmentDto> CreateAsync(CreateGeneralEquipmentDto dto)
    {
        if (await _generalEquipmentRepository.ExistsAsync(dto.DeviceCode))
            throw new Exception("设备编号已存在");

        var equipment = _mapper.Map<GeneralEquipment>(dto);
        equipment.DeviceStatus = DeviceStatus.Normal;
        equipment.UseStatus = UsageStatus.Unused;
        
        // 计算设备名称下的序号
        var existingCount = await _generalEquipmentRepository.CountByNameAsync(dto.DeviceName);
        equipment.NameSequence = existingCount + 1;

        // 处理图片数据
        if (dto.Images != null && dto.Images.Count > 0)
        {
            // 处理并保存所有图片
            foreach (var imageDto in dto.Images)
            {
                if (!string.IsNullOrEmpty(imageDto.Url))
                {
                    try
                    {
                        // 提取base64部分并转换为字节数组
                        var base64Data = imageDto.Url.Split(',')[1];
                        var imageData = Convert.FromBase64String(base64Data);
                        
                        // 创建新的Image实体
                        var image = new Image
                        {
                            Name = imageDto.Name ?? "Image",
                            Type = imageDto.Type ?? "image/png",
                            Data = imageData,
                            Url = imageDto.Url,
                            CreatedAt = DateTime.Now
                        };
                        
                        equipment.Images.Add(image);
                    }
                    catch (Exception ex)
                    {
                        // 图片数据转换失败，记录错误但不影响创建过程
                        Console.WriteLine($"图片数据转换失败: {ex.Message}");
                    }
                }
            }
            
            // 使用第一张图片作为主图片（保持向后兼容）
            if (dto.Images.Count > 0)
            {
                var firstImage = dto.Images[0];
                if (!string.IsNullOrEmpty(firstImage.Url))
                {
                    // 不再设置 ImageUrl 字段，使用 Images 集合
                }
            }
        }

        var createdEquipment = await _generalEquipmentRepository.AddAsync(equipment);

        // 使用基类方法创建库存
        await CreateInventoryAsync(createdEquipment.Id, dto.Quantity, false);

        // 清除缓存
        await _cacheService.RemoveAsync("general_equipment:all");
        await _cacheService.RemoveAsync($"general_equipment:type:{equipment.DeviceType}");
        await _cacheService.RemoveAsync($"general_equipment:id:{createdEquipment.Id}");

        return _mapper.Map<GeneralEquipmentDto>(createdEquipment);
    }

    public override async Task<GeneralEquipmentDto> UpdateAsync(int id, UpdateGeneralEquipmentDto dto)
    {
        try
        {
            Console.WriteLine($"开始更新通用设备，ID: {id}");
            
            if (id <= 0)
                throw new Exception("设备ID必须大于0");
            if (dto is null)
                throw new Exception("设备信息不能为空");
            if (string.IsNullOrWhiteSpace(dto.DeviceName))
                throw new Exception("设备名称不能为空");
            if (!dto.Quantity.HasValue || dto.Quantity.Value <= 0)
                throw new Exception("设备数量必须大于0");
            if (dto.DeviceCode != null && string.IsNullOrWhiteSpace(dto.DeviceCode))
                throw new Exception("设备编号不能为空");

            Console.WriteLine("获取设备信息...");
            var equipment = await _generalEquipmentRepository.GetByIdAsync(id);
            if (equipment is null)
                throw new Exception("设备不存在");
            Console.WriteLine($"设备信息获取成功，设备名称: {equipment.DeviceName}");

            // 处理图片数据 - 暂时不处理，避免EF Core的跟踪问题
            // 图片的处理将在后续版本中通过单独的API实现
            
            // 先处理库存关联，再更新设备
            try
            {
                Console.WriteLine("处理库存关联...");
                if (dto.DeviceType.HasValue)
                {
                    // 获取当前设备类型
                    var currentDeviceType = equipment.DeviceType;
                    // 获取新设备类型
                    var newDeviceType = dto.DeviceType.Value;
                    
                    // 如果设备类型从通用设备改为非通用设备，需要先删除库存关联
                    if (currentDeviceType == DeviceType.GeneralDevice && newDeviceType != DeviceType.GeneralDevice)
                    {
                        // 删除库存关联
                        var inventory = await _inventoryRepository.GetByGeneralEquipmentIdAsync(id);
                        if (inventory != null)
                        {
                            await _inventoryRepository.DeleteAsync(inventory.Id);
                            Console.WriteLine("库存关联已删除");
                        }
                    }
                    // 如果设备类型从非通用设备改为通用设备，需要先创建库存关联
                    else if (currentDeviceType != DeviceType.GeneralDevice && newDeviceType == DeviceType.GeneralDevice)
                    {
                        // 创建库存关联
                        await CreateInventoryAsync(id, dto.Quantity.HasValue ? dto.Quantity.Value : equipment.Quantity, false);
                        Console.WriteLine("库存关联已创建");
                    }
                    // 如果设备类型保持为通用设备，需要更新库存数量
                    else if (currentDeviceType == DeviceType.GeneralDevice && newDeviceType == DeviceType.GeneralDevice)
                    {
                        // 更新库存数量
                        await UpdateInventoryAsync(id, dto.Quantity.HasValue ? dto.Quantity.Value : equipment.Quantity, false);
                        Console.WriteLine("库存数量已更新");
                    }
                }
                else if (equipment.DeviceType == DeviceType.GeneralDevice)
                {
                    // 如果设备类型保持为通用设备，且未指定新设备类型，需要更新库存数量
                    await UpdateInventoryAsync(id, dto.Quantity.HasValue ? dto.Quantity.Value : equipment.Quantity, false);
                    Console.WriteLine("库存数量已更新");
                }
            }
            catch (Exception ex)
            {
                // 记录错误但不影响主流程
                Console.WriteLine($"处理库存关联失败: {ex.Message}");
            }
            
            // 清除导航属性，避免EF Core跟踪冲突
            Console.WriteLine("清除导航属性...");
            equipment.Inventory = null;
            equipment.Images ??= [];
            
            // 设置更新时间和更新人
            equipment.UpdatedAt = DateTime.Now;
            equipment.UpdatedBy = "System";
            // 设置Status属性，避免空引用错误
            equipment.Status ??= "";
            
            // 手动映射基本属性，避免映射Images属性
            Console.WriteLine("映射设备属性...");
            equipment.SortOrder = dto.SortOrder.HasValue ? dto.SortOrder.Value : equipment.SortOrder;
            equipment.DeviceType = dto.DeviceType.HasValue ? dto.DeviceType.Value : equipment.DeviceType;
            equipment.DeviceName = !string.IsNullOrWhiteSpace(dto.DeviceName) ? dto.DeviceName : equipment.DeviceName;
            if (dto.DeviceCode != null && !string.IsNullOrWhiteSpace(dto.DeviceCode))
            {
                equipment.DeviceCode = dto.DeviceCode;
            }
            equipment.Brand = dto.Brand;
            equipment.Model = dto.Model;
            equipment.SerialNumber = dto.SerialNumber;
            equipment.Specification = dto.Specification;
            equipment.Quantity = dto.Quantity.HasValue ? dto.Quantity.Value : equipment.Quantity;
            equipment.Unit = dto.Unit;
            equipment.Location = dto.Location;
            equipment.ProjectName = dto.ProjectName;
            equipment.ProjectTime = dto.ProjectTime;
            equipment.Company = dto.Company;
            equipment.Accessories = dto.Accessories;
            equipment.Remark = dto.Remark;
            equipment.DeviceStatus = dto.DeviceStatus.HasValue ? dto.DeviceStatus.Value : equipment.DeviceStatus;
            equipment.UseStatus = dto.UseStatus.HasValue ? dto.UseStatus.Value : equipment.UseStatus;
            equipment.RepairStatus = dto.RepairStatus;
            equipment.RepairPerson = dto.RepairPerson;
            equipment.RepairDate = dto.RepairDate;
            equipment.FaultReason = dto.FaultReason;
            
            // 更新设备
            Console.WriteLine("更新设备...");
            await _generalEquipmentRepository.UpdateAsync(equipment);
            Console.WriteLine("设备更新成功");

            // 清除缓存
            try
            {
                Console.WriteLine("清除缓存...");
                await _cacheService.RemoveAsync($"{CacheKeyPrefix}:all");
                await _cacheService.RemoveAsync($"{CacheKeyPrefix}:type:{equipment.DeviceType}");
                await _cacheService.RemoveAsync($"{CacheKeyPrefix}:id:{id}");
                // 清除报表和分析缓存
                await _cacheService.RemoveAsync($"{CacheKeyPrefix}_inventory_summary_0_0");
                await _cacheService.RemoveAsync($"{CacheKeyPrefix}_usage_analysis_12");
                // 清除库存相关缓存
                await _cacheService.RemoveAsync("inventory:all");
                await _cacheService.RemoveAsync("inventory:all:1");
                await _cacheService.RemoveAsync("inventory:all:2");
                await _cacheService.RemoveAsync("inventory:all:3");
                await _cacheService.RemoveAsync("inventory:all:4");
                await _cacheService.RemoveAsync("inventory:lowstock");
                await _cacheService.RemoveAsync("inventory:lowstock:10");
                await _cacheService.RemoveAsync("inventory:zerostock");
                Console.WriteLine("缓存清除成功");
            }
            catch (Exception ex)
            {
                // 缓存清除失败，不影响主流程
                Console.WriteLine($"缓存清除失败: {ex.Message}");
            }

            // 映射返回结果
            Console.WriteLine("映射返回结果...");
            var result = _mapper.Map<GeneralEquipmentDto>(equipment);
            Console.WriteLine("更新完成");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"更新设备失败: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"内部异常: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    public async Task<IEnumerable<GeneralEquipmentSummaryDto>> GetSummaryAsync()
    {
        var equipments = await _generalEquipmentRepository.GetAllAsync();

        var summary = equipments
            .GroupBy(d => new { d.DeviceName, d.Brand, d.Model })
            .Select(g => new GeneralEquipmentSummaryDto
            {
                DeviceName = g.Key.DeviceName,
                Brand = g.Key.Brand,
                Model = g.Key.Model,
                Count = g.Sum(d => d.Quantity),
                Unit = g.First().Unit,
                DeviceType = g.First().DeviceType
            })
            .OrderByDescending(s => s.Count)
            .ToList();

        return summary;
    }

    public override async Task DeleteAsync(int id)
    {
        if (id <= 0)
            throw new Exception("设备ID必须大于0");

        var equipment = await _generalEquipmentRepository.GetByIdAsync(id);
        if (equipment is null)
            throw new Exception("设备不存在");

        var deletedEquipment = equipment;
        var deviceName = deletedEquipment.DeviceName;
        
        // 先删除关联的库存记录
        var inventory = await _inventoryRepository.GetByGeneralEquipmentIdAsync(id);
        if (inventory != null)
        {
            await _inventoryRepository.DeleteAsync(inventory.Id);
        }
        
        // 再删除设备
        await _generalEquipmentRepository.DeleteAsync(id);

        // 重新计算同名称设备的序号
        await RecalculateNameSequencesAsync(deviceName);

        // 清除缓存
        try
        {
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}:all");
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}:type:{deletedEquipment.DeviceType}");
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}:id:{id}");
            // 清除分页缓存
            await _cacheService.RemovePatternAsync($"{CacheKeyPrefix}_paged_*");
            await _cacheService.RemovePatternAsync($"{CacheKeyPrefix}_grouped_*");
            // 清除搜索缓存
            await _cacheService.RemovePatternAsync($"{CacheKeyPrefix}_search_*");
            // 清除报表和分析缓存
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}_inventory_summary_0_0");
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}_usage_analysis_12");
        }
        catch (Exception)
        {
            // 缓存清除失败，不影响主流程
        }
    }

    public override async Task<IEnumerable<GeneralEquipmentDto>> SearchAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return Enumerable.Empty<GeneralEquipmentDto>();

        var cacheKey = $"{CacheKeyPrefix}_search_{keyword.ToLower()}";
        
        // 尝试从缓存获取
        var cachedData = await _cacheService.GetAsync<IEnumerable<GeneralEquipmentDto>>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }
        
        // 从数据库获取
        var equipments = keyword.ToLower() == "all" 
            ? await _generalEquipmentRepository.GetAllAsync() 
            : await _generalEquipmentRepository.SearchAsync(keyword);
        
        // 手动创建 GeneralEquipmentDto 对象，避免 AutoMapper 尝试映射 Images 属性
        var dtos = equipments.Select(item => {
            // 确保 Images 属性不为 null
            item.Images ??= [];
            
            // 创建 GeneralEquipmentDto 对象
            var dto = new GeneralEquipmentDto
            {
                Id = item.Id,
                SortOrder = item.SortOrder,
                DeviceType = item.DeviceType,
                DeviceName = item.DeviceName,
                DeviceCode = item.DeviceCode,
                Brand = item.Brand,
                Model = item.Model,
                SerialNumber = item.SerialNumber,
                Quantity = item.Quantity,
                Unit = item.Unit,
                DeviceStatus = item.DeviceStatus,
                UseStatus = item.UseStatus,
                Location = item.Location,
                ProjectName = item.ProjectName,
                ProjectTime = item.ProjectTime,
                Company = item.Company,
                Accessories = item.Accessories,
                Remark = item.Remark,
                RepairStatus = item.RepairStatus,
                RepairPerson = item.RepairPerson,
                RepairDate = item.RepairDate,
                FaultReason = item.FaultReason,
                CreatedAt = item.CreatedAt,
                // 初始化 Images 数组
                Images = new List<ImageDto>()
            };
            
            // 设置主图片 URL（使用第一张图片）
            if (item.Images != null && item.Images.Count > 0)
            {
                var firstImage = item.Images.FirstOrDefault();
                if (firstImage != null)
                {
                    dto.ImageUrl = firstImage.Url;
                }
            }
            
            // 手动映射 Image 实体到 ImageDto
            if (item.Images != null && item.Images.Count > 0)
            {
                foreach (var image in item.Images)
                {
                    var imageDto = new ImageDto
                    {
                        Name = image.Name,
                        Type = image.Type,
                        Url = image.Url,
                        ImageData = image.Data,
                        ImageContentType = image.Type,
                        ImageUrl = image.Url
                    };
                    dto.Images.Add(imageDto);
                }
            }
            
            return dto;
        }).ToList();
        
        // 处理图片数据，将ImageData转换为base64字符串
        foreach (var dto in dtos)
        {
            if (dto.Images != null && dto.Images.Count > 0)
            {
                foreach (var imageDto in dto.Images)
                {
                    // 如果ImageData存在，转换为base64字符串
                    if (imageDto.ImageData != null)
                    {
                        var base64ImageData = System.Convert.ToBase64String(imageDto.ImageData);
                        imageDto.ImageDataBase64 = base64ImageData;
                        // 如果没有ImageUrl，创建一个
                        if (string.IsNullOrWhiteSpace(imageDto.ImageUrl) && !string.IsNullOrWhiteSpace(imageDto.ImageContentType))
                        {
                            imageDto.ImageUrl = $"data:{imageDto.ImageContentType};base64,{base64ImageData}";
                        }
                    }
                }
            }
        }
        
        // 缓存结果，设置过期时间为2分钟
        await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(2));
        
        return dtos;
    }

    public override async Task<object> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false, DeviceStatus? deviceStatus = null, UsageStatus? useStatus = null, string? brand = null)
    {
        if (pageNumber < 1)
            throw new Exception("页码必须大于0");
        if (pageSize < 1 || pageSize > 100)
            throw new Exception("每页大小必须在1-100之间");

        try {
            // 直接从数据库获取数据，不使用缓存，确保数据实时更新
            var (items, totalCount, pageNumberResult, pageSizeResult, totalPages) = await _generalEquipmentRepository.GetPagedAsync(pageNumber, pageSize, keyword, sortBy, sortDescending, deviceStatus, useStatus, brand);
            
            // 手动创建 GeneralEquipmentDto 对象，避免 AutoMapper 尝试映射 Images 属性
            var mappedItems = items.Select(item => {
                // 确保 Images 属性不为 null
                if (item.Images == null)
                {
                    item.Images = new List<Image>();
                }
                
                // 创建 GeneralEquipmentDto 对象
                var dto = new GeneralEquipmentDto
                {
                    Id = item.Id,
                    SortOrder = item.SortOrder,
                    DeviceType = item.DeviceType,
                    DeviceName = item.DeviceName,
                    DeviceCode = item.DeviceCode,
                    Brand = item.Brand,
                    Model = item.Model,
                    SerialNumber = item.SerialNumber,
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    DeviceStatus = item.DeviceStatus,
                    UseStatus = item.UseStatus,
                    Location = item.Location,
                    ProjectName = item.ProjectName,
                    ProjectTime = item.ProjectTime,
                    Company = item.Company,
                    Accessories = item.Accessories,
                    Remark = item.Remark,
                    RepairStatus = item.RepairStatus,
                    RepairPerson = item.RepairPerson,
                    RepairDate = item.RepairDate,
                    FaultReason = item.FaultReason,
                    CreatedAt = item.CreatedAt,
                    // 初始化 Images 数组
                    Images = new List<ImageDto>()
                };
                
                // 设置主图片 URL（使用第一张图片）
                if (item.Images != null && item.Images.Count > 0)
                {
                    var firstImage = item.Images.FirstOrDefault();
                    if (firstImage != null)
                    {
                        dto.ImageUrl = firstImage.Url;
                    }
                }
                
                // 手动映射 Image 实体到 ImageDto
                if (item.Images != null && item.Images.Count > 0)
                {
                    foreach (var image in item.Images)
                    {
                        var imageDto = new ImageDto
                        {
                            Name = image.Name,
                            Type = image.Type,
                            Url = image.Url,
                            ImageData = image.Data,
                            ImageContentType = image.Type,
                            ImageUrl = image.Url
                        };
                        dto.Images.Add(imageDto);
                    }
                }
                
                return dto;
            }).ToList();
            
            // 处理图片数据，将ImageData转换为base64字符串
            foreach (var dto in mappedItems)
            {
                if (dto.Images != null && dto.Images.Count > 0)
                {
                    foreach (var imageDto in dto.Images)
                    {
                        // 如果ImageData存在，转换为base64字符串
                        if (imageDto.ImageData != null)
                        {
                            var base64ImageData = System.Convert.ToBase64String(imageDto.ImageData);
                            imageDto.ImageDataBase64 = base64ImageData;
                            // 如果没有ImageUrl，创建一个
                            if (string.IsNullOrWhiteSpace(imageDto.ImageUrl) && !string.IsNullOrWhiteSpace(imageDto.ImageContentType))
                            {
                                imageDto.ImageUrl = $"data:{imageDto.ImageContentType};base64,{base64ImageData}";
                            }
                        }
                    }
                }
            }
            
            return new {
                items = mappedItems,
                totalCount,
                pageNumber = pageNumberResult,
                pageSize = pageSizeResult,
                totalPages
            };
        } catch (Exception ex) {
            // 记录错误并返回空结果
            Console.WriteLine($"GetPagedAsync error: {ex.Message}");
            if (ex.InnerException != null) {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            return new {
                items = Enumerable.Empty<GeneralEquipmentDto>(),
                totalCount = 0,
                pageNumber = pageNumber,
                pageSize = pageSize,
                totalPages = 0
            };
        }
    }

    public override async Task<object> GetGroupedPagedAsync(int pageNumber, int pageSize, string? keyword = null)
    {
        // 直接从数据库获取数据，不使用缓存，确保数据实时更新
        var (items, totalCount) = await _generalEquipmentRepository.GetGroupedPagedAsync(keyword, pageNumber, pageSize);
        var mappedItems = _mapper.Map<IEnumerable<GeneralEquipmentDto>>(items);
        return new {
            items = mappedItems,
            totalCount,
            pageNumber,
            pageSize
        };
    }

    public async Task<bool> ExistsAsync(string number)
    {
        return await _generalEquipmentRepository.ExistsAsync(number);
    }

    public async Task<object> CreateBatchAsync(IEnumerable<CreateGeneralEquipmentDto> dtos)
    {
        if (dtos is null || !dtos.Any())
            throw new Exception("设备列表不能为空");

        var successCount = 0;
        var errorCount = 0;
        var errors = new List<string>();
        var totalCount = dtos.Count();

        Console.WriteLine($"开始批量导入通用设备，总记录数: {totalCount}");

        // 批量检查设备编号是否重复
        var deviceCodes = dtos.Select(dto => dto.DeviceCode).Where(code => !string.IsNullOrWhiteSpace(code)).ToList();
        if (deviceCodes.Count > 0)
        {
            // 这里可以实现批量检查，暂时使用逐个检查
            var existingCodes = new HashSet<string>();
            foreach (var code in deviceCodes)
            {
                if (await _generalEquipmentRepository.ExistsAsync(code))
                {
                    existingCodes.Add(code);
                }
            }

            if (existingCodes.Count > 0)
            {
                Console.WriteLine($"发现重复设备编号: {string.Join(", ", existingCodes)}");
            }

            foreach (var dto in dtos)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.DeviceCode))
                    {
                        throw new Exception("设备编号不能为空");
                    }
                    if (string.IsNullOrWhiteSpace(dto.DeviceName))
                    {
                        throw new Exception("设备名称不能为空");
                    }
                    if (dto.Quantity <= 0)
                    {
                        throw new Exception("设备数量必须大于0");
                    }

                    if (existingCodes.Contains(dto.DeviceCode))
                    {
                        throw new Exception($"设备编号 {dto.DeviceCode} 已存在");
                    }

                    var generalEquipment = _mapper.Map<GeneralEquipment>(dto);
                    // 转换int类型的枚举值为对应的枚举类型
                    generalEquipment.DeviceType = (DeviceType)dto.DeviceType;
                    generalEquipment.DeviceStatus = (DeviceStatus)dto.DeviceStatus;
                    generalEquipment.UseStatus = (UsageStatus)dto.UseStatus;
                    
                    // 计算设备名称下的序号
                    var existingCount = await _generalEquipmentRepository.CountByNameAsync(dto.DeviceName);
                    generalEquipment.NameSequence = existingCount + 1;

                    var createdGeneralEquipment = await _generalEquipmentRepository.AddAsync(generalEquipment);
                    await CreateInventoryAsync(createdGeneralEquipment.Id, dto.Quantity, false);
                    successCount++;
                    Console.WriteLine($"成功导入设备: {dto.DeviceName} ({dto.DeviceCode})");
                }
                catch (Exception ex)
                {
                    errorCount++;
                    var errorMessage = $"设备 {dto.DeviceName} ({dto.DeviceCode}): {ex.Message}";
                    errors.Add(errorMessage);
                    Console.WriteLine($"导入失败: {errorMessage}");
                }
            }
        }
        else
        {
            Console.WriteLine("没有有效的设备编号");
            foreach (var dto in dtos)
            {
                errorCount++;
                var errorMessage = $"设备 {dto.DeviceName}: 设备编号不能为空";
                errors.Add(errorMessage);
                Console.WriteLine($"导入失败: {errorMessage}");
            }
        }

        Console.WriteLine($"批量导入完成，成功: {successCount}, 失败: {errorCount}, 总记录数: {totalCount}");

        // 清除缓存
        try
        {
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}:all");
            // 清除报表和分析缓存
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}_inventory_summary_0_0");
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}_usage_analysis_12");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"清除缓存失败: {ex.Message}");
            // 缓存清除失败，不影响主流程
        }

        return new
        {
            successCount,
            errorCount,
            totalCount,
            errors
        };
    }

    public async Task<object> DeleteAllAsync()
    {
        try
        {
            // 先获取所有设备，用于计算删除的数量
            var allEquipments = await _generalEquipmentRepository.GetAllAsync();
            var totalCount = allEquipments.Count();
            
            Console.WriteLine($"开始清空库存，总设备数: {totalCount}");
            
            // 直接使用仓库的批量删除方法，确保所有设备都被删除
            await _generalEquipmentRepository.DeleteAllAsync();
            
            // 清除所有与 GeneralEquipments 相关的缓存
            if (_cacheService != null)
            {
                try
                {
                    await _cacheService.RemoveAsync($"{CacheKeyPrefix}:all");
                    await _cacheService.RemovePatternAsync($"{CacheKeyPrefix}_paged_*");
                    await _cacheService.RemovePatternAsync($"{CacheKeyPrefix}_grouped_*");
                    await _cacheService.RemovePatternAsync($"{CacheKeyPrefix}_search_*");
                    await _cacheService.RemoveAsync($"{CacheKeyPrefix}_inventory_summary_0_0");
                    await _cacheService.RemoveAsync($"{CacheKeyPrefix}_usage_analysis_12");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"清除缓存失败: {ex.Message}");
                    // 缓存清除失败，不影响主流程
                }
            }
            
            // 再次获取设备数量，确认是否删除成功
            var remainingEquipments = await _generalEquipmentRepository.GetAllAsync();
            var remainingCount = remainingEquipments.Count();
            var deletedCount = totalCount - remainingCount;
            
            Console.WriteLine($"清空库存完成，删除了 {deletedCount} 台设备，剩余 {remainingCount} 台设备");
            
            // 打印剩余设备的详细信息
            if (remainingCount > 0)
            {
                Console.WriteLine("剩余设备详细信息:");
                foreach (var equipment in remainingEquipments.Take(5)) // 只打印前5台设备
                {
                    Console.WriteLine($"  设备ID: {equipment.Id}, 设备名称: {equipment.DeviceName}, 设备编号: {equipment.DeviceCode}");
                }
            }
            
            return new
            {
                successCount = deletedCount,
                errorCount = 0,
                message = $"成功清空 {deletedCount} 台设备",
                remainingCount = remainingCount
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"清空库存失败: {ex.Message}");
            return new
            {
                successCount = 0,
                errorCount = 1,
                errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<object> DeleteBatchAsync(IEnumerable<int> ids)
    {
        if (ids is null || !ids.Any())
            throw new Exception("设备ID列表不能为空");

        var successCount = 0;
        var errorCount = 0;
        var errors = new List<string>();
        var deviceNames = new HashSet<string>();

        foreach (var id in ids)
        {
            try
            {
                if (id <= 0)
                    throw new Exception("设备ID必须大于0");

                var generalEquipment = await _generalEquipmentRepository.GetByIdAsync(id);
                if (generalEquipment is null)
                    throw new Exception("设备不存在");

                deviceNames.Add(generalEquipment.DeviceName);
                
                // 先删除关联的库存记录
                var inventory = await _inventoryRepository.GetByGeneralEquipmentIdAsync(id);
                if (inventory != null)
                {
                    await _inventoryRepository.DeleteAsync(inventory.Id);
                }
                
                // 再删除设备
                await _generalEquipmentRepository.DeleteAsync(id);
                successCount++;
            }
            catch (Exception ex)
            {
                errorCount++;
                errors.Add($"设备ID {id}: {ex.Message}");
            }
        }

        // 重新计算受影响设备名称的序号
        foreach (var deviceName in deviceNames)
        {
            await RecalculateNameSequencesAsync(deviceName);
        }

        // 清除缓存
        try
        {
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}:all");
            await _cacheService.RemovePatternAsync($"{CacheKeyPrefix}_paged_*");
            await _cacheService.RemovePatternAsync($"{CacheKeyPrefix}_grouped_*");
            await _cacheService.RemovePatternAsync($"{CacheKeyPrefix}_search_*");
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}_inventory_summary_0_0");
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}_usage_analysis_12");
        }
        catch (Exception)
        {
            // 缓存清除失败，不影响主流程
        }

        return new
        {
            successCount,
            errorCount,
            errors
        };
    }

    /// <summary>
    /// 重新计算指定设备名称下的所有设备的序号
    /// </summary>
    /// <param name="deviceName">设备名称</param>
    private async Task RecalculateNameSequencesAsync(string deviceName)
    {
        // 获取该设备名称下的所有设备，按ID排序
        var equipments = await _generalEquipmentRepository.GetAllAsync();
        var sameNameEquipments = equipments
            .Where(e => e.DeviceName == deviceName)
            .OrderBy(e => e.Id)
            .ToList();

        // 重新计算序号
        for (int i = 0; i < sameNameEquipments.Count; i++)
        {
            sameNameEquipments[i].NameSequence = i + 1;
            await _generalEquipmentRepository.UpdateAsync(sameNameEquipments[i]);
        }
    }
}
