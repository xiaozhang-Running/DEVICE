using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeviceWarehouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _service;

    public InventoryController(IInventoryService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryDto>> GetById(int id)
    {
        try
        {
            var inventory = await _service.GetByIdAsync(id);
            return Ok(inventory);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("special/{specialEquipmentId}")]
    public async Task<ActionResult<InventoryDto>> GetBySpecialEquipmentId(int specialEquipmentId)
    {
        try
        {
            var inventory = await _service.GetBySpecialEquipmentIdAsync(specialEquipmentId);
            return Ok(inventory);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("general/{generalEquipmentId}")]
    public async Task<ActionResult<InventoryDto>> GetByGeneralEquipmentId(int generalEquipmentId)
    {
        try
        {
            var inventory = await _service.GetByGeneralEquipmentIdAsync(generalEquipmentId);
            return Ok(inventory);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("consumable/{consumableId}")]
    public async Task<ActionResult<InventoryDto>> GetByConsumableId(int consumableId)
    {
        try
        {
            var inventory = await _service.GetByConsumableIdAsync(consumableId);
            return Ok(inventory);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("raw/{rawMaterialId}")]
    public async Task<ActionResult<InventoryDto>> GetByRawMaterialId(int rawMaterialId)
    {
        try
        {
            var inventory = await _service.GetByRawMaterialIdAsync(rawMaterialId);
            return Ok(inventory);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetAll([FromQuery] int? category)
    {
        var inventories = await _service.GetAllAsync(category);
        return Ok(inventories);
    }

    [HttpGet("lowstock")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetLowStock()
    {
        var inventories = await _service.GetLowStockAsync();
        return Ok(inventories);
    }

    [HttpGet("lowstock/{threshold}")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetLowStock(int threshold)
    {
        var inventories = await _service.GetLowStockAsync(threshold);
        return Ok(inventories);
    }

    [HttpGet("zerostock")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetZeroStock()
    {
        var inventories = await _service.GetZeroStockAsync();
        return Ok(inventories);
    }

    [HttpGet("alerts/low-stock")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetLowStockAlerts()
    {
        var inventories = await _service.GetLowStockAsync();
        return Ok(inventories);
    }

    [HttpGet("alerts/zero-stock")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetZeroStockAlerts()
    {
        var inventories = await _service.GetZeroStockAsync();
        return Ok(inventories);
    }

    [HttpPost]
    public async Task<ActionResult<InventoryDto>> Create([FromBody] CreateInventoryDto dto)
    {
        var inventory = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = inventory.Id }, inventory);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInventoryDto dto)
    {
        try
        {
            await _service.UpdateAsync(id, dto);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{inventoryId}/add")]
    public async Task<ActionResult<InventoryDto>> AddStock(int inventoryId, [FromBody] InventoryTransactionDto dto)
    {
        try
        {
            var inventory = await _service.AddStockAsync(inventoryId, dto.Quantity, dto.Reason ?? string.Empty, dto.Reference ?? string.Empty);
            return Ok(inventory);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{inventoryId}/remove")]
    public async Task<ActionResult<InventoryDto>> RemoveStock(int inventoryId, [FromBody] InventoryTransactionDto dto)
    {
        try
        {
            var inventory = await _service.RemoveStockAsync(inventoryId, dto.Quantity, dto.Reason ?? string.Empty, dto.Reference ?? string.Empty);
            return Ok(inventory);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("transactions")]
    public async Task<ActionResult<InventoryTransactionDto>> CreateTransaction([FromBody] InventoryTransactionDto dto)
    {
        try
        {
            var transaction = await _service.CreateTransactionAsync(dto);
            return Ok(transaction);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<IEnumerable<InventoryTransactionDto>>> GetTransactions([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var transactions = await _service.GetTransactionsAsync(startDate, endDate);
        return Ok(transactions);
    }

    [HttpGet("{inventoryId}/transactions")]
    public async Task<ActionResult<IEnumerable<InventoryTransactionDto>>> GetTransactions(int inventoryId)
    {
        var transactions = await _service.GetTransactionsAsync(inventoryId);
        return Ok(transactions);
    }

    [HttpGet("transactions/range")]
    public async Task<ActionResult<IEnumerable<InventoryTransactionDto>>> GetTransactionsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var transactions = await _service.GetTransactionsByDateRangeAsync(startDate, endDate);
        return Ok(transactions);
    }

    [HttpGet("report")]
    public async Task<ActionResult<InventoryReportDto>> GetInventoryReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var report = await _service.GetInventoryReportAsync(startDate, endDate);
        return Ok(report);
    }

    [HttpGet("report/category")]
    public async Task<ActionResult<IEnumerable<InventoryReportDto>>> GetCategoryReport()
    {
        var reports = await _service.GetCategoryReportAsync();
        return Ok(reports);
    }
}