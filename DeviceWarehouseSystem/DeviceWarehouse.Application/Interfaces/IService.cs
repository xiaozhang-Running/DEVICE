using DeviceWarehouse.Application.DTOs;

namespace DeviceWarehouse.Application.Interfaces;

public interface IService<TEntity, TDto, TCreateDto, TUpdateDto, TSummaryDto>
    where TEntity : class
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
    where TSummaryDto : class
{
    Task<TDto> GetByIdAsync(int id);
    Task<IEnumerable<TDto>> GetAllAsync();
    Task<TDto> CreateAsync(TCreateDto createDto);
    Task<TDto> UpdateAsync(int id, TUpdateDto updateDto);
    Task DeleteAsync(int id);
    Task<IEnumerable<TSummaryDto>> GetSummaryAsync();
    Task<bool> ExistsAsync(string number);
    Task CompleteAsync(int id) => Task.CompletedTask;
}
