using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeviceWarehouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RawMaterialInboundsController : ControllerBase
{
    private readonly IRawMaterialInboundService _inboundService;

    public RawMaterialInboundsController(IRawMaterialInboundService inboundService)
    {
        _inboundService = inboundService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RawMaterialInboundDto>>> GetAll()
    {
        var inbounds = await _inboundService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<RawMaterialInboundDto>>.SuccessResponse(inbounds));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RawMaterialInboundDto>> GetById(int id)
    {
        try
        {
            var inbound = await _inboundService.GetByIdAsync(id);
            return Ok(ApiResponse<RawMaterialInboundDto>.SuccessResponse(inbound));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RawMaterialInboundDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<RawMaterialInboundDto>> Create(CreateRawMaterialInboundDto dto)
    {
        try
        {
            var inbound = await _inboundService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = inbound.Id }, 
                ApiResponse<RawMaterialInboundDto>.SuccessResponse(inbound, "入库单创建成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RawMaterialInboundDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateRawMaterialInboundDto dto)
    {
        try
        {
            await _inboundService.UpdateAsync(id, dto);
            return Ok(ApiResponse<string>.SuccessResponse("", "入库单更新成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}/complete")]
    public async Task<ActionResult> Complete(int id)
    {
        try
        {
            await _inboundService.CompleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "入库单完成成功"));
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
            await _inboundService.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "入库单删除成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("exists/{number}")]
    public async Task<ActionResult<bool>> Exists(string number)
    {
        var exists = await _inboundService.ExistsAsync(number);
        return Ok(ApiResponse<bool>.SuccessResponse(exists));
    }
}
