namespace DeviceWarehouse.Domain.Interfaces;

public interface IDeviceTemplateRepository
{
    Task<List<DeviceTemplateDto>> GetSpecialEquipmentTemplatesAsync();
    Task<List<DeviceTemplateDto>> GetGeneralEquipmentTemplatesAsync();
    Task<List<ConsumableTemplateDto>> GetConsumableTemplatesAsync();
}

public class DeviceTemplateDto
{
    public string DeviceName { get; set; } = null!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Specification { get; set; }
    public string? Unit { get; set; }
    public int Count { get; set; }
}

public class ConsumableTemplateDto
{
    public string Name { get; set; } = null!;
    public string? Brand { get; set; }
    public string? ModelSpecification { get; set; }
    public string? Unit { get; set; }
    public int Count { get; set; }
}
