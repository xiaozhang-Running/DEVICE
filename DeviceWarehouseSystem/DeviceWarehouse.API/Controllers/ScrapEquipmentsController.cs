using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace DeviceWarehouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScrapEquipmentsController : ControllerBase
{
    private readonly IScrapEquipmentService _scrapEquipmentService;

    public ScrapEquipmentsController(IScrapEquipmentService scrapEquipmentService)
    {
        _scrapEquipmentService = scrapEquipmentService;
    }

    [HttpGet]
    public async Task<ActionResult>
GetAll(
    [FromQuery] DeviceType? type,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? keyword = null,
    [FromQuery] string? sortBy = null,
    [FromQuery] bool sortDescending = false)
    {
        if (type.HasValue)
        {
            var scrapEquipments = await _scrapEquipmentService.GetByDeviceTypeAsync(type.Value);
            return Ok(ApiResponse<IEnumerable<ScrapEquipmentDto>>.SuccessResponse(scrapEquipments));
        }
        
        var pagedResult = await _scrapEquipmentService.GetPagedAsync(
            pageNumber, pageSize, keyword, sortBy, sortDescending);
        return Ok(ApiResponse<object>.SuccessResponse(pagedResult));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ScrapEquipmentDto>> GetById(int id)
    {
        try
        {
            var scrapEquipment = await _scrapEquipmentService.GetByIdAsync(id);
            return Ok(ApiResponse<ScrapEquipmentDto>.SuccessResponse(scrapEquipment));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ScrapEquipmentDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ScrapEquipmentDto>> Create(CreateScrapEquipmentDto dto)
    {
        try
        {
            var scrapEquipment = await _scrapEquipmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = scrapEquipment.Id }, 
                ApiResponse<ScrapEquipmentDto>.SuccessResponse(scrapEquipment, "报废设备创建成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ScrapEquipmentDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateScrapEquipmentDto dto)
    {
        try
        {
            await _scrapEquipmentService.UpdateAsync(id, dto);
            return Ok(ApiResponse<string>.SuccessResponse("", "报废设备更新成功"));
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
            await _scrapEquipmentService.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "报废设备删除成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("search/{keyword}")]
    public async Task<ActionResult<IEnumerable<ScrapEquipmentDto>>>
Search(string keyword)
    {
        var scrapEquipments = await _scrapEquipmentService.SearchAsync(keyword);
        return Ok(ApiResponse<IEnumerable<ScrapEquipmentDto>>.SuccessResponse(scrapEquipments));
    }

    [HttpPost("scrap-device")]
    public async Task<ActionResult>
ScrapDevice([FromBody] ScrapDeviceRequest request)
    {
        try
        {
            await _scrapEquipmentService.ScrapDeviceAsync(
                request.DeviceCode, 
                request.ScrapReason, 
                request.ScrappedBy);
            return Ok(ApiResponse<string>.SuccessResponse("", "设备报废成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    public class ScrapDeviceRequest
    {
        public string DeviceCode { get; set; } = null!;
        public string ScrapReason { get; set; } = null!;
        public string ScrappedBy { get; set; } = null!;
    }
}