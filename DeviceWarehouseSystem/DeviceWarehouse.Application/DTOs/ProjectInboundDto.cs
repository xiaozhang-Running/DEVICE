using System.ComponentModel.DataAnnotations;
using DeviceWarehouse.Domain.Entities;

namespace DeviceWarehouse.Application.DTOs;

public class ProjectInboundItemDto
{
    public int Id { get; set; }
    public int InboundId { get; set; }
    public ItemType ItemType { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; } = null!;
    public string? DeviceCode { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Accessories { get; set; }
    public string? Remark { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsInbound { get; set; }
    public string? DeviceStatus { get; set; }
}

public class CreateProjectInboundItemDto
{
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
    
    public bool IsInbound { get; set; }
}

public class ProjectInboundDto
{
    public int Id { get; set; }
    public string InboundNumber { get; set; } = null!;
    public DateTime InboundDate { get; set; }
    public string ProjectName { get; set; } = null!;
    public string? ProjectCode { get; set; }
    public string? ProjectManager { get; set; }
    public string? Supplier { get; set; }
    public string? InboundType { get; set; }
    public string? ProjectTime { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? StorageLocation { get; set; }
    public string? Handler { get; set; }
    public string? WarehouseKeeper { get; set; }
    public List<string> InboundImages { get; set; } = new List<string>();
    public string? Remark { get; set; }
    public string? Operator { get; set; }
    public int TotalQuantity { get; set; }
    public string Status { get; set; } = "待入库";
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<int> ProjectOutboundIds { get; set; } = new List<int>();
    public List<string> ProjectOutboundNumbers { get; set; } = new List<string>();
    public List<ProjectInboundItemDto> Items { get; set; } = new List<ProjectInboundItemDto>();
    public DateTime CreatedAt { get; set; }
}

public class CreateProjectInboundDto
{
    [MaxLength(50)]
    public string? InboundNumber { get; set; }

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

    public List<string> InboundImages { get; set; } = new List<string>();

    [MaxLength(500)]
    public string? Remark { get; set; }

    public List<int> ProjectOutboundIds { get; set; } = new List<int>();

    [Required]
    public List<CreateProjectInboundItemDto> Items { get; set; } = new List<CreateProjectInboundItemDto>();
}

public class UpdateProjectInboundDto
{
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

    public List<string> InboundImages { get; set; } = new List<string>();

    [MaxLength(500)]
    public string? Remark { get; set; }

    public List<int> ProjectOutboundIds { get; set; } = new List<int>();

    [Required]
    public List<CreateProjectInboundItemDto> Items { get; set; } = new List<CreateProjectInboundItemDto>();
}
