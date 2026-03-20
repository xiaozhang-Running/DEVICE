using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeviceWarehouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsumablesController : ControllerBase
{
    private readonly IConsumableService _consumableService;

    public ConsumablesController(IConsumableService consumableService)
    {
        _consumableService = consumableService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? keyword = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var pagedResult = await _consumableService.GetPagedAsync(
            pageNumber, pageSize, keyword, sortBy, sortDescending);
        return Ok(ApiResponse<object>.SuccessResponse(pagedResult));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ConsumableDto>> GetById(int id)
    {
        try
        {
            var consumable = await _consumableService.GetByIdAsync(id);
            return Ok(ApiResponse<ConsumableDto>.SuccessResponse(consumable));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ConsumableDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ConsumableDto>> Create(CreateConsumableDto dto)
    {
        try
        {
            var consumable = await _consumableService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = consumable.Id }, 
                ApiResponse<ConsumableDto>.SuccessResponse(consumable, "耗材创建成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ConsumableDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateConsumableDto dto)
    {
        try
        {
            await _consumableService.UpdateAsync(id, dto);
            return Ok(ApiResponse<string>.SuccessResponse("", "耗材更新成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await _consumableService.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "耗材删除成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("search/{keyword}")]
    public async Task<ActionResult<IEnumerable<ConsumableDto>>> Search(string keyword)
    {
        var consumables = await _consumableService.SearchAsync(keyword);
        return Ok(ApiResponse<IEnumerable<ConsumableDto>>.SuccessResponse(consumables));
    }

    [HttpDelete("all")]
    public async Task<ActionResult> DeleteAll()
    {
        try
        {
            var result = await _consumableService.DeleteAllAsync();
            return Ok(ApiResponse<object>.SuccessResponse(result, "所有耗材已清空"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("batch")]
    public async Task<ActionResult> ImportBatch([FromBody] IEnumerable<CreateConsumableDto> dtos)
    {
        try
        {
            var result = await _consumableService.CreateBatchAsync(dtos);
            return Ok(ApiResponse<object>.SuccessResponse(result, "批量导入成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }
}
