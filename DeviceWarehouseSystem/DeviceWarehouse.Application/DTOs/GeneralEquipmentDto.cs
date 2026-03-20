using DeviceWarehouse.Domain.Enums;

namespace DeviceWarehouse.Application.DTOs;

public class ImageDto
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Url { get; set; }
    public byte[]? ImageData { get; set; }
    public string? ImageDataBase64 { get; set; }
    public string? ImageContentType { get; set; }
    public string? ImageUrl { get; set; }
}

public class GeneralEquipmentDto
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
    public DeviceType DeviceType { get; set; }
    public string DeviceName { get; set; } = null!;
    public string DeviceCode { get; set; } = null!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? Specification { get; set; }
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

public class CreateGeneralEquipmentDto
{
    public int SortOrder { get; set; }
    public int DeviceType { get; set; }
    public string DeviceName { get; set; } = null!;
    public string DeviceCode { get; set; } = null!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? Specification { get; set; }
    public int Quantity { get; set; }
    public string? Unit { get; set; }
    public string? ImageUrl { get; set; }
    public byte[]? ImageData { get; set; }
    public string? ImageDataBase64 { get; set; }
    public string? ImageContentType { get; set; }
    public List<ImageDto>? Images { get; set; }
    public int DeviceStatus { get; set; }
    public int UseStatus { get; set; }
    public string? Location { get; set; }
    public string? ProjectName { get; set; }
    public string? ProjectTime { get; set; }
    public string? Company { get; set; }
    public string? Accessories { get; set; }
    public string? Remark { get; set; }
}

public class UpdateGeneralEquipmentDto
{
    public int? SortOrder { get; set; }
    public DeviceType? DeviceType { get; set; }
    public string? DeviceName { get; set; }
    public string? DeviceCode { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? Specification { get; set; }
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

public class GeneralEquipmentSummaryDto
{
    public string DeviceName { get; set; } = null!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int Count { get; set; }
    public string? Unit { get; set; }
    public DeviceType DeviceType { get; set; }
}
