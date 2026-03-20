using System.ComponentModel.DataAnnotations;
using DeviceWarehouse.Domain.Enums;

namespace DeviceWarehouse.Domain.Entities;

public class EquipmentInboundItem
{
    public int Id { get; set; }
    
    [Required]
    public int InboundId { get; set; }
    
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
    
    public DeviceType EquipmentType { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public EquipmentInbound? Inbound { get; set; }
}
