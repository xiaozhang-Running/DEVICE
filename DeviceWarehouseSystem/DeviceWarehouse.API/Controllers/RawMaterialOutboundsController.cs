using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeviceWarehouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RawMaterialOutboundsController : ControllerBase
{
    private readonly IRawMaterialOutboundService _outboundService;

    public RawMaterialOutboundsController(IRawMaterialOutboundService outboundService)
    {
        _outboundService = outboundService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RawMaterialOutboundDto>>> GetAll()
    {
        var outbounds = await _outboundService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<RawMaterialOutboundDto>>.SuccessResponse(outbounds));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RawMaterialOutboundDto>> GetById(int id)
    {
        try
        {
            var outbound = await _outboundService.GetByIdAsync(id);
            return Ok(ApiResponse<RawMaterialOutboundDto>.SuccessResponse(outbound));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RawMaterialOutboundDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<RawMaterialOutboundDto>> Create(CreateRawMaterialOutboundDto dto)
    {
        try
        {
            var outbound = await _outboundService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = outbound.Id }, 
                ApiResponse<RawMaterialOutboundDto>.SuccessResponse(outbound, "出库单创建成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RawMaterialOutboundDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateRawMaterialOutboundDto dto)
    {
        try
        {
            await _outboundService.UpdateAsync(id, dto);
            return Ok(ApiResponse<string>.SuccessResponse("", "出库单更新成功"));
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
            await _outboundService.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "出库单删除成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("exists/{number}")]
    public async Task<ActionResult<bool>> Exists(string number)
    {
        var exists = await _outboundService.ExistsAsync(number);
        return Ok(ApiResponse<bool>.SuccessResponse(exists));
    }

    [HttpGet("generate-number")]
    public async Task<ActionResult<string>> GenerateNumber()
    {
        var number = await _outboundService.GenerateOutboundNumberAsync();
        return Ok(ApiResponse<string>.SuccessResponse(number));
    }

    [HttpPut("{id}/complete")]
    public async Task<ActionResult> Complete(int id)
    {
        try
        {
            await _outboundService.CompleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "出库单完成成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }
}
