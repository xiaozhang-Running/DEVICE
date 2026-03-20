using System.ComponentModel.DataAnnotations;

namespace DeviceWarehouse.Application.DTOs;

public class RawMaterialInboundItemDto
{
    public int Id { get; set; }
    public int InboundId { get; set; }
    public int RawMaterialId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? Specification { get; set; }
    public int Quantity { get; set; }
    public string? Remark { get; set; }
}

public class CreateRawMaterialInboundItemDto
{
    public int? RawMaterialId { get; set; }
    
    [MaxLength(200)]
    public string? ProductName { get; set; }
    
    [MaxLength(200)]
    public string? Specification { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [MaxLength(500)]
    public string? Remark { get; set; }
}

public class RawMaterialInboundDto
{
    public int Id { get; set; }
    public string InboundNumber { get; set; } = null!;
    public DateTime InboundDate { get; set; }
    public string? DeliveryPerson { get; set; }
    public string? Remark { get; set; }
    public string? Operator { get; set; }
    public string? Status { get; set; }
    public int TotalQuantity { get; set; }
    public List<RawMaterialInboundItemDto> Items { get; set; } = new List<RawMaterialInboundItemDto>();
    public DateTime CreatedAt { get; set; }
}

public class CreateRawMaterialInboundDto
{
    [Required]
    [MaxLength(50)]
    public string InboundNumber { get; set; } = null!;
    
    [Required]
    public DateTime InboundDate { get; set; }
    
    [MaxLength(200)]
    public string? DeliveryPerson { get; set; }
    
    [MaxLength(500)]
    public string? Remark { get; set; }
    
    [MaxLength(100)]
    public string? Operator { get; set; }
    
    [Required]
    public List<CreateRawMaterialInboundItemDto> Items { get; set; } = new List<CreateRawMaterialInboundItemDto>();
}

public class UpdateRawMaterialInboundDto
{
    [Required]
    [MaxLength(50)]
    public string InboundNumber { get; set; } = null!;
    
    [Required]
    public DateTime InboundDate { get; set; }
    
    [MaxLength(200)]
    public string? DeliveryPerson { get; set; }
    
    [MaxLength(500)]
    public string? Remark { get; set; }
    
    [MaxLength(100)]
    public string? Operator { get; set; }
    
    [Required]
    public List<CreateRawMaterialInboundItemDto> Items { get; set; } = new List<CreateRawMaterialInboundItemDto>();
}
