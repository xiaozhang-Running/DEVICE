using System.ComponentModel.DataAnnotations;

namespace DeviceWarehouse.Domain.Entities;

public class RawMaterialInboundItem
{
    public int Id { get; set; }
    
    [Required]
    public int InboundId { get; set; }
    
    [Required]
    public int RawMaterialId { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [MaxLength(200)]
    public string? Specification { get; set; }
    
    [MaxLength(500)]
    public string? Remark { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public RawMaterialInbound? Inbound { get; set; }
    public RawMaterial? RawMaterial { get; set; }
}
