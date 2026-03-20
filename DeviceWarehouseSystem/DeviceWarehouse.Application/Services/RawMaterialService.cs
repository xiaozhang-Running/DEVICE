using AutoMapper;
using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;

namespace DeviceWarehouse.Application.Services;

public class RawMaterialService : IRawMaterialService
{
    private readonly IRawMaterialRepository _rawMaterialRepository;
    private readonly IMapper _mapper;

    public RawMaterialService(
        IRawMaterialRepository rawMaterialRepository,
        IMapper mapper)
    {
        _rawMaterialRepository = rawMaterialRepository;
        _mapper = mapper;
    }

    public async Task<RawMaterialDto> GetByIdAsync(int id)
    {
        var material = await _rawMaterialRepository.GetByIdAsync(id);
        if (material == null)
            throw new Exception("原材料不存在");
        return _mapper.Map<RawMaterialDto>(material);
    }

    public async Task<IEnumerable<RawMaterialDto>> GetAllAsync()
    {
        var materials = await _rawMaterialRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<RawMaterialDto>>(materials);
    }

    async Task<RawMaterialDto> IService<RawMaterial, RawMaterialDto, CreateRawMaterialDto, UpdateRawMaterialDto, RawMaterialDto>.CreateAsync(CreateRawMaterialDto dto)
    {
        var material = _mapper.Map<RawMaterial>(dto);
        
        // 如果没有提供总数或已使用数，根据剩余数量计算
        if (material.TotalQuantity == 0 && material.UsedQuantity == 0 && material.RemainingQuantity > 0)
        {
            material.TotalQuantity = material.RemainingQuantity;
            material.UsedQuantity = 0;
        }
        // 否则确保剩余数量等于总数减去已使用数
        else
        {
            material.RemainingQuantity = material.TotalQuantity - material.UsedQuantity;
        }
        
        var createdMaterial = await _rawMaterialRepository.AddAsync(material);
        return _mapper.Map<RawMaterialDto>(createdMaterial);
    }

    async Task<RawMaterialDto> IService<RawMaterial, RawMaterialDto, CreateRawMaterialDto, UpdateRawMaterialDto, RawMaterialDto>.UpdateAsync(int id, UpdateRawMaterialDto dto)
    {
        var material = await _rawMaterialRepository.GetByIdAsync(id);
        if (material == null)
            throw new Exception("原材料不存在");

        _mapper.Map(dto, material);
        
        // 如果提供了剩余数量，根据剩余数量计算总数和已使用数
        if (dto.RemainingQuantity.HasValue)
        {
            material.TotalQuantity = dto.RemainingQuantity.Value;
            material.UsedQuantity = 0;
            material.RemainingQuantity = dto.RemainingQuantity.Value;
        }
        // 否则确保剩余数量等于总数减去已使用数
        else
        {
            material.RemainingQuantity = material.TotalQuantity - material.UsedQuantity;
        }
        
        await _rawMaterialRepository.UpdateAsync(material);
        return _mapper.Map<RawMaterialDto>(material);
    }

    public async Task<RawMaterialDto> CreateAsync(CreateRawMaterialDto dto)
    {
        return await ((IService<RawMaterial, RawMaterialDto, CreateRawMaterialDto, UpdateRawMaterialDto, RawMaterialDto>)this).CreateAsync(dto);
    }

    public async Task<RawMaterialDto> UpdateAsync(int id, UpdateRawMaterialDto dto)
    {
        return await ((IService<RawMaterial, RawMaterialDto, CreateRawMaterialDto, UpdateRawMaterialDto, RawMaterialDto>)this).UpdateAsync(id, dto);
    }

    public async Task DeleteAsync(int id)
    {
        var material = await _rawMaterialRepository.GetByIdAsync(id);
        if (material == null)
            throw new Exception("原材料不存在");

        await _rawMaterialRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<RawMaterialDto>> GetSummaryAsync()
    {
        var materials = await _rawMaterialRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<RawMaterialDto>>(materials);
    }

    public async Task<bool> ExistsAsync(string number)
    {
        return await _rawMaterialRepository.ExistsAsync(number);
    }

    public async Task<IEnumerable<RawMaterialDto>> SearchAsync(string keyword)
    {
        var materials = await _rawMaterialRepository.SearchAsync(keyword);
        return _mapper.Map<IEnumerable<RawMaterialDto>>(materials);
    }

    public async Task<object> DeleteAllAsync()
    {
        var allMaterials = await _rawMaterialRepository.GetAllAsync();
        var ids = allMaterials.Select(m => m.Id).ToList();
        
        if (!ids.Any())
        {
            return new
            {
                successCount = 0,
                errorCount = 0,
                message = "没有可删除的原材料"
            };
        }

        var successCount = 0;
        var errorCount = 0;
        var errors = new List<string>();

        foreach (var id in ids)
        {
            try
            {
                await _rawMaterialRepository.DeleteAsync(id);
                successCount++;
            }
            catch (Exception ex)
            {
                errorCount++;
                errors.Add($"原材料ID {id}: {ex.Message}");
            }
        }

        return new
        {
            successCount,
            errorCount,
            errors
        };
    }

    public async Task<object> CreateBatchAsync(IEnumerable<CreateRawMaterialDto> dtos)
    {
        if (dtos == null || !dtos.Any())
            throw new Exception("原材料列表不能为空");

        var successCount = 0;
        var errorCount = 0;
        var errors = new List<string>();
        var totalCount = dtos.Count();

        Console.WriteLine($"开始批量导入原材料，总记录数: {totalCount}");

        foreach (var dto in dtos)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.ProductName))
                {
                    throw new Exception("产品名称不能为空");
                }

                var material = _mapper.Map<RawMaterial>(dto);
                
                // 如果没有提供总数或已使用数，根据剩余数量计算
                if (material.TotalQuantity == 0 && material.UsedQuantity == 0 && material.RemainingQuantity > 0)
                {
                    material.TotalQuantity = material.RemainingQuantity;
                    material.UsedQuantity = 0;
                }
                // 否则确保剩余数量等于总数减去已使用数
                else
                {
                    material.RemainingQuantity = material.TotalQuantity - material.UsedQuantity;
                }
                
                await _rawMaterialRepository.AddAsync(material);
                successCount++;
            }
            catch (Exception ex)
            {
                errorCount++;
                errors.Add($"原材料: {dto.ProductName} - {ex.Message}");
            }
        }

        Console.WriteLine($"批量导入完成，成功: {successCount}, 失败: {errorCount}");

        return new
        {
            successCount,
            errorCount,
            errors
        };
    }
}
