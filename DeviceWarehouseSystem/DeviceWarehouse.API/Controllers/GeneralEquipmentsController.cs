using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DeviceWarehouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeneralEquipmentsController(IGeneralEquipmentService generalEquipmentService, IUserActivityLogService activityLogService) : ControllerBase
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
            var equipments = await generalEquipmentService.GetByTypeAsync(type.Value);
            return Ok(ApiResponse<IEnumerable<GeneralEquipmentDto>>.SuccessResponse(equipments));
        }
        
        var pagedResult = await generalEquipmentService.GetPagedAsync(
            pageNumber, pageSize, keyword, sortBy, sortDescending, deviceStatus, useStatus, brand);
        return Ok(ApiResponse<object>.SuccessResponse(pagedResult));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<IEnumerable<GeneralEquipmentSummaryDto>>> GetSummary(
        [FromQuery] DeviceType? type)
    {
        var summary = await generalEquipmentService.GetEquipmentSummaryAsync(type);
        return Ok(ApiResponse<IEnumerable<GeneralEquipmentSummaryDto>>.SuccessResponse(summary));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GeneralEquipmentDto>> GetById(int id)
    {
        try
        {
            var equipment = await generalEquipmentService.GetByIdAsync(id);
            return Ok(ApiResponse<GeneralEquipmentDto>.SuccessResponse(equipment));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<GeneralEquipmentDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<GeneralEquipmentDto>> Create(CreateGeneralEquipmentDto dto)
    {
        try
        {
            var equipment = await generalEquipmentService.CreateAsync(dto);
            
            // 记录操作日志
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null)
            {
                var userId = int.Parse(userIdClaim.Value);
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
                await activityLogService.CreateAsync(userId, "Create", $"创建通用设备: {equipment.DeviceName}", ipAddress, userAgent);
            }
            
            return CreatedAtAction(nameof(GetById), new { id = equipment.Id }, 
                ApiResponse<GeneralEquipmentDto>.SuccessResponse(equipment, "设备创建成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<GeneralEquipmentDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult> Update(int id, UpdateGeneralEquipmentDto dto)
    {
        try
        {
            await generalEquipmentService.UpdateAsync(id, dto);
            
            // 记录操作日志
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null)
            {
                var userId = int.Parse(userIdClaim.Value);
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
                await activityLogService.CreateAsync(userId, "Update", $"更新通用设备 (ID: {id})", ipAddress, userAgent);
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
            await generalEquipmentService.DeleteAsync(id);
            
            // 记录操作日志
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null)
            {
                var userId = int.Parse(userIdClaim.Value);
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
                await activityLogService.CreateAsync(userId, "Delete", $"删除通用设备 (ID: {id})", ipAddress, userAgent);
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
            var result = await generalEquipmentService.DeleteAllAsync();
            
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
            
            await activityLogService.CreateAsync(userId, "Delete", $"清空所有通用设备，共删除 {successCount} 台设备", ipAddress, userAgent);
            
            return Ok(ApiResponse<object>.SuccessResponse(result ?? new { successCount = 0 }, "所有设备已清空"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("batch")]
    [Authorize]
    public async Task<ActionResult> ImportBatch([FromBody] IEnumerable<CreateGeneralEquipmentDto> dtos)
    {
        try
        {
            var result = await generalEquipmentService.CreateBatchAsync(dtos);
            
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
            
            await activityLogService.CreateAsync(userId, "Create", $"批量导入通用设备，成功 {successCount} 台，失败 {errorCount} 台", ipAddress, userAgent);
            
            return Ok(ApiResponse<object>.SuccessResponse(result ?? new { successCount = 0, errorCount = 0 }, "批量导入成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("batch")]
    [Authorize]
    public async Task<ActionResult> DeleteBatch([FromBody] IEnumerable<int> ids)
    {
        try
        {
            var result = await generalEquipmentService.DeleteBatchAsync(ids);
            
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
            
            await activityLogService.CreateAsync(userId, "Delete", $"批量删除通用设备，共删除 {successCount} 台设备", ipAddress, userAgent);
            
            return Ok(ApiResponse<object>.SuccessResponse(result ?? new { successCount = 0 }, "批量删除成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("search/{keyword}")]
    public async Task<ActionResult<IEnumerable<GeneralEquipmentDto>>> Search(string keyword)
    {
        try
        {
            var equipments = await generalEquipmentService.SearchAsync(keyword);
            return Ok(ApiResponse<IEnumerable<GeneralEquipmentDto>>.SuccessResponse(equipments, "搜索成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }
}
