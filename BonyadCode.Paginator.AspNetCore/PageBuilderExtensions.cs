using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace BonyadCode.Paginator.AspNetCore;

/// <summary>
/// Provides extension methods for paginating collections and queryable data.
/// </summary>
public static partial class PageBuilderExtensions
{
    /// <summary>
    /// Converts an IEnumerable source into a paginated response.
    /// </summary>
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
            request.AscendingOrder ?? false,
            request.OrderBy,
            expressionFunction,
            cancellationToken
        );
    }

    /// <summary>
    /// Converts an IQueryable source into a paginated response asynchronously.
    /// </summary>
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
            request.AscendingOrder ?? false,
            request.OrderBy,
            expressionFunction,
            cancellationToken
        );
    }
}

public static partial class PageBuilderExtensions
{
    /// <summary>
    /// Creates a paginated response from an IEnumerable source.
    /// </summary>
    private static PageBuilder<T> Create<T>(
        IEnumerable<T>? source,
        uint pageNumber,
        uint pageSize,
        bool ascendingOrder,
        string? orderBy = null,
        Expression<Func<T, object>>? orderByExpression = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        pageSize = pageSize == 0 ? 1 : pageSize;
        if (source == null)
            return new PageBuilder<T>(0, 1, pageSize, []);

        var list = source.ToList();
        if (list.Count == 0)
            return new PageBuilder<T>(0, 1, pageSize, []);

        orderByExpression ??= orderBy != null ? GetExpressionFunction<T>(orderBy) : null;

        var count = (uint)list.Count;
        var items = ApplyOrderingAndPaging(list, pageNumber, pageSize, ascendingOrder, orderByExpression,
            cancellationToken);
        return new PageBuilder<T>(count, pageNumber, pageSize, items);
    }

    /// <summary>
    /// Creates a paginated response from an IQueryable source asynchronously.
    /// </summary>
    private static async Task<PageBuilder<T>> CreateAsync<T>(
        IQueryable<T>? source,
        uint pageNumber,
        uint pageSize,
        bool ascendingOrder,
        string? orderBy = null,
        Expression<Func<T, object>>? orderByExpression = null,
        CancellationToken cancellationToken = default)
    {
        pageSize = pageSize == 0 ? 1 : pageSize;
        if (source == null || !source.Any())
            return new PageBuilder<T>(0, 1, pageSize, []);

        orderByExpression ??= orderBy != null ? GetExpressionFunction<T>(orderBy) : null;

        var count = await source.CountAsync(cancellationToken);
        var items = await ApplyOrderingAndPagingAsync(source, pageNumber, pageSize, ascendingOrder, orderByExpression,
            cancellationToken);
        return new PageBuilder<T>((uint)count, pageNumber, pageSize, items);
    }

    /// <summary>
    /// Applies sorting and pagination on an IEnumerable source.
    /// </summary>
    private static List<T> ApplyOrderingAndPaging<T>(
        IEnumerable<T> source,
        uint pageNumber,
        uint pageSize,
        bool ascendingOrder,
        Expression<Func<T, object>>? orderByExpression,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var query = source;

        if (orderByExpression != null)
            query = ascendingOrder
                ? query.OrderBy(orderByExpression.Compile())
                : query.OrderByDescending(orderByExpression.Compile());

        return query
            .Skip((int)((pageNumber - 1) * pageSize))
            .Take((int)pageSize)
            .ToList();
    }

    /// <summary>
    /// Applies sorting and pagination on an IQueryable source asynchronously.
    /// </summary>
    private static async Task<List<T>> ApplyOrderingAndPagingAsync<T>(
        IQueryable<T> source,
        uint pageNumber,
        uint pageSize,
        bool ascendingOrder,
        Expression<Func<T, object>>? orderByExpression,
        CancellationToken cancellationToken = default)
    {
        var query = source;

        if (orderByExpression != null)
            query = ascendingOrder
                ? query.OrderBy(orderByExpression)
                : query.OrderByDescending(orderByExpression);

        return await query
            .Skip((int)((pageNumber - 1) * pageSize))
            .Take((int)pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Generates an expression function for ordering by a specified field.
    /// </summary>
    private static Expression<Func<T, object>> GetExpressionFunction<T>(string field)
    {
        var type = typeof(T);
        var fieldCache =
            PageBuilder<T>.Cache.GetOrAdd(type, _ => new ConcurrentDictionary<string, Expression<Func<T, object>>>());

        return fieldCache.GetOrAdd(field, key =>
        {
            var param = Expression.Parameter(type, "entity");

            var property = type.GetProperty(key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                           ?? throw new ArgumentException(
                               $"Property '{key}' was not found on type '{type.Name}' for Pagination Ordering.");

            var propertyAccess = Expression.Property(param, property);
            var convert = Expression.Convert(propertyAccess, typeof(object));
            return Expression.Lambda<Func<T, object>>(convert, param);
        });
    }
}