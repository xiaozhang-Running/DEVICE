using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DeviceWarehouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpecialEquipmentsController(ISpecialEquipmentService specialEquipmentService, IUserActivityLogService activityLogService) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] DeviceType? type,
        [FromQuery] DeviceStatus? deviceStatus,
        [FromQuery] UsageStatus? useStatus,
        [FromQuery] string? brand,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? keyword = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        if (type.HasValue)
        {
            var specialEquipments = await specialEquipmentService.GetByTypeAsync(type.Value);
            return Ok(ApiResponse<IEnumerable<SpecialEquipmentDto>>.SuccessResponse(specialEquipments));
        }
        
        var pagedResult = await specialEquipmentService.GetPagedAsync(
            pageNumber, pageSize, keyword, sortBy, sortDescending, deviceStatus, useStatus, brand);
        return Ok(ApiResponse<object>.SuccessResponse(pagedResult));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<IEnumerable<SpecialEquipmentSummaryDto>>> GetSummary(
        [FromQuery] DeviceType? type)
    {
        var summary = await specialEquipmentService.GetSpecialEquipmentSummaryAsync(type);
        return Ok(ApiResponse<IEnumerable<SpecialEquipmentSummaryDto>>.SuccessResponse(summary));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SpecialEquipmentDto>> GetById(int id)
    {
        try
        {
            var specialEquipment = await specialEquipmentService.GetByIdAsync(id);
            return Ok(ApiResponse<SpecialEquipmentDto>.SuccessResponse(specialEquipment));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<SpecialEquipmentDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<SpecialEquipmentDto>> Create(CreateSpecialEquipmentDto dto)
    {
        try
        {
            var specialEquipment = await specialEquipmentService.CreateAsync(dto);
            
            // 记录操作日志
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null)
            {
                var userId = int.Parse(userIdClaim.Value);
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
                await activityLogService.CreateAsync(userId, "Create", $"创建专用设备: {specialEquipment.DeviceName}", ipAddress, userAgent);
            }
            
            return CreatedAtAction(nameof(GetById), new { id = specialEquipment.Id }, 
                ApiResponse<SpecialEquipmentDto>.SuccessResponse(specialEquipment, "设备创建成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<SpecialEquipmentDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult> Update(int id, UpdateSpecialEquipmentDto dto)
    {
        try
        {
            await specialEquipmentService.UpdateAsync(id, dto);
            
            // 记录操作日志
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                // 如果没有UserId声明，使用默认值1（管理员）
                await activityLogService.CreateAsync(1, "Update", $"更新专用设备 (ID: {id})", null, null);
            }
            else
            {
                var userId = int.Parse(userIdClaim.Value);
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
                await activityLogService.CreateAsync(userId, "Update", $"更新专用设备 (ID: {id})", ipAddress, userAgent);
            }
            
            return Ok(ApiResponse<string>.SuccessResponse("", "设备更新成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await specialEquipmentService.DeleteAsync(id);
            
            // 记录操作日志
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null)
            {
                var userId = int.Parse(userIdClaim.Value);
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
                await activityLogService.CreateAsync(userId, "Delete", $"删除专用设备 (ID: {id})", ipAddress, userAgent);
            }
            
            return Ok(ApiResponse<string>.SuccessResponse("", "设备删除成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteAll()
    {
        try
        {
            var result = await specialEquipmentService.DeleteAllAsync();
            
            // 记录操作日志
            var userIdClaim = User.FindFirst("UserId");
            int? userId = null;
            if (userIdClaim != null)
            {
                userId = int.Parse(userIdClaim.Value);
            }
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
            
            // 检查result对象是否包含successCount属性
            int successCount = 0;
            if (result != null)
            {
                var resultType = result.GetType();
                var successCountProperty = resultType.GetProperty("successCount");
                if (successCountProperty != null)
                {
                    var value = successCountProperty.GetValue(result);
                    if (value != null)
                    {
                        successCount = (int)value;
                    }
                }
            }
            
            await activityLogService.CreateAsync(userId, "Delete", $"清空所有专用设备，共删除 {successCount} 台设备", ipAddress, userAgent);
            
            return Ok(ApiResponse<object>.SuccessResponse(result ?? new { successCount = 0 }, "所有设备已清空"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("batch")]
    [Authorize]
    public async Task<ActionResult> ImportBatch([FromBody] IEnumerable<CreateSpecialEquipmentDto> dtos)
    {
        try
        {
            var result = await specialEquipmentService.CreateBatchAsync(dtos);
            
            // 记录操作日志
            var userIdClaim = User.FindFirst("UserId");
            int? userId = null;
            if (userIdClaim != null)
            {
                userId = int.Parse(userIdClaim.Value);
            }
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
            
            // 检查result对象是否包含successCount和errorCount属性
            int successCount = 0;
            int errorCount = 0;
            if (result != null)
            {
                var resultType = result.GetType();
                var successCountProperty = resultType.GetProperty("successCount");
                if (successCountProperty != null)
                {
                    var value = successCountProperty.GetValue(result);
                    if (value != null)
                    {
                        successCount = (int)value;
                    }
                }
                var errorCountProperty = resultType.GetProperty("errorCount");
                if (errorCountProperty != null)
                {
                    var errorValue = errorCountProperty.GetValue(result);
                    if (errorValue != null)
                    {
                        errorCount = (int)errorValue;
                    }
                }
            }
            
            await activityLogService.CreateAsync(userId, "Create", $"批量导入专用设备，成功 {successCount} 台，失败 {errorCount} 台", ipAddress, userAgent);
            
            return Ok(ApiResponse<object>.SuccessResponse(result ?? new { successCount = 0, errorCount = 0 }, "批量导入成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("search/{keyword}")]
    public async Task<ActionResult<IEnumerable<SpecialEquipmentDto>>> Search(string keyword)
    {
        var specialEquipments = await specialEquipmentService.SearchAsync(keyword);
        return Ok(ApiResponse<IEnumerable<SpecialEquipmentDto>>.SuccessResponse(specialEquipments));
    }

    // 暂时移除这些端点，因为存储过程创建失败
    // 后续可以通过EF Core实现这些功能

    [HttpGet("inventory-summary")]
    public async Task<ActionResult> GetInventorySummary(
        [FromQuery] int? deviceType, 
        [FromQuery] int? status)
    {
        try
        {
            var summary = await specialEquipmentService.GetInventorySummaryAsync(deviceType, status);
            return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(summary));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("outbound-report")]
    public async Task<ActionResult> GetOutboundReport(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        try
        {
            var report = await specialEquipmentService.GetOutboundReportAsync(startDate, endDate);
            return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(report));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("usage-analysis")]
    public async Task<ActionResult> GetDeviceUsageAnalysis(
        [FromQuery] int months = 6)
    {
        try
        {
            var analysis = await specialEquipmentService.GetDeviceUsageAnalysisAsync(months);
            return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(analysis));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }
}
