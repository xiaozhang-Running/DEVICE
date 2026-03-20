using System.ComponentModel.DataAnnotations;

namespace DeviceWarehouse.Application.DTOs;

public class RawMaterialOutboundItemDto
{
    public int Id { get; set; }
    public int OutboundId { get; set; }
    public int RawMaterialId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? Specification { get; set; }
    public int Quantity { get; set; }
    public string? Remark { get; set; }
}

public class CreateRawMaterialOutboundItemDto
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

public class RawMaterialOutboundDto
{
    public int Id { get; set; }
    public string OutboundNumber { get; set; } = null!;
    public DateTime OutboundDate { get; set; }
    public string? Recipient { get; set; }
    public string? Remark { get; set; }
    public string? Operator { get; set; }
    public string? Status { get; set; }
    public int TotalQuantity { get; set; }
    public List<RawMaterialOutboundItemDto> Items { get; set; } = new List<RawMaterialOutboundItemDto>();
    public DateTime CreatedAt { get; set; }
}

public class CreateRawMaterialOutboundDto
{
    [Required]
    [MaxLength(50)]
    public string OutboundNumber { get; set; } = null!;
    
    [Required]
    public DateTime OutboundDate { get; set; }
    
    [MaxLength(200)]
    public string? Recipient { get; set; }
    
    [MaxLength(500)]
    public string? Remark { get; set; }
    
    [MaxLength(100)]
    public string? Operator { get; set; }
    
    [Required]
    public List<CreateRawMaterialOutboundItemDto> Items { get; set; } = new List<CreateRawMaterialOutboundItemDto>();
}

public class UpdateRawMaterialOutboundDto
{
    [Required]
    [MaxLength(50)]
    public string OutboundNumber { get; set; } = null!;
    
    [Required]
    public DateTime OutboundDate { get; set; }
    
    [MaxLength(200)]
    public string? Recipient { get; set; }
    
    [MaxLength(500)]
    public string? Remark { get; set; }
    
    [MaxLength(100)]
    public string? Operator { get; set; }
    
    [Required]
    public List<CreateRawMaterialOutboundItemDto> Items { get; set; } = new List<CreateRawMaterialOutboundItemDto>();
}
