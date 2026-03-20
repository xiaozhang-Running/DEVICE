using DeviceWarehouse.Domain.Interfaces;
using DeviceWarehouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DeviceWarehouse.Infrastructure.Repositories;

public class DeviceTemplateRepository : IDeviceTemplateRepository
{
    private readonly ApplicationDbContext _context;

    public DeviceTemplateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DeviceTemplateDto>> GetSpecialEquipmentTemplatesAsync()
    {
        var templates = await _context.SpecialEquipments
            .GroupBy(e => new { e.DeviceName, e.Brand, e.Model, e.Specification, e.Unit })
            .Select(g => new DeviceTemplateDto
            {
                DeviceName = g.Key.DeviceName,
                Brand = g.Key.Brand,
                Model = g.Key.Model,
                Specification = g.Key.Specification,
                Unit = g.Key.Unit,
                Count = g.Count()
            })
            .OrderByDescending(t => t.Count)
            .Take(50)
            .ToListAsync();

        return templates;
    }

    public async Task<List<DeviceTemplateDto>> GetGeneralEquipmentTemplatesAsync()
    {
        var templates = await _context.GeneralEquipments
            .GroupBy(e => new { e.DeviceName, e.Brand, e.Model, e.Specification, e.Unit })
            .Select(g => new DeviceTemplateDto
            {
                DeviceName = g.Key.DeviceName,
                Brand = g.Key.Brand,
                Model = g.Key.Model,
                Specification = g.Key.Specification,
                Unit = g.Key.Unit,
                Count = g.Count()
            })
            .OrderByDescending(t => t.Count)
            .Take(50)
            .ToListAsync();

        return templates;
    }

    public async Task<List<ConsumableTemplateDto>> GetConsumableTemplatesAsync()
    {
        var templates = await _context.Consumables
            .GroupBy(e => new { e.Name, e.Brand, e.ModelSpecification, e.Unit })
            .Select(g => new ConsumableTemplateDto
            {
                Name = g.Key.Name,
                Brand = g.Key.Brand,
                ModelSpecification = g.Key.ModelSpecification,
                Unit = g.Key.Unit,
                Count = g.Count()
            })
            .OrderByDescending(t => t.Count)
            .Take(50)
            .ToListAsync();

        return templates;
    }
}
