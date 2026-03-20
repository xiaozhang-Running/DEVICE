namespace DeviceWarehouse.Application.DTOs;

public class InventoryDto
{
    public int Id { get; set; }
    public int? SpecialEquipmentId { get; set; }
    public int? GeneralEquipmentId { get; set; }
    public int? ConsumableId { get; set; }
    public int? RawMaterialId { get; set; }
    public int CurrentQuantity { get; set; }
    public int AlertMinQuantity { get; set; }
    public int AlertMaxQuantity { get; set; }
    public string? Unit { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? Remark { get; set; }
    
    // 关联实体信息
    public string? EquipmentName { get; set; }
    public string? EquipmentCode { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public int? UseStatus { get; set; }
    public string? Company { get; set; }
}

public class CreateInventoryDto
{
    public int? SpecialEquipmentId { get; set; }
    public int? GeneralEquipmentId { get; set; }
    public int? ConsumableId { get; set; }
    public int? RawMaterialId { get; set; }
    public int CurrentQuantity { get; set; }
    public int AlertMinQuantity { get; set; }
    public int AlertMaxQuantity { get; set; }
    public string? Unit { get; set; }
    public string? Location { get; set; }
    public string? Remark { get; set; }
}

public class UpdateInventoryDto
{
    public int CurrentQuantity { get; set; }
    public int AlertMinQuantity { get; set; }
    public int AlertMaxQuantity { get; set; }
    public string? Unit { get; set; }
    public string? Location { get; set; }
    public bool? IsActive { get; set; }
    public string? Remark { get; set; }
}

public class InventoryTransactionDto
{
    public int InventoryId { get; set; }
    public int Quantity { get; set; }
    public string? TransactionType { get; set; } // Inbound, Outbound, Transfer
    public string? Reason { get; set; }
    public string? Reference { get; set; }
}

public class InventoryReportDto
{
    public string? Category { get; set; }
    public int TotalItems { get; set; }
    public int LowStockItems { get; set; }
    public int ZeroStockItems { get; set; }
    public int TotalQuantity { get; set; }
}
