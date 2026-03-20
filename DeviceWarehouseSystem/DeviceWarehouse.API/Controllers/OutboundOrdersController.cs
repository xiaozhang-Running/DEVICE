using DeviceWarehouse.API.Controllers;
using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace DeviceWarehouse.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OutboundOrdersController : ControllerBase
{
    private readonly IOutboundOrderService _outboundOrderService;

    public OutboundOrdersController(IOutboundOrderService outboundOrderService)
    {
        _outboundOrderService = outboundOrderService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OutboundOrderDto>>> GetAll()
    {
        try
        {
            var orders = await _outboundOrderService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<OutboundOrderDto>>.SuccessResponse(orders));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<IEnumerable<OutboundOrderDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OutboundOrderDto>> GetById(int id)
    {
        try
        {
            var order = await _outboundOrderService.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound(ApiResponse<OutboundOrderDto>.ErrorResponse("出库单不存在"));
            }
            return Ok(ApiResponse<OutboundOrderDto>.SuccessResponse(order));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<OutboundOrderDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<OutboundOrderDto>> Create(CreateOutboundOrderDto dto)
    {
        try
        {
            var order = await _outboundOrderService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, ApiResponse<OutboundOrderDto>.SuccessResponse(order));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<OutboundOrderDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, CreateOutboundOrderDto dto)
    {
        try
        {
            await _outboundOrderService.UpdateAsync(id, dto);
            return Ok(ApiResponse<bool>.SuccessResponse(true));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await _outboundOrderService.DeleteAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(true));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult> Approve(int id)
    {
        try
        {
            await _outboundOrderService.CompleteAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(true));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        try
        {
            var orders = await _outboundOrderService.GetAllAsync();
            var stats = new
            {
                totalOrders = orders.Count(),
                pendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                completedOrders = orders.Count(o => o.Status == OrderStatus.Completed),
                totalQuantity = orders.Sum(o => o.TotalQuantity)
            };
            return Ok(ApiResponse<object>.SuccessResponse(stats));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}
