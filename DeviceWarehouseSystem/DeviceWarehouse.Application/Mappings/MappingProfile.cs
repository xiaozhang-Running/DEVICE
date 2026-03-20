using AutoMapper;
using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Application.Mappings;

public class MappingProfile : Profile
{
    private static List<string> MapOutboundImages(string? outboundImages)
    {
        if (string.IsNullOrEmpty(outboundImages))
            return new List<string>();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(outboundImages) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public MappingProfile()
    {
        CreateMap<Image, ImageDto>()
            .ForMember(dest => dest.ImageData, opt => opt.MapFrom(src => src.Data))
            .ForMember(dest => dest.ImageContentType, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Url))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
            .ForMember(dest => dest.ImageDataBase64, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, prop) => prop != null));

        CreateMap<ImageDto, Image>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.ImageData))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.ImageContentType ?? src.Type))
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.ImageUrl ?? src.Url))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForAllMembers(opt => opt.Condition((src, dest, prop) => prop != null));

        CreateMap<SpecialEquipment, SpecialEquipmentDto>()
            .ForMember(dest => dest.DeviceStatus, opt => opt.MapFrom(src => src.DeviceStatus))
            .ForMember(dest => dest.UseStatus, opt => opt.MapFrom(src => src.UseStatus))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));


        CreateMap<CreateSpecialEquipmentDto, SpecialEquipment>()
            .ForMember(dest => dest.DeviceStatus, opt => opt.MapFrom(src => src.DeviceStatus))
            .ForMember(dest => dest.UseStatus, opt => opt.MapFrom(src => src.UseStatus))
            // 不再需要映射 ImageData 字段，使用 Images 集合
            .ForSourceMember(src => src.ImageDataBase64, opt => opt.DoNotValidate()); // 忽略ImageDataBase64，在服务层手动处理
        CreateMap<UpdateSpecialEquipmentDto, SpecialEquipment>()
            .ForMember(dest => dest.DeviceStatus, opt => opt.MapFrom(src => src.DeviceStatus))
            .ForMember(dest => dest.UseStatus, opt => opt.MapFrom(src => src.UseStatus))
            // 不再需要映射 ImageData 字段，使用 Images 集合
            .ForSourceMember(src => src.ImageDataBase64, opt => opt.DoNotValidate()) // 忽略ImageDataBase64，在服务层手动处理
            .ForAllMembers(opt => opt.Condition((src, dest, prop) => prop != null));

        CreateMap<GeneralEquipment, GeneralEquipmentDto>()
            .ForMember(dest => dest.DeviceStatus, opt => opt.MapFrom(src => src.DeviceStatus))
            .ForMember(dest => dest.UseStatus, opt => opt.MapFrom(src => src.UseStatus))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));

        CreateMap<CreateGeneralEquipmentDto, GeneralEquipment>()
            .ForMember(dest => dest.DeviceStatus, opt => opt.MapFrom(src => src.DeviceStatus))
            .ForMember(dest => dest.UseStatus, opt => opt.MapFrom(src => src.UseStatus));
        CreateMap<UpdateGeneralEquipmentDto, GeneralEquipment>()
            .ForMember(dest => dest.DeviceStatus, opt => opt.MapFrom(src => src.DeviceStatus))
            .ForMember(dest => dest.UseStatus, opt => opt.MapFrom(src => src.UseStatus))
            .ForAllMembers(opt => opt.Condition((src, dest, prop) => prop != null));

        CreateMap<Consumable, ConsumableDto>();
        CreateMap<CreateConsumableDto, Consumable>();
        CreateMap<UpdateConsumableDto, Consumable>()
            .ForAllMembers(opt => opt.Condition((src, dest, prop) => prop != null));

        CreateMap<RawMaterial, RawMaterialDto>();
        CreateMap<CreateRawMaterialDto, RawMaterial>();
        CreateMap<UpdateRawMaterialDto, RawMaterial>()
            .ForAllMembers(opt => opt.Condition((src, dest, prop) => prop != null));

        CreateMap<RawMaterialInbound, RawMaterialInboundDto>();
        CreateMap<CreateRawMaterialInboundDto, RawMaterialInbound>()
            .ForMember(dest => dest.Items, opt => opt.Ignore());
        CreateMap<UpdateRawMaterialInboundDto, RawMaterialInbound>()
            .ForMember(dest => dest.Items, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, prop) => prop != null));
        
        CreateMap<RawMaterialInboundItem, RawMaterialInboundItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.RawMaterial != null ? src.RawMaterial.ProductName : string.Empty));
        CreateMap<CreateRawMaterialInboundItemDto, RawMaterialInboundItem>();

        CreateMap<RawMaterialOutbound, RawMaterialOutboundDto>();
        CreateMap<CreateRawMaterialOutboundDto, RawMaterialOutbound>()
            .ForMember(dest => dest.Items, opt => opt.Ignore());
        CreateMap<UpdateRawMaterialOutboundDto, RawMaterialOutbound>()
            .ForMember(dest => dest.Items, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, prop) => prop != null));
        
        CreateMap<RawMaterialOutboundItem, RawMaterialOutboundItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.RawMaterial!.ProductName));
        CreateMap<CreateRawMaterialOutboundItemDto, RawMaterialOutboundItem>();

        CreateMap<EquipmentInbound, EquipmentInboundDto>();
        CreateMap<CreateEquipmentInboundDto, EquipmentInbound>()
            .ForMember(dest => dest.Items, opt => opt.Ignore());
        CreateMap<UpdateEquipmentInboundDto, EquipmentInbound>()
            .ForMember(dest => dest.Items, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, prop) => prop != null));
        
        CreateMap<EquipmentInboundItem, EquipmentInboundItemDto>();
        CreateMap<CreateEquipmentInboundItemDto, EquipmentInboundItem>();

        CreateMap<InboundOrder, InboundOrderDto>();
        CreateMap<CreateInboundOrderDto, InboundOrder>();
        CreateMap<InboundOrderItem, InboundOrderItemDto>()
            .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.SpecialEquipment!.DeviceName));
        CreateMap<CreateInboundOrderItemDto, InboundOrderItem>();

        CreateMap<OutboundOrder, OutboundOrderDto>();
        CreateMap<CreateOutboundOrderDto, OutboundOrder>();
        CreateMap<OutboundOrderItem, OutboundOrderItemDto>()
            .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.SpecialEquipment!.DeviceName));
        CreateMap<CreateOutboundOrderItemDto, OutboundOrderItem>();

        CreateMap<Inventory, InventoryDto>()
            .ForMember(dest => dest.EquipmentName, opt => opt.MapFrom(src => 
                src.SpecialEquipment != null ? src.SpecialEquipment.DeviceName : 
                src.GeneralEquipment != null ? src.GeneralEquipment.DeviceName : string.Empty))
            .ForMember(dest => dest.EquipmentCode, opt => opt.MapFrom(src => 
                src.SpecialEquipment != null ? src.SpecialEquipment.DeviceCode : 
                src.GeneralEquipment != null ? src.GeneralEquipment.DeviceCode : string.Empty))
            .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => 
                src.SpecialEquipment != null ? src.SpecialEquipment.Brand : 
                src.GeneralEquipment != null ? src.GeneralEquipment.Brand : string.Empty))
            .ForMember(dest => dest.Model, opt => opt.MapFrom(src => 
                src.SpecialEquipment != null ? src.SpecialEquipment.Model : 
                src.GeneralEquipment != null ? src.GeneralEquipment.Model : string.Empty))
            .ForMember(dest => dest.SerialNumber, opt => opt.MapFrom(src => 
                src.SpecialEquipment != null ? src.SpecialEquipment.SerialNumber : string.Empty))
            .ForMember(dest => dest.UseStatus, opt => opt.MapFrom(src => 
                src.SpecialEquipment != null ? (int?)src.SpecialEquipment.UseStatus : 
                src.GeneralEquipment != null ? (int?)src.GeneralEquipment.UseStatus : null))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => 
                src.SpecialEquipment != null ? "专用设备" : 
                src.GeneralEquipment != null ? "通用设备" : "未知"));
        CreateMap<CreateInventoryDto, Inventory>();
        CreateMap<UpdateInventoryDto, Inventory>()
            .ForAllMembers(opt => opt.Condition((src, dest, prop) => prop != null));

        CreateMap<InventoryTransaction, InventoryTransactionDto>();
        CreateMap<InventoryTransactionDto, InventoryTransaction>();

        CreateMap<ProjectOutbound, ProjectOutboundDto>()
            .ForMember(dest => dest.OutboundImages, opt => opt.MapFrom(src => MapOutboundImages(src.OutboundImages)));
        CreateMap<CreateProjectOutboundDto, ProjectOutbound>();
        CreateMap<UpdateProjectOutboundDto, ProjectOutbound>()
            .ForAllMembers(opt => opt.Condition((src, dest, prop) => prop != null));
        
        CreateMap<ProjectOutboundItem, ProjectOutboundItemDto>();
        CreateMap<CreateProjectOutboundItemDto, ProjectOutboundItem>();

        CreateMap<ProjectInbound, ProjectInboundDto>()
            .ForMember(dest => dest.InboundImages, opt => opt.MapFrom(src => MapOutboundImages(src.InboundImages)));
        CreateMap<CreateProjectInboundDto, ProjectInbound>();
        CreateMap<UpdateProjectInboundDto, ProjectInbound>()
            .ForAllMembers(opt => opt.Condition((src, dest, prop) => prop != null));
        
        CreateMap<ProjectInboundItem, ProjectInboundItemDto>();
        CreateMap<CreateProjectInboundItemDto, ProjectInboundItem>();

        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name));
        CreateMap<CreateUserDto, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName));
        CreateMap<UpdateUserDto, ApplicationUser>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
            .ForAllMembers(opt => opt.Condition((src, dest, prop) => prop != null));

        // 添加User到UserDto的映射
        CreateMap<User, UserDto>();
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>()
            .ForAllMembers(opt => opt.Condition((src, dest, prop) => prop != null));

        CreateMap<ScrapEquipment, ScrapEquipmentDto>();
        CreateMap<CreateScrapEquipmentDto, ScrapEquipment>();
        CreateMap<UpdateScrapEquipmentDto, ScrapEquipment>()
            .ForAllMembers(opt => opt.Condition((src, dest, prop) => prop != null));

        // 角色和权限映射
        CreateMap<Role, RoleDto>();
        CreateMap<Permission, PermissionDto>();

        // 用户活动日志映射
        CreateMap<UserActivityLog, UserActivityLogDto>();

    }
}
