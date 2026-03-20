using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeviceWarehouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RawMaterialsController : ControllerBase
{
    private readonly IRawMaterialService _rawMaterialService;

    public RawMaterialsController(IRawMaterialService rawMaterialService)
    {
        _rawMaterialService = rawMaterialService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RawMaterialDto>>> GetAll()
    {
        var materials = await _rawMaterialService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<RawMaterialDto>>.SuccessResponse(materials));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RawMaterialDto>> GetById(int id)
    {
        try
        {
            var material = await _rawMaterialService.GetByIdAsync(id);
            return Ok(ApiResponse<RawMaterialDto>.SuccessResponse(material));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RawMaterialDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<RawMaterialDto>> Create(CreateRawMaterialDto dto)
    {
        try
        {
            var material = await _rawMaterialService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = material.Id }, 
                ApiResponse<RawMaterialDto>.SuccessResponse(material, "原材料创建成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RawMaterialDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateRawMaterialDto dto)
    {
        try
        {
            await _rawMaterialService.UpdateAsync(id, dto);
            return Ok(ApiResponse<string>.SuccessResponse("", "原材料更新成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("batch")]
    public async Task<ActionResult> ImportBatch([FromBody] IEnumerable<CreateRawMaterialDto> dtos)
    {
        try
        {
            var result = await _rawMaterialService.CreateBatchAsync(dtos);
            return Ok(ApiResponse<object>.SuccessResponse(result, "批量导入成功"));
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
            await _rawMaterialService.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "原材料删除成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("all")]
    public async Task<ActionResult> DeleteAll()
    {
        try
        {
            var result = await _rawMaterialService.DeleteAllAsync();
            return Ok(ApiResponse<object>.SuccessResponse(result, "清空成功"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("search/{keyword}")]
    public async Task<ActionResult> Search(string keyword)
    {
        try
        {
            var materials = await _rawMaterialService.SearchAsync(keyword);
            return Ok(ApiResponse<IEnumerable<RawMaterialDto>>.SuccessResponse(materials));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }
}
