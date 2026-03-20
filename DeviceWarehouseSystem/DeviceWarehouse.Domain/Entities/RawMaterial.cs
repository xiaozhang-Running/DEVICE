using System.ComponentModel.DataAnnotations;

namespace DeviceWarehouse.Domain.Entities;

public class RawMaterial
{
    public int Id { get; set; }
    
    [Required]
    public int SortOrder { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = null!;
    
    [MaxLength(200)]
    public string? Specification { get; set; }
    
    public int TotalQuantity { get; set; }
    
    public int UsedQuantity { get; set; }
    
    public int RemainingQuantity { get; set; }
    
    [MaxLength(20)]
    public string? Unit { get; set; }
    
    [MaxLength(500)]
    public string? Remark { get; set; }
    
    [MaxLength(200)]
    public string? Supplier { get; set; }
    
    [MaxLength(200)]
    public string? Company { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
