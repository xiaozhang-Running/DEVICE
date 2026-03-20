using AutoMapper;
using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Application.Exceptions;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Enums;
using DeviceWarehouse.Domain.Interfaces;

namespace DeviceWarehouse.Application.Services;

public class SpecialEquipmentService(ISpecialEquipmentRepository specialEquipmentRepository, IInventoryRepository inventoryRepository, IMapper mapper, ICacheService cacheService) : EquipmentServiceBase<SpecialEquipment, SpecialEquipmentDto, CreateSpecialEquipmentDto, UpdateSpecialEquipmentDto, SpecialEquipmentSummaryDto>(inventoryRepository, mapper, cacheService), ISpecialEquipmentService
{
    // 缓存键前缀
    private const string CacheKeyPrefix = "special_equipment";
    
    // 缓存过期时间
    private static readonly TimeSpan ShortCacheExpiration = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan MediumCacheExpiration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan LongCacheExpiration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan ReportCacheExpiration = TimeSpan.FromHours(1);
    private static readonly TimeSpan AnalysisCacheExpiration = TimeSpan.FromHours(6);
    
    private readonly ISpecialEquipmentRepository _specialEquipmentRepository = specialEquipmentRepository;
    
    /// <summary>
    /// 通用缓存操作方法
    /// </summary>
    /// <typeparam name="T">缓存数据类型</typeparam>
    /// <param name="cacheKey">缓存键</param>
    /// <param name="expiration">过期时间</param>
    /// <param name="fetchData">数据获取方法</param>
    /// <returns>缓存的数据</returns>
    private async Task<T> GetCachedDataAsync<T>(string cacheKey, TimeSpan expiration, Func<Task<T>> fetchData)
    {
        try
        {
            // 尝试从缓存获取
            var cachedData = await _cacheService.GetAsync<T>(cacheKey);
            if (cachedData != null)
            {
                return cachedData;
            }
        }
        catch (Exception)
        {
            // 缓存读取失败，继续从数据源获取
            // 可以记录日志，这里暂时忽略
        }
        
        // 从数据源获取
        var data = await fetchData();
        
        try
        {
            // 缓存结果
            await _cacheService.SetAsync(cacheKey, data, expiration);
        }
        catch (Exception)
        {
            // 缓存写入失败，继续返回数据
            // 可以记录日志，这里暂时忽略
        }
        
        return data;
    }

