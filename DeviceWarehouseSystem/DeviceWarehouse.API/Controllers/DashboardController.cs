using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Application.Services;
using DeviceWarehouse.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeviceWarehouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(ApplicationDbContext context, IConsumableService consumableService, IInventoryService inventoryService, IProjectInboundService projectInboundService, IProjectOutboundService projectOutboundService, IScrapEquipmentService scrapEquipmentService) : ControllerBase
{
    private readonly ApplicationDbContext _context = context;
    private readonly IConsumableService _consumableService = consumableService;
    private readonly IInventoryService _inventoryService = inventoryService;
    private readonly IProjectInboundService _projectInboundService = projectInboundService;
    private readonly IProjectOutboundService _projectOutboundService = projectOutboundService;
    private readonly IScrapEquipmentService _scrapEquipmentService = scrapEquipmentService;

    [HttpGet("overview")]
    public async Task<ActionResult<DashboardOverviewDto>> GetOverview()
    {
        var overview = new DashboardOverviewDto
        {
            TotalConsumables = await _context.Consumables.CountAsync(),
            TotalGeneralEquipments = await _context.GeneralEquipments.CountAsync(),
            TotalSpecialEquipments = await _context.SpecialEquipments.CountAsync(),
            TotalRawMaterials = await _context.RawMaterials.CountAsync(),
            TotalScrapEquipments = await _context.ScrapEquipments.CountAsync(),
            TotalProjectInbounds = await _context.ProjectInbounds.CountAsync(),
            TotalProjectOutbounds = await _context.ProjectOutbounds.CountAsync(),
            LowStockItems = await _context.Consumables.CountAsync(c => c.RemainingQuantity <= 10), // 暂时设置为10作为最低库存阈值
            RecentInbounds = await _context.ProjectInbounds
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new RecentActivityDto
                {
                    Id = p.Id,
                    Type = "入库",
                    ProjectName = p.ProjectName,
                    CreatedAt = p.CreatedAt,
                    Status = p.Status
                })
                .ToListAsync(),
            RecentOutbounds = await _context.ProjectOutbounds
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new RecentActivityDto
                {
                    Id = p.Id,
                    Type = "出库",
                    ProjectName = p.ProjectName,
                    CreatedAt = p.CreatedAt,
                    Status = p.IsCompleted ? "已完成" : "进行中"
                })
                .ToListAsync()
        };

        return Ok(overview);
    }

    [HttpGet("inventory-status")]
    public async Task<ActionResult<InventoryStatusDto>> GetInventoryStatus()
    {
        var lowStockConsumables = await _context.Consumables
            .Where(c => c.RemainingQuantity <= 10) // 暂时设置为10作为最低库存阈值
            .Select(c => new LowStockItemDto
            {
                Id = c.Id,
                Name = c.Name,
                CurrentQuantity = c.RemainingQuantity,
                MinimumStockLevel = 10, // 暂时设置为10作为最低库存阈值
                Location = c.Location
            })
            .ToListAsync();

        var status = new InventoryStatusDto
        {
            LowStockItems = lowStockConsumables,
            TotalLowStockItems = lowStockConsumables.Count
        };

        return Ok(status);
    }

    [HttpGet("equipment-status")]
    public async Task<ActionResult<EquipmentStatusDto>> GetEquipmentStatus()
    {
        var totalGeneral = await _context.GeneralEquipments.CountAsync();
        var totalSpecial = await _context.SpecialEquipments.CountAsync();
        var totalScrap = await _context.ScrapEquipments.CountAsync();

        var status = new EquipmentStatusDto
        {
            TotalEquipment = totalGeneral + totalSpecial,
            GeneralEquipmentCount = totalGeneral,
            SpecialEquipmentCount = totalSpecial,
            ScrapEquipmentCount = totalScrap
        };

        return Ok(status);
    }
}

public class DashboardOverviewDto
{
    public int TotalConsumables { get; set; }
    public int TotalGeneralEquipments { get; set; }
    public int TotalSpecialEquipments { get; set; }
    public int TotalRawMaterials { get; set; }
    public int TotalScrapEquipments { get; set; }
    public int TotalProjectInbounds { get; set; }
    public int TotalProjectOutbounds { get; set; }
    public int LowStockItems { get; set; }
    public List<RecentActivityDto>? RecentInbounds { get; set; }
    public List<RecentActivityDto>? RecentOutbounds { get; set; }
}

public class RecentActivityDto
{
    public int Id { get; set; }
    public string? Type { get; set; }
    public string? ProjectName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Status { get; set; }
}

public class InventoryStatusDto
{
    public List<LowStockItemDto>? LowStockItems { get; set; }
    public int TotalLowStockItems { get; set; }
}

public class LowStockItemDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int CurrentQuantity { get; set; }
    public int MinimumStockLevel { get; set; }
    public string? Location { get; set; }
}

public class EquipmentStatusDto
{
    public int TotalEquipment { get; set; }
    public int GeneralEquipmentCount { get; set; }
    public int SpecialEquipmentCount { get; set; }
    public int ScrapEquipmentCount { get; set; }
}