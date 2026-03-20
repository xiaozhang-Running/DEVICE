using System.ComponentModel.DataAnnotations;
using DeviceWarehouse.Domain.Enums;

namespace DeviceWarehouse.Domain.Entities;

public class ProjectOutbound
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string OutboundNumber { get; set; } = null!;
    
    [Required]
    public DateTime OutboundDate { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string ProjectName { get; set; } = null!;
    
    [MaxLength(50)]
    public string? ProjectCode { get; set; }
    
    [MaxLength(200)]
    public string? ProjectManager { get; set; }
    
    [MaxLength(200)]
    public string? Recipient { get; set; }

    [MaxLength(50)]
    public string? OutboundType { get; set; }

    [MaxLength(100)]
    public string? ProjectTime { get; set; }

    [MaxLength(50)]
    public string? ContactPhone { get; set; }

    [MaxLength(200)]
    public string? UsageLocation { get; set; }

    public DateTime? ReturnDate { get; set; }

    [MaxLength(100)]
    public string? Handler { get; set; }

    [MaxLength(100)]
    public string? WarehouseKeeper { get; set; }

    public LogisticsMethod? LogisticsMethod { get; set; }

    public string? OutboundImages { get; set; }
    
    [MaxLength(500)]
    public string? Remark { get; set; }
    
    public int TotalQuantity { get; set; }
    
    public bool IsCompleted { get; set; } = false;
    
    public DateTime? CompletedAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    public ICollection<ProjectInboundOutbound> ProjectInboundOutbounds { get; set; } = new List<ProjectInboundOutbound>();
    
    public ICollection<ProjectOutboundItem> Items { get; set; } = new List<ProjectOutboundItem>();
}

public class ProjectOutboundItem
{
    public int Id { get; set; }
    
    [Required]
    public int OutboundId { get; set; }
    
    [Required]
    public ItemType ItemType { get; set; }
    
    [Required]
    public int ItemId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string ItemName { get; set; } = null!;

    [MaxLength(100)]
    public string? DeviceCode { get; set; }

    [MaxLength(100)]
    public string? Brand { get; set; }
    
    [MaxLength(200)]
    public string? Model { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [MaxLength(50)]
    public string? Unit { get; set; }

    [MaxLength(500)]
    public string? Accessories { get; set; }

    [MaxLength(500)]
    public string? Remark { get; set; }
    
    [MaxLength(50)]
    public string? DeviceStatus { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public ProjectOutbound? Outbound { get; set; }
}

public enum ItemType
{
    SpecialEquipment = 1,
    GeneralEquipment = 2,
    Consumable = 3,
    RawMaterial = 4
}
