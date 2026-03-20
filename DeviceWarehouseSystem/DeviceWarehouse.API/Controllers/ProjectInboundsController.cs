using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeviceWarehouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectInboundsController : ControllerBase
{
    private readonly IProjectInboundService _inboundService;

    public ProjectInboundsController(IProjectInboundService inboundService)
    {
        _inboundService = inboundService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectInboundDto>>> GetAll()
    {
        var inbounds = await _inboundService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<ProjectInboundDto>>.SuccessResponse(inbounds));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectInboundDto>> GetById(int id)
    {
        try
        {
            var inbound = await _inboundService.GetByIdAsync(id);
            return Ok(ApiResponse<ProjectInboundDto>.SuccessResponse(inbound));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ProjectInboundDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProjectInboundDto>> Create(CreateProjectInboundDto dto)
    {
        try
        {
            var inbound = await _inboundService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = inbound.Id }, 
                ApiResponse<ProjectInboundDto>.SuccessResponse(inbound, "入库单创建成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ProjectInboundDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateProjectInboundDto dto)
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

    [HttpPost("{id}/complete")]
    public async Task<ActionResult> Complete(int id)
    {
        try
        {
            await _inboundService.CompleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "入库单已完成"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{id}/partial-inbound")]
    public async Task<ActionResult> PartialInbound(int id)
    {
        try
        {
            await _inboundService.PartialInboundAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "部分入库成功"));
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

    [HttpGet("available-outbounds")]
    public async Task<ActionResult<IEnumerable<ProjectOutboundDto>>>
    GetAvailableProjectOutbounds()
    {
        var outbounds = await _inboundService.GetAvailableProjectOutbounds();
        return Ok(ApiResponse<IEnumerable<ProjectOutboundDto>>.SuccessResponse(outbounds));
    }

    [HttpGet("outbound/{id}")]
    public async Task<ActionResult<ProjectOutboundDto>> GetProjectOutboundById(int id)
    {
        try
        {
            var outbound = await _inboundService.GetProjectOutboundByIdAsync(id);
            return Ok(ApiResponse<ProjectOutboundDto>.SuccessResponse(outbound));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ProjectOutboundDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("uninbound-items/{outboundId}")]
    public async Task<ActionResult<IEnumerable<ProjectOutboundItemDto>>> GetUninboundItemsByOutboundId(int outboundId)
    {
        try
        {
            var items = await _inboundService.GetUninboundItemsByOutboundIdAsync(outboundId);
            return Ok(ApiResponse<IEnumerable<ProjectOutboundItemDto>>.SuccessResponse(items));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<IEnumerable<ProjectOutboundItemDto>>.ErrorResponse(ex.Message));
        }
    }
}
