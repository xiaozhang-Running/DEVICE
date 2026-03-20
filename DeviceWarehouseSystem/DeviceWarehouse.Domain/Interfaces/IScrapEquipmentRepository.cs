using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceWarehouse.Domain.Interfaces
{
    public interface IScrapEquipmentRepository
    {
        Task<ScrapEquipment?> GetByIdAsync(int id);
        Task<ScrapEquipment?> GetByDeviceCodeAsync(string deviceCode);
        Task<IEnumerable<ScrapEquipment>> GetAllAsync();
        Task<ScrapEquipment> AddAsync(ScrapEquipment scrapEquipment);
        Task UpdateAsync(ScrapEquipment scrapEquipment);
        Task DeleteAsync(int id);
        Task<IEnumerable<ScrapEquipment>> GetByDeviceTypeAsync(DeviceType deviceType);
        Task<bool> ExistsByDeviceCodeAsync(string deviceCode);
        Task<IEnumerable<ScrapEquipment>> SearchAsync(string keyword);
        Task<(IEnumerable<ScrapEquipment> Items, int TotalCount, int PageNumber, int PageSize, int TotalPages)> GetPagedAsync(int pageNumber, int pageSize, string? keyword = null, string? sortBy = null, bool sortDescending = false);
    }
}