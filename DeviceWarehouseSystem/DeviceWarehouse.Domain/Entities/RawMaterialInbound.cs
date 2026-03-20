using System.ComponentModel.DataAnnotations;

namespace DeviceWarehouse.Domain.Entities;

public class RawMaterialInbound
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string InboundNumber { get; set; } = null!;
    
    [Required]
    public DateTime InboundDate { get; set; }
    
    [MaxLength(200)]
    public string? DeliveryPerson { get; set; }

    [MaxLength(200)]
    public string? Company { get; set; }

    [MaxLength(200)]
    public string? Supplier { get; set; }
    
    [MaxLength(500)]
    public string? Remark { get; set; }
    
    [MaxLength(100)]
    public string? Operator { get; set; }
    
    public string? Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    public ICollection<RawMaterialInboundItem> Items { get; set; } = new List<RawMaterialInboundItem>();
}
