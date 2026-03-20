using DeviceWarehouse.Domain.Enums;

namespace DeviceWarehouse.Application.DTOs;



public class SpecialEquipmentDto
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
    public DeviceType DeviceType { get; set; }
    public string DeviceName { get; set; } = null!;
    public string DeviceCode { get; set; } = null!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public int Quantity { get; set; }
    public string? Unit { get; set; }
    public string? ImageUrl { get; set; }
    public byte[]? ImageData { get; set; }
    public string? ImageContentType { get; set; }
    public List<ImageDto>? Images { get; set; }
    public DeviceStatus DeviceStatus { get; set; }
    public UsageStatus UseStatus { get; set; }
    public string? Location { get; set; }
    public string? ProjectName { get; set; }
    public string? ProjectTime { get; set; }
    public string? Company { get; set; }
    public string? Accessories { get; set; }
    public string? Remark { get; set; }
    public int? RepairStatus { get; set; }
    public string? RepairPerson { get; set; }
    public DateTime? RepairDate { get; set; }
    public string? FaultReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSpecialEquipmentDto
{
    public int SortOrder { get; set; }
    public DeviceType DeviceType { get; set; }
    public string DeviceName { get; set; } = null!;
    public string DeviceCode { get; set; } = null!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public int Quantity { get; set; }
    public string? Unit { get; set; }
    public string? ImageUrl { get; set; }
    public byte[]? ImageData { get; set; }
    public string? ImageDataBase64 { get; set; }
    public string? ImageContentType { get; set; }
    public List<ImageDto>? Images { get; set; }
    public DeviceStatus DeviceStatus { get; set; }
    public UsageStatus UseStatus { get; set; }
    public string? Location { get; set; }
    public string? ProjectName { get; set; }
    public string? ProjectTime { get; set; }
    public string? Company { get; set; }
    public string? Accessories { get; set; }
    public string? Remark { get; set; }
}

public class UpdateSpecialEquipmentDto
{
    public int? SortOrder { get; set; }
    public DeviceType? DeviceType { get; set; }
    public string? DeviceName { get; set; }
    public string? DeviceCode { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public int? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? ImageUrl { get; set; }
    public byte[]? ImageData { get; set; }
    public string? ImageDataBase64 { get; set; }
    public string? ImageContentType { get; set; }
    public List<ImageDto>? Images { get; set; }
    public DeviceStatus? DeviceStatus { get; set; }
    public UsageStatus? UseStatus { get; set; }
    public string? Location { get; set; }
    public string? ProjectName { get; set; }
    public string? ProjectTime { get; set; }
    public string? Company { get; set; }
    public string? Accessories { get; set; }
    public string? Remark { get; set; }
    public int? RepairStatus { get; set; }
    public string? RepairPerson { get; set; }
    public DateTime? RepairDate { get; set; }
    public string? FaultReason { get; set; }
}

public class SpecialEquipmentSummaryDto
{
    public string DeviceName { get; set; } = null!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int Count { get; set; }
    public string? Unit { get; set; }
    public DeviceType DeviceType { get; set; }
}

public class InboundOrderDto
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public InboundType InboundType { get; set; }
    public string? DeliveryPerson { get; set; }
    public string? Operator { get; set; }
    public string? Receiver { get; set; }
    public string? ReceiverPhone { get; set; }
    public int TotalQuantity { get; set; }
    public decimal? TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string? Remark { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<InboundOrderItemDto> Items { get; set; } = new List<InboundOrderItemDto>();
}

public class CreateInboundOrderDto
{
    public DateTime OrderDate { get; set; }
    public InboundType InboundType { get; set; }
    public string? DeliveryPerson { get; set; }
    public string? Operator { get; set; }
    public string? Receiver { get; set; }
    public string? ReceiverPhone { get; set; }
    public string? Remark { get; set; }
    public List<CreateInboundOrderItemDto> Items { get; set; } = new List<CreateInboundOrderItemDto>();
}

public class InboundOrderItemDto
{
    public int Id { get; set; }
    public int SpecialEquipmentId { get; set; }
    public string DeviceName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Remark { get; set; }
}

public class CreateInboundOrderItemDto
{
    public int SpecialEquipmentId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Remark { get; set; }
}

public class OutboundOrderDto
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public OutboundType OutboundType { get; set; }
    public string? ProjectName { get; set; }
    public string? Operator { get; set; }
    public int TotalQuantity { get; set; }
    public OrderStatus Status { get; set; }
    public string? Remark { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OutboundOrderItemDto> Items { get; set; } = new List<OutboundOrderItemDto>();
}

public class CreateOutboundOrderDto
{
    public DateTime OrderDate { get; set; }
    public OutboundType OutboundType { get; set; }
    public string? ProjectName { get; set; }
    public string? Operator { get; set; }
    public string? Remark { get; set; }
    public List<CreateOutboundOrderItemDto> Items { get; set; } = new List<CreateOutboundOrderItemDto>();
}

public class OutboundOrderItemDto
{
    public int Id { get; set; }
    public int SpecialEquipmentId { get; set; }
    public string DeviceName { get; set; } = null!;
    public int Quantity { get; set; }
    public string? Remark { get; set; }
}

public class CreateOutboundOrderItemDto
{
    public int SpecialEquipmentId { get; set; }
    public int Quantity { get; set; }
    public string? Remark { get; set; }
}

public class LoginDto
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class TokenDto
{
    public string AccessToken { get; set; } = null!;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public string UserName { get; set; } = null!;
    public string Name { get; set; } = null!;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    
    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }
    
    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
