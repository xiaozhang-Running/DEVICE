using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceWarehouse.Domain.Entities;

public class Inventory
{
    [Key]
    public int Id { get; set; }
    
    // 关联设备
    public int? SpecialEquipmentId { get; set; }
    public SpecialEquipment? SpecialEquipment { get; set; }
    
    public int? GeneralEquipmentId { get; set; }
    public GeneralEquipment? GeneralEquipment { get; set; }
    
    // 库存信息
    public int CurrentQuantity { get; set; }
    public int AlertMinQuantity { get; set; }
    public int AlertMaxQuantity { get; set; }
    
    // 时间戳
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}
