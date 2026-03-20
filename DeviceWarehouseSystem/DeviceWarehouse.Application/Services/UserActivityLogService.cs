using AutoMapper;
using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Application.Interfaces;
using DeviceWarehouse.Domain.Entities;
using DeviceWarehouse.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceWarehouse.Application.Services
{
    public class UserActivityLogService : IUserActivityLogService
    {
        private readonly IUserActivityLogRepository _logRepository;
        private readonly IMapper _mapper;

        public UserActivityLogService(IUserActivityLogRepository logRepository, IMapper mapper)
        {
            _logRepository = logRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserActivityLogDto>> GetAllAsync()
        {
            var logs = await _logRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserActivityLogDto>>(logs);
        }

        public async Task<IEnumerable<UserActivityLogDto>> GetByUserIdAsync(int userId)
        {
            var logs = await _logRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<UserActivityLogDto>>(logs);
        }

        public async Task<UserActivityLogDto> CreateAsync(int? userId, string activityType, string activityDescription, string? ipAddress = null, string? userAgent = null)
        {
            var log = new UserActivityLog
            {
                UserId = userId ?? 0, // 使用 0 作为默认值，避免 NULL 值
                ActivityType = activityType,
                ActivityDescription = activityDescription,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            var createdLog = await _logRepository.AddAsync(log);
            return _mapper.Map<UserActivityLogDto>(createdLog);
        }

        public async Task<IEnumerable<UserActivityLogDto>> GetByDateRangeAsync(System.DateTime startDate, System.DateTime endDate)
        {
            var logs = await _logRepository.GetByDateRangeAsync(startDate, endDate);
            return _mapper.Map<IEnumerable<UserActivityLogDto>>(logs);
        }

        public async Task<IEnumerable<UserActivityLogDto>> GetByActivityTypeAsync(string activityType)
        {
            var logs = await _logRepository.GetByActivityTypeAsync(activityType);
            return _mapper.Map<IEnumerable<UserActivityLogDto>>(logs);
        }
    }
}
