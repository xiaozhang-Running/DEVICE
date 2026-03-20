using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceWarehouse.Domain.Entities;

public class InventoryTransaction
{
    [Key]
    public int Id { get; set; }
    
    // 关联库存
    public int InventoryId { get; set; }
    public required Inventory Inventory { get; set; }
    
    // 交易信息
    public int Quantity { get; set; } // 正数为入库，负数为出库
    public required string TransactionType { get; set; } // Inbound, Outbound, Transfer, Adjustment
    public string? Reason { get; set; }
    public string? Reference { get; set; } // 参考号，如订单号、入库单号等
    
    // 时间戳
    public DateTime TransactionDate { get; set; } = DateTime.Now;
    
    // 操作人员
    public string? Operator { get; set; }
    
    // 备注
    public string? Remark { get; set; }
}
