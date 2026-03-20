using AutoMapper;
using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Enums;
using DeviceWarehouse.Domain.Interfaces;

namespace DeviceWarehouse.Application.Services;

public class ScrapEquipmentService : IScrapEquipmentService
{
    private readonly IScrapEquipmentRepository _scrapEquipmentRepository;
    private readonly IGeneralEquipmentRepository _generalEquipmentRepository;
    private readonly ISpecialEquipmentRepository _specialEquipmentRepository;
    private readonly IMapper _mapper;

    public ScrapEquipmentService(
        IScrapEquipmentRepository scrapEquipmentRepository,
        IGeneralEquipmentRepository generalEquipmentRepository,
        ISpecialEquipmentRepository specialEquipmentRepository,
        IMapper mapper)
    {
        _scrapEquipmentRepository = scrapEquipmentRepository;
        _generalEquipmentRepository = generalEquipmentRepository;
        _specialEquipmentRepository = specialEquipmentRepository;
        _mapper = mapper;
    }

    public async Task<ScrapEquipmentDto> GetByIdAsync(int id)
    {
        var scrapEquipment = await _scrapEquipmentRepository.GetByIdAsync(id);
        if (scrapEquipment == null)
            throw new Exception("报废设备不存在");
        return _mapper.Map<ScrapEquipmentDto>(scrapEquipment);
    }

    public async Task<IEnumerable<ScrapEquipmentDto>> GetAllAsync()
    {
        var scrapEquipments = await _scrapEquipmentRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ScrapEquipmentDto>>(scrapEquipments);
    }

    public async Task<IEnumerable<ScrapEquipmentDto>> GetByDeviceTypeAsync(DeviceType deviceType)
    {
        var scrapEquipments = await _scrapEquipmentRepository.GetByDeviceTypeAsync(deviceType);
        return _mapper.Map<IEnumerable<ScrapEquipmentDto>>(scrapEquipments);
    }

    public async Task<ScrapEquipmentDto> CreateAsync(CreateScrapEquipmentDto dto)
    {
        if (await _scrapEquipmentRepository.ExistsByDeviceCodeAsync(dto.DeviceCode))
            throw new Exception("该设备已经在报废列表中");

        var scrapEquipment = _mapper.Map<ScrapEquipment>(dto);
        scrapEquipment.ScrapDate = DateTime.Now;

        var createdScrapEquipment = await _scrapEquipmentRepository.AddAsync(scrapEquipment);
        return _mapper.Map<ScrapEquipmentDto>(createdScrapEquipment);
    }

    public async Task UpdateAsync(int id, UpdateScrapEquipmentDto dto)
    {
        var scrapEquipment = await _scrapEquipmentRepository.GetByIdAsync(id);
        if (scrapEquipment == null)
            throw new Exception("报废设备不存在");

        _mapper.Map(dto, scrapEquipment);
        await _scrapEquipmentRepository.UpdateAsync(scrapEquipment);
    }

    public async Task DeleteAsync(int id)
    {
        await _scrapEquipmentRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<ScrapEquipmentDto>> SearchAsync(string keyword)
    {
        var scrapEquipments = await _scrapEquipmentRepository.SearchAsync(keyword);
        return _mapper.Map<IEnumerable<ScrapEquipmentDto>>(scrapEquipments);
    }

    public async Task<object> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false)
    {
        var (items, totalCount, pageNumberResult, pageSizeResult, totalPages) = await _scrapEquipmentRepository.GetPagedAsync(pageNumber, pageSize, keyword, sortBy, sortDescending);
        var mappedItems = _mapper.Map<IEnumerable<ScrapEquipmentDto>>(items);
        return new {
            items = mappedItems,
            totalCount,
            pageNumber = pageNumberResult,
            pageSize = pageSizeResult,
            totalPages
        };
    }

    public async Task<bool> ScrapDeviceAsync(string deviceCode, string scrapReason, string scrappedBy)
    {
        // 检查设备是否已经在报废列表中
        if (await _scrapEquipmentRepository.ExistsByDeviceCodeAsync(deviceCode))
            throw new Exception("该设备已经在报废列表中");

        // 检查是否为通用设备
        var generalEquipment = await _generalEquipmentRepository.GetByCodeAsync(deviceCode);
        if (generalEquipment != null)
        {
            // 创建报废设备记录
            var scrapEquipment = new ScrapEquipment
            {
                DeviceCode = generalEquipment.DeviceCode,
                DeviceName = generalEquipment.DeviceName,
                DeviceType = generalEquipment.DeviceType,
                Brand = generalEquipment.Brand,
                Model = generalEquipment.Model,
                Specification = generalEquipment.Specification,
                Unit = generalEquipment.Unit,
                Quantity = generalEquipment.Quantity,
                Location = generalEquipment.Location,
                Company = generalEquipment.Company,
                ScrapReason = scrapReason,
                ScrappedBy = scrappedBy,
                ScrapDate = DateTime.Now
            };

            // 添加到报废列表
            await _scrapEquipmentRepository.AddAsync(scrapEquipment);

            // 更新原设备状态为报废
            generalEquipment.DeviceStatus = DeviceStatus.Scrap;
            await _generalEquipmentRepository.UpdateAsync(generalEquipment);

            return true;
        }

        // 检查是否为特种设备
        var specialEquipment = await _specialEquipmentRepository.GetByCodeAsync(deviceCode);
        if (specialEquipment != null)
        {
            // 创建报废设备记录
            var scrapEquipment = new ScrapEquipment
            {
                DeviceCode = specialEquipment.DeviceCode,
                DeviceName = specialEquipment.DeviceName,
                DeviceType = specialEquipment.DeviceType,
                Brand = specialEquipment.Brand,
                Model = specialEquipment.Model,
                Specification = specialEquipment.Specification,
                Unit = specialEquipment.Unit,
                Quantity = specialEquipment.Quantity,
                Location = specialEquipment.Location,
                Company = specialEquipment.Company,
                ScrapReason = scrapReason,
                ScrappedBy = scrappedBy,
                ScrapDate = DateTime.Now
            };

            // 添加到报废列表
            await _scrapEquipmentRepository.AddAsync(scrapEquipment);

            // 更新原设备状态为报废
            specialEquipment.DeviceStatus = DeviceStatus.Scrap;
            await _specialEquipmentRepository.UpdateAsync(specialEquipment);

            return true;
        }

        throw new Exception("设备不存在");
    }
}