using System.ComponentModel.DataAnnotations;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Enums;

namespace DeviceWarehouse.Application.DTOs;

public class EquipmentInboundItemDto
{
    public int Id { get; set; }
    public int InboundId { get; set; }
    public string DeviceName { get; set; } = null!;
    public string DeviceCode { get; set; } = null!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? Specification { get; set; }
    public int Quantity { get; set; }
    public string? Unit { get; set; }
    public string? ImageUrl { get; set; }
    public string? Status { get; set; }
    public string? Remark { get; set; }
}

public class CreateEquipmentInboundItemDto
{
    [Required]
    [MaxLength(200)]
    public string DeviceName { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string DeviceCode { get; set; } = null!;
    
    [MaxLength(100)]
    public string? Brand { get; set; }
    
    [MaxLength(100)]
    public string? Model { get; set; }
    
    [MaxLength(100)]
    public string? SerialNumber { get; set; }
    
    [MaxLength(200)]
    public string? Specification { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [MaxLength(20)]
    public string? Unit { get; set; }
    
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    
    [MaxLength(200)]
    public string? Status { get; set; }
    
    [MaxLength(500)]
    public string? Remark { get; set; }
    
    [Required]
    public int EquipmentType { get; set; }
}

public class EquipmentInboundDto
{
    public int Id { get; set; }
    public string InboundNumber { get; set; } = null!;
    public DateTime InboundDate { get; set; }
    public DeviceType EquipmentType { get; set; }
    public string EquipmentTypeName { get; set; } = null!;
    public string? DeliveryPerson { get; set; }
    public string? Operator { get; set; }
    public string? Remark { get; set; }
    public string? Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalQuantity { get; set; }
    public List<EquipmentInboundItemDto> Items { get; set; } = new List<EquipmentInboundItemDto>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateEquipmentInboundDto
{
    [Required]
    [MaxLength(50)]
    public string InboundNumber { get; set; } = null!;
    
    [Required]
    public DateTime InboundDate { get; set; }
    
    [Required]
    public DeviceType EquipmentType { get; set; }
    
    [MaxLength(200)]
    public string? DeliveryPerson { get; set; }
    
    [MaxLength(100)]
    public string? Operator { get; set; }
    
    [MaxLength(500)]
    public string? Remark { get; set; }
    
    [Required]
    public List<CreateEquipmentInboundItemDto> Items { get; set; } = new List<CreateEquipmentInboundItemDto>();
}

public class UpdateEquipmentInboundDto
{
    [Required]
    [MaxLength(50)]
    public string InboundNumber { get; set; } = null!;
    
    [Required]
    public DateTime InboundDate { get; set; }
    
    [Required]
    public DeviceType EquipmentType { get; set; }
    
    [MaxLength(200)]
    public string? DeliveryPerson { get; set; }
    
    [MaxLength(100)]
    public string? Operator { get; set; }
    
    [MaxLength(500)]
    public string? Remark { get; set; }
    
    [Required]
    public List<CreateEquipmentInboundItemDto> Items { get; set; } = new List<CreateEquipmentInboundItemDto>();
}
