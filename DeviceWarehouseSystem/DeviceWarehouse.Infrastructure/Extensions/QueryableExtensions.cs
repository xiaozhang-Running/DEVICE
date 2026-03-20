using DeviceWarehouse.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DeviceWarehouse.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize)
    {
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        string? sortBy,
        bool descending = false)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query;

        var property = typeof(T).GetProperty(sortBy);
        if (property == null)
            return query;

        return descending
            ? query.OrderByDescending(e => EF.Property<object>(e!, sortBy))
            : query.OrderBy(e => EF.Property<object>(e!, sortBy));
    }
}
