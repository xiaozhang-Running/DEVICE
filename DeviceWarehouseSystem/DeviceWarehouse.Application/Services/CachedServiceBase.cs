using DeviceWarehouse.Application.DTOs;
using DeviceWarehouse.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DeviceWarehouse.Application.Services;

public abstract class CachedServiceBase
{
    protected readonly ICacheService _cache;
    protected readonly ILogger _logger;

    protected CachedServiceBase(ICacheService cache, ILogger logger)
    {
        _cache = cache;
        _logger = logger;
    }

    protected async Task<T?> GetFromCacheAsync<T>(string cacheKey)
    {
        try
        {
            var cached = await _cache.GetAsync<T>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
                return cached;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error reading from cache for key: {CacheKey}", cacheKey);
        }
        return default;
    }

    protected async Task SetCacheAsync<T>(string cacheKey, T value, TimeSpan? expiration = null)
    {
        try
        {
            await _cache.SetAsync(cacheKey, value, expiration);
            _logger.LogDebug("Cache set for key: {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error writing to cache for key: {CacheKey}", cacheKey);
        }
    }

    protected async Task InvalidateCacheAsync(string cacheKey)
    {
        try
        {
            await _cache.RemoveAsync(cacheKey);
            _logger.LogDebug("Cache invalidated for key: {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error invalidating cache for key: {CacheKey}", cacheKey);
        }
    }

    protected string BuildCacheKey(string prefix, params object[] parameters)
    {
        var key = prefix;
        foreach (var param in parameters)
        {
            key += $":{param}";
        }
        return key;
    }
}
