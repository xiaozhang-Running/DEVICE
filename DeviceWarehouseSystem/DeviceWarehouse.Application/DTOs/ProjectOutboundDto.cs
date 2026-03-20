using System.ComponentModel.DataAnnotations;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Enums;

namespace DeviceWarehouse.Application.DTOs;

public class ProjectOutboundItemDto
{
    public int Id { get; set; }
    public int OutboundId { get; set; }
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
    public string? DeviceStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateProjectOutboundItemDto
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
}

public class ProjectOutboundDto
{
    public int Id { get; set; }
    public string OutboundNumber { get; set; } = null!;
    public DateTime OutboundDate { get; set; }
    public string ProjectName { get; set; } = null!;
    public string? ProjectCode { get; set; }
    public string? ProjectManager { get; set; }
    public string? Recipient { get; set; }
    public string? Company { get; set; }
    public string? Department { get; set; }
    public string? OutboundType { get; set; }
    public string? ProjectTime { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? UsageLocation { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? Handler { get; set; }
    public string? WarehouseKeeper { get; set; }
    public LogisticsMethod? LogisticsMethod { get; set; }
    public List<string> OutboundImages { get; set; } = new List<string>();
    public string? Remark { get; set; }
    public string? Operator { get; set; }
    public int TotalQuantity { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? InboundStatus { get; set; }
    public List<ProjectOutboundItemDto> Items { get; set; } = new List<ProjectOutboundItemDto>();
    public DateTime CreatedAt { get; set; }
}

public class CreateProjectOutboundDto
{
    [MaxLength(50)]
    public string? OutboundNumber { get; set; }

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

    public List<string> OutboundImages { get; set; } = new List<string>();

    [MaxLength(500)]
    public string? Remark { get; set; }

    [Required]
    public List<CreateProjectOutboundItemDto> Items { get; set; } = new List<CreateProjectOutboundItemDto>();
}

public class UpdateProjectOutboundDto
{
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

    public List<string> OutboundImages { get; set; } = new List<string>();

    [MaxLength(500)]
    public string? Remark { get; set; }

    [Required]
    public List<CreateProjectOutboundItemDto> Items { get; set; } = new List<CreateProjectOutboundItemDto>();
}

public class AvailableItemDto
{
    public int id { get; set; }
    public ItemType itemType { get; set; }
    public string itemTypeName { get; set; } = null!;
    public string name { get; set; } = null!;
    public string? brand { get; set; }
    public string? model { get; set; }
    public int availableQuantity { get; set; }
    public string? unit { get; set; }
    public string? location { get; set; }
    public string? company { get; set; }
    public string? deviceCode { get; set; }
    public string? accessories { get; set; }
    public string? remark { get; set; }
    public string? deviceStatus { get; set; }
}

public class AvailableItemsRequestDto
{
    public string? Keyword { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int? ItemType { get; set; }
}

public class AvailableItemsResponseDto
{
    public List<AvailableItemDto> items { get; set; } = new();
    public int totalCount { get; set; }
    public int pageNumber { get; set; }
    public int pageSize { get; set; }
    public int totalPages => (int)Math.Ceiling((double)totalCount / pageSize);
}