    public override async Task<SpecialEquipmentDto> GetByIdAsync(int id)
    {
        var cacheKey = $"{CacheKeyPrefix}:id:{id}";
        
        return await GetCachedDataAsync(cacheKey, LongCacheExpiration, async () => {
            var specialEquipment = await _specialEquipmentRepository.GetByIdAsync(id);
            if (specialEquipment is null)
                throw new NotFoundException("设备不存在");
            
            // 确保 Images 属性不为 null
            specialEquipment.Images ??= [];
            
            // 手动创建 SpecialEquipmentDto 对象，避免 AutoMapper 尝试映射 Images 属性
            var dto = new SpecialEquipmentDto
            {
                Id = specialEquipment.Id,
                SortOrder = specialEquipment.SortOrder,
                DeviceType = specialEquipment.DeviceType,
                DeviceName = specialEquipment.DeviceName,
                DeviceCode = specialEquipment.DeviceCode,
                Brand = specialEquipment.Brand,
                Model = specialEquipment.Model,
                SerialNumber = specialEquipment.SerialNumber,
                Quantity = specialEquipment.Quantity,
                Unit = specialEquipment.Unit,
                DeviceStatus = specialEquipment.DeviceStatus,
                UseStatus = specialEquipment.UseStatus,
                Location = specialEquipment.Location,
                ProjectName = specialEquipment.ProjectName,
                ProjectTime = specialEquipment.ProjectTime,
                Company = specialEquipment.Company,
                Accessories = specialEquipment.Accessories,
                Remark = specialEquipment.Remark,
                RepairStatus = specialEquipment.RepairStatus,
                RepairPerson = specialEquipment.RepairPerson,
                RepairDate = specialEquipment.RepairDate,
                FaultReason = specialEquipment.FaultReason,
                CreatedAt = specialEquipment.CreatedAt,
                // 初始化 Images 数组
                Images = []
            };
            
            // 设置主图片 URL（使用第一张图片）
            if (specialEquipment.Images != null && specialEquipment.Images.Count > 0)
            {
                var firstImage = specialEquipment.Images.FirstOrDefault();
                if (firstImage != null)
                {
                    dto.ImageUrl = firstImage.Url;
                }
            }
            
            // 手动映射 Image 实体到 ImageDto
            if (specialEquipment.Images != null && specialEquipment.Images.Count > 0)
            {
                foreach (var image in specialEquipment.Images)
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
            
            return dto;
        });
    }

    public override async Task<IEnumerable<SpecialEquipmentDto>> GetAllAsync()
    {
        var cacheKey = $"{CacheKeyPrefix}:all";
        
        return await GetCachedDataAsync(cacheKey, MediumCacheExpiration, async () => {
            var specialEquipments = await _specialEquipmentRepository.GetAllAsync();
            
            // 手动创建 SpecialEquipmentDto 对象，避免 AutoMapper 尝试映射 Images 属性
            var dtos = specialEquipments.Select(item => {
                // 确保 Images 属性不为 null
                item.Images ??= [];
                
                // 创建 SpecialEquipmentDto 对象
                var dto = new SpecialEquipmentDto
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
                    Images = []
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
            
            return dtos;
        });
    }

    public override async Task<IEnumerable<SpecialEquipmentDto>> GetByTypeAsync(DeviceType type)
    {
        var cacheKey = $"{CacheKeyPrefix}:type:{type}";
        
        return await GetCachedDataAsync(cacheKey, MediumCacheExpiration, async () => {
            var specialEquipments = await _specialEquipmentRepository.GetByTypeAsync(type);
            
            // 手动创建 SpecialEquipmentDto 对象，避免 AutoMapper 尝试映射 Images 属性
            var dtos = specialEquipments.Select(item => {
                // 确保 Images 属性不为 null
                item.Images ??= [];
                
                // 创建 SpecialEquipmentDto 对象
                var dto = new SpecialEquipmentDto
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
                    Images = []
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
            
            return dtos;
        });
    }

    public override async Task<IEnumerable<SpecialEquipmentSummaryDto>> GetEquipmentSummaryAsync(DeviceType? type)
    {
        var specialEquipments = type.HasValue
            ? await _specialEquipmentRepository.GetByTypeAsync(type.Value)
            : await _specialEquipmentRepository.GetAllAsync();

        var summary = specialEquipments
            .GroupBy(d => new { d.DeviceName, d.Brand, d.Model })
            .Select(g => new SpecialEquipmentSummaryDto
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

    public override async Task<SpecialEquipmentDto> CreateAsync(CreateSpecialEquipmentDto dto)
    {
        if (dto is null)
            throw new ValidationException("设备信息不能为空");
        if (string.IsNullOrWhiteSpace(dto.DeviceCode))
            throw new ValidationException("设备编号不能为空");
        if (string.IsNullOrWhiteSpace(dto.DeviceName))
            throw new ValidationException("设备名称不能为空");
        if (dto.Quantity <= 0)
            throw new ValidationException("设备数量必须大于0");

        if (await _specialEquipmentRepository.ExistsAsync(dto.DeviceCode))
            throw new AlreadyExistsException("设备编号已存在");

        // 手动创建SpecialEquipment对象，避免映射Images属性
        var specialEquipment = new SpecialEquipment
        {
            SortOrder = dto.SortOrder,
            DeviceType = dto.DeviceType,
            DeviceName = dto.DeviceName,
            DeviceCode = dto.DeviceCode,
            Brand = dto.Brand,
            Model = dto.Model,
            SerialNumber = dto.SerialNumber,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            Location = dto.Location,
            ProjectName = dto.ProjectName,
            ProjectTime = dto.ProjectTime,
            Company = dto.Company,
            Accessories = dto.Accessories,
            Remark = dto.Remark
        };
        
        // 处理图片数据
        if (dto.Images != null && dto.Images.Count > 0)
        {
            // 处理并保存所有图片
            foreach (var imageDto in dto.Images)
            {
                if (!string.IsNullOrWhiteSpace(imageDto.Url))
                {
                    try
                    {
                        // 确保base64数据不包含前缀
                        var base64Data = imageDto.Url;
                        if (base64Data.StartsWith("data:image/"))
                        {
                            base64Data = base64Data.Split(",")[1];
                        }
                        // 去除可能的空白字符
                        base64Data = base64Data.Trim();
                        // 确保base64数据长度是4的倍数
                        while (base64Data.Length % 4 != 0)
                        {
                            base64Data += "=";
                        }
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
                        
                        specialEquipment.Images.Add(image);
                    }
                    catch (Exception ex)
                    {
                        // 图片数据转换失败，记录错误但不影响创建过程
                        Console.WriteLine($"图片数据转换失败: {ex.Message}");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(imageDto.ImageDataBase64))
                {
                    try
                    {
                        // 确保base64数据不包含前缀
                        var base64Data = imageDto.ImageDataBase64;
                        if (base64Data.StartsWith("data:image/"))
                        {
                            base64Data = base64Data.Split(",")[1];
                        }
                        // 去除可能的空白字符
                        base64Data = base64Data.Trim();
                        // 确保base64数据长度是4的倍数
                        while (base64Data.Length % 4 != 0)
                        {
                            base64Data += "=";
                        }
                        var imageData = Convert.FromBase64String(base64Data);
                        
                        // 创建新的Image实体
                        var image = new Image
                        {
                            Name = "Image",
                            Type = imageDto.ImageContentType ?? "image/png",
                            Data = imageData,
                            Url = imageDto.ImageUrl ?? string.Empty,
                            CreatedAt = DateTime.Now
                        };
                        
                        specialEquipment.Images.Add(image);
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
                if (!string.IsNullOrWhiteSpace(firstImage.Url))
                {
                    // 不再设置 ImageUrl 字段，使用 Images 集合
                }
            }
        }
        else if (!string.IsNullOrWhiteSpace(dto.ImageDataBase64))
        {
            try
            {
                // 确保base64数据不包含前缀
                var base64Data = dto.ImageDataBase64;
                if (base64Data.StartsWith("data:image/"))
                {
                    base64Data = base64Data.Split(",")[1];
                }
                // 去除可能的空白字符
                base64Data = base64Data.Trim();
                // 确保base64数据长度是4的倍数
                while (base64Data.Length % 4 != 0)
                {
                    base64Data += "=";
                }
                var imageData = Convert.FromBase64String(base64Data);
                
                // 创建新的Image实体
                var image = new Image
                {
                    Name = "Image",
                    Type = dto.ImageContentType ?? "image/png",
                    Data = imageData,
                    Url = dto.ImageUrl ?? string.Empty,
                    CreatedAt = DateTime.Now
                };
                
                specialEquipment.Images.Add(image);
            }
            catch (Exception ex)
            {
                // 图片数据转换失败，记录错误但不影响创建过程
                Console.WriteLine($"图片数据转换失败: {ex.Message}");
                // 记录详细的错误信息，包括base64数据的长度和前20个字符
                Console.WriteLine($"Base64数据长度: {dto.ImageDataBase64.Length}");
                Console.WriteLine($"Base64数据前20个字符: {dto.ImageDataBase64[..Math.Min(20, dto.ImageDataBase64.Length)]}");
            }
        }
        
        specialEquipment.DeviceStatus = DeviceStatus.Normal;
        specialEquipment.UseStatus = UsageStatus.Unused;
        
        // 计算设备名称下的序号
        var existingCount = await _specialEquipmentRepository.CountByNameAsync(dto.DeviceName);
        specialEquipment.NameSequence = existingCount + 1;

        var createdSpecialEquipment = await _specialEquipmentRepository.AddAsync(specialEquipment);

        // 使用基类方法创建库存
        await CreateInventoryAsync(createdSpecialEquipment.Id, dto.Quantity, true);

        // 清除缓存
        try
        {
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}:all");
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}:type:{specialEquipment.DeviceType}");
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}:id:{createdSpecialEquipment.Id}");
            // 清除报表和分析缓存
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}_inventory_summary_0_0");
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}_usage_analysis_12");
        }
        catch (Exception)
        {
            // 缓存清除失败，不影响主流程
            // 可以记录日志，这里暂时忽略
        }

        return _mapper.Map<SpecialEquipmentDto>(createdSpecialEquipment);
    }

    public override async Task<SpecialEquipmentDto> UpdateAsync(int id, UpdateSpecialEquipmentDto dto)
    {
        if (id <= 0)
            throw new ValidationException("设备ID必须大于0");
        if (dto is null)
            throw new ValidationException("设备信息不能为空");
        if (string.IsNullOrWhiteSpace(dto.DeviceCode))
            throw new ValidationException("设备编号不能为空");
        if (string.IsNullOrWhiteSpace(dto.DeviceName))
            throw new ValidationException("设备名称不能为空");
        if (dto.Quantity <= 0)
            throw new ValidationException("设备数量必须大于0");

        var specialEquipment = await _specialEquipmentRepository.GetByIdAsync(id);
        if (specialEquipment is null)
            throw new NotFoundException("设备不存在");

        // 处理图片数据
        if (dto.Images != null && dto.Images.Count > 0)
        {
            // 清空现有的图片
            specialEquipment.Images.Clear();
            
            // 处理并保存所有图片
            foreach (var imageDto in dto.Images)
            {
                if (!string.IsNullOrWhiteSpace(imageDto.Url))
                {
                    try
                    {
                        // 确保base64数据不包含前缀
                        var base64Data = imageDto.Url;
                        if (base64Data.StartsWith("data:image/"))
                        {
                            base64Data = base64Data.Split(",")[1];
                        }
                        // 去除可能的空白字符
                        base64Data = base64Data.Trim();
                        // 确保base64数据长度是4的倍数
                        while (base64Data.Length % 4 != 0)
                        {
                            base64Data += "=";
                        }
                        var imageData = Convert.FromBase64String(base64Data);
                        
                        // 创建新的Image实体
                        var image = new Image
                        {
                            Name = imageDto.Name ?? "Image",
                            Type = imageDto.Type ?? "image/png",
                            Data = imageData,
                            Url = imageDto.Url,
                            SpecialEquipmentId = specialEquipment.Id,
                            CreatedAt = DateTime.Now
                        };
                        
                        specialEquipment.Images.Add(image);
                    }
                    catch (Exception ex)
                    {
                        // 图片数据转换失败，记录错误但不影响更新过程
                        Console.WriteLine($"图片数据转换失败: {ex.Message}");
                    }
                }
            }
            
            // 使用第一张图片作为主图片（保持向后兼容）
            if (dto.Images.Count > 0)
            {
                var firstImage = dto.Images[0];
                if (!string.IsNullOrWhiteSpace(firstImage.Url))
                {
                    // 不再设置 ImageUrl 字段，使用 Images 集合
                }
            }
        }
        else if (!string.IsNullOrWhiteSpace(dto.ImageDataBase64))
        {
            try
            {
                // 确保base64数据不包含前缀
                var base64Data = dto.ImageDataBase64;
                if (base64Data.StartsWith("data:image/"))
                {
                    base64Data = base64Data.Split(",")[1];
                }
                // 去除可能的空白字符
                base64Data = base64Data.Trim();
                // 确保base64数据长度是4的倍数
                while (base64Data.Length % 4 != 0)
                {
                    base64Data += "=";
                }
                var imageData = Convert.FromBase64String(base64Data);
                
                // 清空现有的图片
                specialEquipment.Images.Clear();
                
                // 创建新的Image实体
                var image = new Image
                {
                    Name = "Image",
                    Type = dto.ImageContentType ?? "image/png",
                    Data = imageData,
                    Url = dto.ImageUrl ?? string.Empty,
                    SpecialEquipmentId = specialEquipment.Id,
                    CreatedAt = DateTime.Now
                };
                
                specialEquipment.Images.Add(image);
            }
            catch (Exception ex)
            {
                // 图片数据转换失败，记录错误但不影响更新过程
                Console.WriteLine($"图片数据转换失败: {ex.Message}");
                // 记录详细的错误信息，包括base64数据的长度和前20个字符
                Console.WriteLine($"Base64数据长度: {dto.ImageDataBase64.Length}");
                Console.WriteLine($"Base64数据前20个字符: {dto.ImageDataBase64[..Math.Min(20, dto.ImageDataBase64.Length)]}");
            }
        }
        else
        {
            // 如果没有提供图片，清空图片相关字段
            specialEquipment.Images.Clear();
        }
        
        // 先处理库存关联，再更新设备
        if (dto.DeviceType.HasValue)
        {
            // 获取当前设备类型
            var currentDeviceType = specialEquipment.DeviceType;
            // 获取新设备类型
            var newDeviceType = dto.DeviceType.Value;
            
            // 如果设备类型从专用设备改为非专用设备，需要先删除库存关联
            if (currentDeviceType == DeviceType.SpecialDevice && newDeviceType != DeviceType.SpecialDevice)
            {
                // 使用专用的方法删除库存关联，避免EF Core跟踪冲突
                await _inventoryRepository.DeleteBySpecialEquipmentIdAsync(id);
            }
            // 如果设备类型从非专用设备改为专用设备，需要先创建库存关联
            else if (currentDeviceType != DeviceType.SpecialDevice && newDeviceType == DeviceType.SpecialDevice)
            {
                // 使用专用的方法创建库存关联，避免EF Core跟踪冲突
                await _inventoryRepository.CreateBySpecialEquipmentIdAsync(id, dto.Quantity.HasValue ? dto.Quantity.Value : specialEquipment.Quantity);
            }
            // 如果设备类型保持为专用设备，需要更新库存数量
            else if (currentDeviceType == DeviceType.SpecialDevice && newDeviceType == DeviceType.SpecialDevice)
            {
                // 使用专用的方法更新库存数量，避免EF Core跟踪冲突
                await _inventoryRepository.UpdateBySpecialEquipmentIdAsync(id, dto.Quantity.HasValue ? dto.Quantity.Value : specialEquipment.Quantity);
            }
        }
        else if (specialEquipment.DeviceType == DeviceType.SpecialDevice)
        {
            // 如果设备类型保持为专用设备，且未指定新设备类型，需要更新库存数量
            // 使用专用的方法更新库存数量，避免EF Core跟踪冲突
            await _inventoryRepository.UpdateBySpecialEquipmentIdAsync(id, dto.Quantity.HasValue ? dto.Quantity.Value : specialEquipment.Quantity);
        }
        
        // 清除Inventory导航属性，避免EF Core跟踪冲突
        specialEquipment.Inventory = null;
        
        // 手动映射基本属性，避免映射Images属性
        if (dto.SortOrder.HasValue) specialEquipment.SortOrder = dto.SortOrder.Value;
        if (dto.DeviceType.HasValue) specialEquipment.DeviceType = dto.DeviceType.Value;
        if (!string.IsNullOrWhiteSpace(dto.DeviceName)) specialEquipment.DeviceName = dto.DeviceName;
        if (!string.IsNullOrWhiteSpace(dto.DeviceCode)) specialEquipment.DeviceCode = dto.DeviceCode;
        if (!string.IsNullOrWhiteSpace(dto.Brand)) specialEquipment.Brand = dto.Brand;
        if (!string.IsNullOrWhiteSpace(dto.Model)) specialEquipment.Model = dto.Model;
        if (!string.IsNullOrWhiteSpace(dto.SerialNumber)) specialEquipment.SerialNumber = dto.SerialNumber;
        if (dto.Quantity.HasValue) specialEquipment.Quantity = dto.Quantity.Value;
        if (!string.IsNullOrWhiteSpace(dto.Unit)) specialEquipment.Unit = dto.Unit;
        if (!string.IsNullOrWhiteSpace(dto.Location)) specialEquipment.Location = dto.Location;
        if (!string.IsNullOrWhiteSpace(dto.ProjectName)) specialEquipment.ProjectName = dto.ProjectName;
        if (!string.IsNullOrWhiteSpace(dto.ProjectTime)) specialEquipment.ProjectTime = dto.ProjectTime;
        if (!string.IsNullOrWhiteSpace(dto.Company)) specialEquipment.Company = dto.Company;
        if (!string.IsNullOrWhiteSpace(dto.Accessories)) specialEquipment.Accessories = dto.Accessories;
        if (!string.IsNullOrWhiteSpace(dto.Remark)) specialEquipment.Remark = dto.Remark;
        if (dto.DeviceStatus.HasValue) specialEquipment.DeviceStatus = dto.DeviceStatus.Value;
        if (dto.UseStatus.HasValue) specialEquipment.UseStatus = dto.UseStatus.Value;
        if (dto.RepairStatus.HasValue) specialEquipment.RepairStatus = dto.RepairStatus.Value;
        if (!string.IsNullOrWhiteSpace(dto.RepairPerson)) specialEquipment.RepairPerson = dto.RepairPerson;
        if (dto.RepairDate.HasValue) specialEquipment.RepairDate = dto.RepairDate.Value;
        if (!string.IsNullOrWhiteSpace(dto.FaultReason)) specialEquipment.FaultReason = dto.FaultReason;
        

        
        // 设置更新时间
        specialEquipment.UpdatedAt = DateTime.Now;
        
        // 更新设备
        await _specialEquipmentRepository.UpdateAsync(specialEquipment);

        // 清除缓存
        try
        {
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}:all");
            await _cacheService.RemoveAsync($"{CacheKeyPrefix}:type:{specialEquipment.DeviceType}");
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
        }
        catch (Exception)
        {
            // 缓存清除失败，不影响主流程
            // 可以记录日志，这里暂时忽略
        }

        return _mapper.Map<SpecialEquipmentDto>(specialEquipment);
    }

    public async Task<object> CreateBatchAsync(IEnumerable<CreateSpecialEquipmentDto> dtos)
    {
        if (dtos is null || !dtos.Any())
            throw new ValidationException("设备列表不能为空");

        var successCount = 0;
        var errorCount = 0;
        var errors = new List<string>();
        var totalCount = dtos.Count();

        Console.WriteLine($"开始批量导入专用设备，总记录数: {totalCount}");

        // 批量检查设备编号是否重复
        var deviceCodes = dtos.Select(dto => dto.DeviceCode).Where(code => !string.IsNullOrWhiteSpace(code)).ToList();
        if (deviceCodes.Count > 0)
        {
            // 这里可以实现批量检查，暂时使用逐个检查
            var existingCodes = new HashSet<string>();
            foreach (var code in deviceCodes)
            {
                if (await _specialEquipmentRepository.ExistsAsync(code))
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
                        throw new ValidationException("设备编号不能为空");
                    }
                    if (string.IsNullOrWhiteSpace(dto.DeviceName))
                    {
                        throw new ValidationException("设备名称不能为空");
                    }
                    if (dto.Quantity <= 0)
                    {
                        throw new ValidationException("设备数量必须大于0");
                    }

                    if (existingCodes.Contains(dto.DeviceCode))
                    {
                        throw new AlreadyExistsException($"设备编号 {dto.DeviceCode} 已存在");
                    }

                    var specialEquipment = _mapper.Map<SpecialEquipment>(dto);
                    
                    // 设置设备状态和使用状态
                    specialEquipment.DeviceStatus = DeviceStatus.Normal;
                    specialEquipment.UseStatus = UsageStatus.Unused;
                    
                    // 计算设备名称下的序号
                    var existingCount = await _specialEquipmentRepository.CountByNameAsync(dto.DeviceName);
                    specialEquipment.NameSequence = existingCount + 1;

                    var createdSpecialEquipment = await _specialEquipmentRepository.AddAsync(specialEquipment);
                    await CreateInventoryAsync(createdSpecialEquipment.Id, dto.Quantity, true);
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
        if (_cacheService != null)
        {
            try {
                // 清除所有与 SpecialEquipments 相关的缓存
                await _cacheService.RemoveAsync($"{CacheKeyPrefix}:all");
                await _cacheService.RemovePatternAsync($"{CacheKeyPrefix}_paged_*");
                await _cacheService.RemovePatternAsync($"{CacheKeyPrefix}_grouped_*");
                await _cacheService.RemovePatternAsync($"{CacheKeyPrefix}_search_*");
                // 清除报表和分析缓存
                await _cacheService.RemoveAsync($"{CacheKeyPrefix}_inventory_summary_0_0");
                await _cacheService.RemoveAsync($"{CacheKeyPrefix}_usage_analysis_12");
            } catch (Exception ex) {
                Console.WriteLine($"清除缓存失败: {ex.Message}");
                // 缓存清除失败，不影响主流程
            }
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
            var allEquipments = await _specialEquipmentRepository.GetAllAsync();
            var totalCount = allEquipments.Count();
            
            Console.WriteLine($"开始清空库存，总设备数: {totalCount}");
            
            // 直接使用仓库的批量删除方法，确保所有设备都被删除
            await _specialEquipmentRepository.DeleteAllAsync();
            
            // 清除所有与 SpecialEquipments 相关的缓存
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
            var remainingEquipments = await _specialEquipmentRepository.GetAllAsync();
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



    /// <summary>
    /// 获取设备汇总信息
    /// </summary>
    /// <returns>设备汇总列表</returns>
    public async Task<IEnumerable<SpecialEquipmentSummaryDto>> GetSummaryAsync()
    {
        return await GetEquipmentSummaryAsync(null);
    }

    public override async Task DeleteAsync(int id)
    {
        if (id <= 0)
            throw new ValidationException("设备ID必须大于0");

        var specialEquipment = await _specialEquipmentRepository.GetByIdAsync(id);
        if (specialEquipment is null)
            throw new NotFoundException("设备不存在");

        var deletedEquipment = specialEquipment;
        var deviceName = deletedEquipment.DeviceName;
        
        // 先删除关联的库存记录
        await _inventoryRepository.DeleteBySpecialEquipmentIdAsync(id);
        
        // 再删除设备
        await _specialEquipmentRepository.DeleteAsync(id);

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
            // 可以记录日志，这里暂时忽略
        }
    }

    public override async Task<IEnumerable<SpecialEquipmentDto>> SearchAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return Enumerable.Empty<SpecialEquipmentDto>();

        var cacheKey = $"{CacheKeyPrefix}_search_{keyword.ToLower()}";
        
        return await GetCachedDataAsync(cacheKey, ShortCacheExpiration, async () => {
            var specialEquipments = keyword.ToLower() == "all" 
                ? await _specialEquipmentRepository.GetAllAsync() 
                : await _specialEquipmentRepository.SearchAsync(keyword);
            return _mapper.Map<IEnumerable<SpecialEquipmentDto>>(specialEquipments);
        });
    }

    public override async Task<object> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false, DeviceStatus? deviceStatus = null, UsageStatus? useStatus = null, string? brand = null)
    {
        if (pageNumber < 1)
            throw new ValidationException("页码必须大于0");
        if (pageSize < 1 || pageSize > 100)
            throw new ValidationException("每页大小必须在1-100之间");

        try {
            // 直接从数据库获取数据，不使用缓存，确保数据实时更新
            var (items, totalCount, pageNumberResult, pageSizeResult, totalPages) = await _specialEquipmentRepository.GetPagedAsync(pageNumber, pageSize, keyword, sortBy, sortDescending, deviceStatus, useStatus, brand);
            
            // 手动创建 SpecialEquipmentDto 对象，避免 AutoMapper 尝试映射 Images 属性
            var mappedItems = items.Select(item => {
                // 确保 Images 属性不为 null
                item.Images ??= [];
                
                // 创建 SpecialEquipmentDto 对象
                var dto = new SpecialEquipmentDto
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
                    Images = []
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
                items = Enumerable.Empty<SpecialEquipmentDto>(),
                totalCount = 0,
                pageNumber = pageNumber,
                pageSize = pageSize,
                totalPages = 0
            };
        }
    }

    public override async Task<object> GetGroupedPagedAsync(int pageNumber, int pageSize, string? keyword = null)
    {
        if (pageNumber < 1)
            throw new ValidationException("页码必须大于0");
        if (pageSize < 1 || pageSize > 100)
            throw new ValidationException("每页大小必须在1-100之间");

        var cacheKey = $"{CacheKeyPrefix}_grouped_{pageNumber}_{pageSize}_{keyword ?? "null"}";
        
        return await GetCachedDataAsync(cacheKey, ShortCacheExpiration, async () => {
            var (items, groupCount) = await _specialEquipmentRepository.GetGroupedPagedAsync(keyword, pageNumber, pageSize);
            var mappedItems = _mapper.Map<IEnumerable<SpecialEquipmentDto>>(items);
            return new {
                items = mappedItems,
                groupCount,
                totalCount = mappedItems.Count(),
                pageNumber,
                pageSize
            };
        });
    }
    
    // 保持原有的 GetSpecialEquipmentSummaryAsync 方法，因为它是 ISpecialEquipmentService 接口的一部分
    public async Task<IEnumerable<SpecialEquipmentSummaryDto>> GetSpecialEquipmentSummaryAsync(DeviceType? type)
    {
        return await GetEquipmentSummaryAsync(type);
    }

    // 暂时移除这些方法，因为存储过程创建失败
    // 后续可以通过EF Core实现这些功能

    public async Task<IEnumerable<object>> GetInventorySummaryAsync(int? deviceType, int? status)
    {
        // 生成缓存键
        var cacheKey = $"{CacheKeyPrefix}_inventory_summary_{deviceType ?? 0}_{status ?? 0}";
        
        return await GetCachedDataAsync(cacheKey, MediumCacheExpiration, async () => {
            var equipments = await _specialEquipmentRepository.GetAllAsync();
            
            if (deviceType.HasValue)
            {
                equipments = equipments.Where(e => (int)e.DeviceType == deviceType.Value);
            }
            
            if (status.HasValue)
            {
                equipments = equipments.Where(e => (int)e.DeviceStatus == status.Value);
            }
            
            return equipments.Select(e => new InventorySummary
            {
                Id = e.Id,
                DeviceName = e.DeviceName,
                DeviceCode = e.DeviceCode,
                Brand = e.Brand,
                Model = e.Model,
                CurrentStock = e.Quantity,
                DeviceStatus = (int)e.DeviceStatus,
                UseStatus = (int)e.UseStatus,
                Location = e.Location,
                AlertMinQuantity = e.Inventory?.AlertMinQuantity ?? 0,
                AlertMaxQuantity = e.Inventory?.AlertMaxQuantity ?? 0
            }).OrderBy(s => s.DeviceName).ToList();
        });
    }

    public async Task<IEnumerable<object>> GetOutboundReportAsync(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
            throw new ValidationException("开始日期不能晚于结束日期");

        // 生成缓存键
        var cacheKey = $"{CacheKeyPrefix}_outbound_report_{startDate.ToString("yyyyMMdd")}_{endDate.ToString("yyyyMMdd")}";
        
        return await GetCachedDataAsync(cacheKey, ReportCacheExpiration, async () => {
            var equipments = await _specialEquipmentRepository.GetAllAsync();
            
            // 这里可以根据实际的出库记录来生成报表
            // 暂时基于设备信息生成模拟数据
            return equipments
                .Where(e => e.OutboundItems != null && e.OutboundItems.Any(oi => oi.Order != null && oi.Order.OrderDate >= startDate && oi.Order.OrderDate <= endDate))
                .SelectMany(e => e.OutboundItems
                    .Where(oi => oi.Order != null && oi.Order.OrderDate >= startDate && oi.Order.OrderDate <= endDate)
                    .Select(oi => new OutboundReport
                    {
                        OrderCode = oi.Order.OrderCode,
                        OrderDate = oi.Order.OrderDate,
                        OutboundType = (int)oi.Order.OutboundType,
                        ProjectName = oi.Order.ProjectName,
                        DeviceName = e.DeviceName,
                        Brand = e.Brand,
                        Model = e.Model,
                        Quantity = oi.Quantity,
                        Operator = oi.Order.Operator
                    }))
                .OrderByDescending(r => r.OrderDate)
                .ToList();
        });
    }

    public async Task<IEnumerable<object>> GetDeviceUsageAnalysisAsync(int months)
    {
        if (months <= 0)
            throw new ValidationException("月份数必须大于0");

        // 生成缓存键
        var cacheKey = $"{CacheKeyPrefix}_usage_analysis_{months}";
        
        return await GetCachedDataAsync(cacheKey, AnalysisCacheExpiration, async () => {
            var startDate = DateTime.Now.AddMonths(-months);
            var equipments = await _specialEquipmentRepository.GetAllAsync();

            // 实现设备使用分析功能
            return equipments
                .Select(e => new DeviceUsageAnalysis
                {
                    DeviceName = e.DeviceName,
                    Brand = e.Brand,
                    Model = e.Model,
                    UsageCount = e.OutboundItems != null ? e.OutboundItems.Count(oi => oi.Order != null && oi.Order.OrderDate >= startDate) : 0,
                    TotalQuantity = e.Quantity,
                    LastUsedDate = e.OutboundItems != null ? e.OutboundItems
                        .Where(oi => oi.Order != null && oi.Order.OrderDate >= startDate)
                        .OrderByDescending(oi => oi.Order.OrderDate)
                        .Select(oi => (DateTime?)oi.Order.OrderDate)
                        .FirstOrDefault() : null
                })
                .OrderByDescending(a => a.UsageCount)
                .ToList();
        });
    }

    public async Task<bool> ExistsAsync(string number)
    {
        return await _specialEquipmentRepository.ExistsAsync(number);
    }

    /// <summary>
    /// 重新计算指定设备名称下的所有设备的序号
    /// </summary>
    /// <param name="deviceName">设备名称</param>
    private async Task RecalculateNameSequencesAsync(string deviceName)
    {
        // 获取该设备名称下的所有设备，按ID排序
        var equipments = await _specialEquipmentRepository.GetAllAsync();
        var sameNameEquipments = equipments
            .Where(e => e.DeviceName == deviceName)
            .OrderBy(e => e.Id)
            .ToList();

        // 重新计算序号
        for (int i = 0; i < sameNameEquipments.Count; i++)
        {
            sameNameEquipments[i].NameSequence = i + 1;
            await _specialEquipmentRepository.UpdateAsync(sameNameEquipments[i]);
        }
    }
}

public class InventorySummary
{
    public int Id { get; set; }
    public string DeviceName { get; set; } = null!;
    public string DeviceCode { get; set; } = null!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int CurrentStock { get; set; }
    public int DeviceStatus { get; set; }
    public int UseStatus { get; set; }
    public string? Location { get; set; }
    public int AlertMinQuantity { get; set; }
    public int AlertMaxQuantity { get; set; }
}

public class OutboundReport
{
    public string OrderCode { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public int OutboundType { get; set; }
    public string? ProjectName { get; set; }
    public string DeviceName { get; set; } = null!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int Quantity { get; set; }
    public string? Operator { get; set; }
}

public class DeviceUsageAnalysis
{
    public string DeviceName { get; set; } = null!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int UsageCount { get; set; }
    public int TotalQuantity { get; set; }
    public DateTime? LastUsedDate { get; set; }
}