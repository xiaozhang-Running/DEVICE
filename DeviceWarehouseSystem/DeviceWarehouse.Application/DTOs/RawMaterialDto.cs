using System.ComponentModel.DataAnnotations;

namespace DeviceWarehouse.Application.DTOs;

public class RawMaterialDto
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
    public string ProductName { get; set; } = null!;
    public string? Specification { get; set; }
    public int TotalQuantity { get; set; }
    public int UsedQuantity { get; set; }
    public int RemainingQuantity { get; set; }
    public string? Unit { get; set; }
    public string? Remark { get; set; }
    public string? Supplier { get; set; }
    public string? Company { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateRawMaterialDto
{
    public int SortOrder { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = null!;
    
    [MaxLength(200)]
    public string? Specification { get; set; }
    
    public int TotalQuantity { get; set; }
    
    [MaxLength(20)]
    public string? Unit { get; set; }
    
    [MaxLength(500)]
    public string? Remark { get; set; }
    
    [MaxLength(200)]
    public string? Supplier { get; set; }
    
    [MaxLength(200)]
    public string? Company { get; set; }
}

public class UpdateRawMaterialDto
{
    public int SortOrder { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = null!;
    
    [MaxLength(200)]
    public string? Specification { get; set; }
    
    public int? TotalQuantity { get; set; }
    
    public int? UsedQuantity { get; set; }
    
    public int? RemainingQuantity { get; set; }
    
    [MaxLength(20)]
    public string? Unit { get; set; }
    
    [MaxLength(500)]
    public string? Remark { get; set; }
    
    [MaxLength(200)]
    public string? Supplier { get; set; }
    
    [MaxLength(200)]
    public string? Company { get; set; }
}
