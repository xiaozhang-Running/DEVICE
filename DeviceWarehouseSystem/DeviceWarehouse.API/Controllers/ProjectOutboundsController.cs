using System;
using System.Collections.Generic;
using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeviceWarehouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectOutboundsController : ControllerBase
{
    private readonly IProjectOutboundService _outboundService;

    public ProjectOutboundsController(IProjectOutboundService outboundService)
    {
        _outboundService = outboundService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectOutboundDto>>> GetAll()
    {
        try
        {
            var outbounds = await _outboundService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ProjectOutboundDto>>.SuccessResponse(outbounds));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] GetAll failed: {ex.Message}");
            Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[ERROR] InnerException: {ex.InnerException.Message}");
            }
            return BadRequest(ApiResponse<IEnumerable<ProjectOutboundDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectOutboundDto>> GetById(int id)
    {
        try
        {
            var outbound = await _outboundService.GetByIdAsync(id);
            return Ok(ApiResponse<ProjectOutboundDto>.SuccessResponse(outbound));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ProjectOutboundDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProjectOutboundDto>> Create(CreateProjectOutboundDto dto)
    {
        try
        {
            var outbound = await _outboundService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = outbound.Id }, 
                ApiResponse<ProjectOutboundDto>.SuccessResponse(outbound, "项目出库单创建成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ProjectOutboundDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateProjectOutboundDto dto)
    {
        try
        {
            await _outboundService.UpdateAsync(id, dto);
            return Ok(ApiResponse<string>.SuccessResponse("", "项目出库单更新成功"));
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
            return Ok(ApiResponse<string>.SuccessResponse("", "项目出库单删除成功"));
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
            await _outboundService.CompleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "出库单完成成功，库存已更新"));
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

    [HttpGet("available-items")]
    public async Task<ActionResult<IEnumerable<AvailableItemDto>>> GetAvailableItems([FromQuery] string? keyword)
    {
        try
        {
            var items = await _outboundService.GetAvailableItemsAsync(keyword);
            return Ok(ApiResponse<IEnumerable<AvailableItemDto>>.SuccessResponse(items));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] GetAvailableItems failed: {ex.Message}");
            Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[ERROR] InnerException: {ex.InnerException.Message}");
            }
            return BadRequest(ApiResponse<IEnumerable<AvailableItemDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("available-items/paged")]
    public async Task<ActionResult<AvailableItemsResponseDto>> GetAvailableItemsPaged([FromBody] AvailableItemsRequestDto request)
    {
        try
        {
            var result = await _outboundService.GetAvailableItemsPagedAsync(request);
            return Ok(ApiResponse<AvailableItemsResponseDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetAvailableItemsPaged: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return BadRequest(ApiResponse<AvailableItemsResponseDto>.ErrorResponse($"{ex.Message}\n{ex.StackTrace}"));
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProjectOutboundDto>>> SearchOutbounds([FromQuery] string? keyword = null)
    {
        try
        {
            var outbounds = await _outboundService.SearchOutboundsAsync(keyword ?? "");
            return Ok(ApiResponse<IEnumerable<ProjectOutboundDto>>.SuccessResponse(outbounds));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<IEnumerable<ProjectOutboundDto>>.ErrorResponse(ex.Message));
        }
    }
}
