using DeviceWarehouse.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace DeviceWarehouse.Infrastructure.Services;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(5);
    private readonly Dictionary<string, object?> _cacheKeys = new Dictionary<string, object?>();

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(expiration ?? _defaultExpiration)
            .RegisterPostEvictionCallback((evictedKey, _, _, _) =>
            {
                if (evictedKey != null)
                {
                    var keyStr = evictedKey.ToString();
                    if (keyStr != null)
                    {
                        _cacheKeys.Remove(keyStr);
                    }
                }
            });

        _cache.Set(key, value, options);
        _cacheKeys[key] = null;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        _cacheKeys.Remove(key);
        return Task.CompletedTask;
    }

    public Task RemovePatternAsync(string pattern)
    {
        var regex = new Regex(pattern.Replace("*", ".*"));
        var keysToRemove = _cacheKeys.Keys.Where(key => regex.IsMatch(key)).ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _cacheKeys.Remove(key);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
    {
        return Task.FromResult(_cache.TryGetValue(key, out _));
    }
}
