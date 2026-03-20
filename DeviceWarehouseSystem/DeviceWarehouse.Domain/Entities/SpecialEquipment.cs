using DeviceWarehouse.Domain.Enums;

namespace DeviceWarehouse.Domain.Entities;

public class SpecialEquipment
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
    public DeviceType DeviceType { get; set; }
    public string DeviceName { get; set; } = null!;
    public string DeviceCode { get; set; } = null!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? Specification { get; set; }
    public int Quantity { get; set; }
    public string? Unit { get; set; }
    public DeviceStatus DeviceStatus { get; set; }
    public UsageStatus UseStatus { get; set; }
    public string? Status { get; set; }
    public string? Company { get; set; }
    public string? Accessories { get; set; }
    public string? Remark { get; set; }
    public int? RepairStatus { get; set; }
    public string? RepairPerson { get; set; }
    public DateTime? RepairDate { get; set; }
    public string? FaultReason { get; set; }
    public string? Location { get; set; }
    public string? ProjectName { get; set; } // 使用项目名称
    public string? ProjectTime { get; set; } // 使用项目时间
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public int NameSequence { get; set; } // 设备名称下的序号
    
    public ICollection<InboundOrderItem> InboundItems { get; set; } = new List<InboundOrderItem>();
    public ICollection<OutboundOrderItem> OutboundItems { get; set; } = new List<OutboundOrderItem>();
    public ICollection<Image> Images { get; set; } = new List<Image>();
    public Inventory? Inventory { get; set; }
}

public class GeneralEquipment
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
    public DeviceType DeviceType { get; set; }
    public string DeviceName { get; set; } = null!;
    public string DeviceCode { get; set; } = null!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? Specification { get; set; }
    public int Quantity { get; set; }
    public string? Unit { get; set; }
    public DeviceStatus DeviceStatus { get; set; }
    public UsageStatus UseStatus { get; set; }
    public string? Status { get; set; }
    public string? Company { get; set; }
    public string? Accessories { get; set; }
    public string? Remark { get; set; }
    public int? RepairStatus { get; set; }
    public string? RepairPerson { get; set; }
    public DateTime? RepairDate { get; set; }
    public string? FaultReason { get; set; }
    public string? Location { get; set; }
    public string? ProjectName { get; set; } // 使用项目名称
    public string? ProjectTime { get; set; } // 使用项目时间
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public int NameSequence { get; set; } // 设备名称下的序号
    
    public ICollection<InboundOrderItem> InboundItems { get; set; } = new List<InboundOrderItem>();
    public ICollection<OutboundOrderItem> OutboundItems { get; set; } = new List<OutboundOrderItem>();
    public ICollection<Image> Images { get; set; } = new List<Image>();
    public Inventory? Inventory { get; set; }
}

public class InboundOrder
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public InboundType InboundType { get; set; }
    public string? Supplier { get; set; }
    public string? DeliveryPerson { get; set; }
    public string? Operator { get; set; }
    public string? Receiver { get; set; }
    public string? ReceiverPhone { get; set; }
    public int TotalQuantity { get; set; }
    public decimal? TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string? Remark { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // 导航属性
    public ICollection<InboundOrderItem> Items { get; set; } = new List<InboundOrderItem>();
}

public class InboundOrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int SpecialEquipmentId { get; set; }
    public int? GeneralEquipmentId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Remark { get; set; }
    
    public InboundOrder Order { get; set; } = null!;
    public SpecialEquipment? SpecialEquipment { get; set; }
    public GeneralEquipment? GeneralEquipment { get; set; }
}

public class OutboundOrder
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public OutboundType OutboundType { get; set; }
    public string? Purpose { get; set; }
    public string? ProjectName { get; set; }
    public string? Operator { get; set; }
    public int TotalQuantity { get; set; }
    public OrderStatus Status { get; set; }
    public string? Remark { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // 导航属性
    public ICollection<OutboundOrderItem> Items { get; set; } = new List<OutboundOrderItem>();
}

public class OutboundOrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int SpecialEquipmentId { get; set; }
    public int? GeneralEquipmentId { get; set; }
    public int Quantity { get; set; }
    public string? Remark { get; set; }
    
    public OutboundOrder Order { get; set; } = null!;
    public SpecialEquipment? SpecialEquipment { get; set; }
    public GeneralEquipment? GeneralEquipment { get; set; }
}



public class ApplicationUser
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Department { get; set; }
    public string? Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
