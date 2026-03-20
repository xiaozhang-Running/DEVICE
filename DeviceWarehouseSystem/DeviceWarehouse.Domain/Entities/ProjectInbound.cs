using System.ComponentModel.DataAnnotations;

namespace DeviceWarehouse.Domain.Entities;

public class ProjectInbound
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string InboundNumber { get; set; } = null!;
    
    [Required]
    public DateTime InboundDate { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string ProjectName { get; set; } = null!;
    
    [MaxLength(50)]
    public string? ProjectCode { get; set; }
    
    [MaxLength(200)]
    public string? ProjectManager { get; set; }
    
    [MaxLength(200)]
    public string? Supplier { get; set; }

    [MaxLength(50)]
    public string? InboundType { get; set; }

    [MaxLength(100)]
    public string? ProjectTime { get; set; }

    [MaxLength(50)]
    public string? ContactPhone { get; set; }

    [MaxLength(200)]
    public string? StorageLocation { get; set; }

    [MaxLength(100)]
    public string? Handler { get; set; }

    [MaxLength(100)]
    public string? WarehouseKeeper { get; set; }

    public string? InboundImages { get; set; }
    
    [MaxLength(500)]
    public string? Remark { get; set; }
    
    public int TotalQuantity { get; set; }    
    public string Status { get; set; } = "待入库"; // 待入库、部分入库、已完成
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    public ICollection<ProjectInboundOutbound> ProjectInboundOutbounds { get; set; } = new List<ProjectInboundOutbound>();
    
    public ICollection<ProjectInboundItem> Items { get; set; } = new List<ProjectInboundItem>();
}

public class ProjectInboundOutbound
{
    [Required]
    public int ProjectInboundId { get; set; }
    public ProjectInbound? ProjectInbound { get; set; }
    
    [Required]
    public int ProjectOutboundId { get; set; }
    public ProjectOutbound? ProjectOutbound { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class ProjectInboundItem
{
    public int Id { get; set; }
    
    [Required]
    public int InboundId { get; set; }
    
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
    
    public bool IsInbound { get; set; } = false;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public ProjectInbound? Inbound { get; set; }
}
