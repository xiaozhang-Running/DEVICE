using System.ComponentModel.DataAnnotations;

namespace DeviceWarehouse.Domain.Entities;

public class RawMaterialOutbound
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string OutboundNumber { get; set; } = null!;
    
    [Required]
    public DateTime OutboundDate { get; set; }
    
    [MaxLength(200)]
    public string? Recipient { get; set; }
    

    
    [MaxLength(200)]
    public string? Purpose { get; set; }
    
    [MaxLength(500)]
    public string? Remark { get; set; }
    
    [MaxLength(100)]
    public string? Operator { get; set; }
    
    [MaxLength(50)]
    public string? Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    public ICollection<RawMaterialOutboundItem> Items { get; set; } = new List<RawMaterialOutboundItem>();
}

public class RawMaterialOutboundItem
{
    public int Id { get; set; }
    
    [Required]
    public int OutboundId { get; set; }
    
    [Required]
    public int RawMaterialId { get; set; }
    
    [MaxLength(200)]
    public string? Specification { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [MaxLength(500)]
    public string? Remark { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public RawMaterialOutbound? Outbound { get; set; }
    public RawMaterial? RawMaterial { get; set; }
}
