using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace BonyadCode.Paginator;

public static class PageBuilderExtensions
{
    public static Task<PageBuilder<T>> ToPagedResponseAsync<T>(
        this IQueryable<T>? source,
        PagedRequest? request = null,
        Expression<Func<T, object>>? expressionFunction = null,
        CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        
        return CreateAsync(
            source,
            request.PageNumber ?? 1,
            request.PageSize ?? 20,
            request.AscendingOrder,
            request.OrderBy,
            expressionFunction,
            cancellationToken
        );
    }

    public static PageBuilder<T> ToPagedResponse<T>(
        this IEnumerable<T>? source,
        PagedRequest? request = null,
        Expression<Func<T, object>>? expressionFunction = null,
        CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();

        return Create(
            source,
            request.PageNumber ?? 1,
            request.PageSize ?? 20,
            request.AscendingOrder,
            request.OrderBy,
            expressionFunction,
            cancellationToken
        );
    }

    private static async Task<PageBuilder<T>> CreateAsync<T>(
        IQueryable<T>? source,
        uint pageNumber,
        uint pageSize,
        bool? ascendingOrder = null,
        string? orderBy = null,
        Expression<Func<T, object>>? expressionFunction = null,
        CancellationToken cancellationToken = default)
    {
        if (source == null)
            return new PageBuilder<T>(0, 1, pageSize, []);
        
        if (pageSize == 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");

        expressionFunction ??= orderBy != null ? GetExpressionFunction<T>(orderBy) : null;

        var count = await source.CountAsync(cancellationToken);
        var items = await ApplyOrderingAndPagingAsync(source, pageNumber, pageSize,ascendingOrder, expressionFunction, cancellationToken);
        return new PageBuilder<T>((uint)count, pageNumber, pageSize, items);
    }

    private static PageBuilder<T> Create<T>(
        IEnumerable<T>? source,
        uint pageNumber,
        uint pageSize,
        bool? ascendingOrder = null,
        string? orderBy = null,
        Expression<Func<T, object>>? expressionFunction = null,
        CancellationToken cancellationToken = default)
    {
        if (source == null)
            return new PageBuilder<T>(0, 1, pageSize, []);
        
        if (pageSize == 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");

        cancellationToken.ThrowIfCancellationRequested();

        expressionFunction ??= orderBy != null ? GetExpressionFunction<T>(orderBy) : null;

        var list = source.ToList();
        var count = (uint)list.Count;
        var items = ApplyOrderingAndPaging(list, pageNumber, pageSize,ascendingOrder, expressionFunction, cancellationToken);
        return new PageBuilder<T>(count, pageNumber, pageSize, items);
    }

    private static async Task<List<T>> ApplyOrderingAndPagingAsync<T>(
        IQueryable<T> source,
        uint pageNumber,
        uint pageSize,
        bool? ascendingOrder,
        Expression<Func<T, object>>? orderByExpression,
        CancellationToken cancellationToken = default)
    {
        var query = source;

        if (orderByExpression != null)
            query = ascendingOrder ?? false ? query.OrderBy(orderByExpression) : query.OrderByDescending(orderByExpression);

        return await query
            .Skip((int)((pageNumber - 1) * pageSize))
            .Take((int)pageSize)
            .ToListAsync(cancellationToken);
    }

    private static List<T> ApplyOrderingAndPaging<T>(
        IEnumerable<T> source,
        uint pageNumber,
        uint pageSize,
        bool? ascendingOrder,
        Expression<Func<T, object>>? orderByExpression,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var query = source;

        if (orderByExpression != null)
            query = ascendingOrder ?? false ? query.OrderBy(orderByExpression.Compile()) : query.OrderByDescending(orderByExpression.Compile());

        return query
            .Skip((int)((pageNumber - 1) * pageSize))
            .Take((int)pageSize)
            .ToList();
    }

    private static Expression<Func<T, object>> GetExpressionFunction<T>(string field)
    {
        var type = typeof(T);
        var fieldCache = PageBuilder<T>.Cache.GetOrAdd(type, _ => new ConcurrentDictionary<string, Expression<Func<T, object>>>());
        
        return fieldCache.GetOrAdd(field, key =>
        {
            var param = Expression.Parameter(type, "entity");

            var property = type.GetProperty(key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                           ?? throw new ArgumentException($"Property '{key}' was not found on type '{type.Name}'");

            var propertyAccess = Expression.Property(param, property);
            var convert = Expression.Convert(propertyAccess, typeof(object));
            return Expression.Lambda<Func<T, object>>(convert, param);
        });
    }
}