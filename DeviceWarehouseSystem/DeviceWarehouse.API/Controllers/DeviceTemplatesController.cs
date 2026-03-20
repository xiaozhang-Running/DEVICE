using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DeviceWarehouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviceTemplatesController : ControllerBase
{
    private readonly IDeviceTemplateService _templateService;

    public DeviceTemplatesController(IDeviceTemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet("special-equipment")]
    public async Task<ActionResult<IEnumerable<DeviceTemplateDto>>> GetSpecialEquipmentTemplates()
    {
        var templates = await _templateService.GetSpecialEquipmentTemplatesAsync();
        return Ok(ApiResponse<IEnumerable<DeviceTemplateDto>>.SuccessResponse(templates));
    }

    [HttpGet("general-equipment")]
    public async Task<ActionResult<IEnumerable<DeviceTemplateDto>>> GetGeneralEquipmentTemplates()
    {
        var templates = await _templateService.GetGeneralEquipmentTemplatesAsync();
        return Ok(ApiResponse<IEnumerable<DeviceTemplateDto>>.SuccessResponse(templates));
    }

    [HttpGet("consumables")]
    public async Task<ActionResult<IEnumerable<ConsumableTemplateDto>>> GetConsumableTemplates()
    {
        var templates = await _templateService.GetConsumableTemplatesAsync();
        return Ok(ApiResponse<IEnumerable<ConsumableTemplateDto>>.SuccessResponse(templates));
    }
}
