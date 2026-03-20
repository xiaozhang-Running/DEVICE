using DeviceWarehouse.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeviceWarehouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DataController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpDelete("clear-all")]
    public async Task<ActionResult>
ClearAllData()
    {
        try
        {
            // 按照依赖关系顺序删除数据
            // 1. 删除多对多关系表
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ProjectInboundOutbound");
            
            // 2. 删除子表数据
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM RawMaterialInboundItem");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM RawMaterialOutboundItem");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM EquipmentInboundItem");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ProjectOutboundItem");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ProjectInboundItem");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Images");
            
            // 3. 删除主表数据
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM RawMaterialInbound");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM RawMaterialOutbound");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM EquipmentInbound");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ProjectOutbound");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ProjectInbound");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ScrapEquipment");
            
            // 4. 删除库存表数据
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Inventories");
            
            // 5. 删除设备和物料表数据
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM SpecialEquipment");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM GeneralEquipment");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Consumables");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM RawMaterial");
            
            // 6. 重置自增主键
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('SpecialEquipment', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('GeneralEquipment', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Consumables', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('RawMaterial', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Inventories', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('RawMaterialInbound', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('RawMaterialOutbound', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('EquipmentInbound', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('ProjectOutbound', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('ProjectInbound', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('ScrapEquipment', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Images', RESEED, 0)");
            
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "所有数据已成功清除" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "清除数据失败: " + ex.Message });
        }
    }
}
