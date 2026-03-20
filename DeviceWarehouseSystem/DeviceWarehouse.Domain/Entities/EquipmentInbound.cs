using System.ComponentModel.DataAnnotations;
using DeviceWarehouse.Domain.Enums;

namespace DeviceWarehouse.Domain.Entities;

public class EquipmentInbound
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string InboundNumber { get; set; } = null!;
    
    [Required]
    public DateTime InboundDate { get; set; }
    
    [Required]
    public DeviceType EquipmentType { get; set; }
    
    [MaxLength(200)]
    public string? DeliveryPerson { get; set; }
    
    [MaxLength(100)]
    public string? Operator { get; set; }
    
    [MaxLength(500)]
    public string? Remark { get; set; }
    
    [MaxLength(20)]
    public string Status { get; set; } = "待完成"; // 待完成, 已完成
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    public ICollection<EquipmentInboundItem> Items { get; set; } = new List<EquipmentInboundItem>();
}
