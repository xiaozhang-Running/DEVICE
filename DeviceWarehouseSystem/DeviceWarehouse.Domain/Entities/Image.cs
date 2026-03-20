namespace DeviceWarehouse.Domain.Entities;

public class Image
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public byte[] Data { get; set; } = null!;
    public string Url { get; set; } = null!;
    public int? SpecialEquipmentId { get; set; }
    public int? GeneralEquipmentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public SpecialEquipment? SpecialEquipment { get; set; }
    public GeneralEquipment? GeneralEquipment { get; set; }
}