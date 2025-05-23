using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace BonyadCode.Paginator.AspNetCore;

public static class PaginatorService
{
    public static async Task<PagedResponse<T>> GetResponsePagedAsync<T>(
        IEnumerable source,
        uint pageNumber,
        uint pageSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source is IQueryable queryable)
        {
            // If the source is IQueryable (e.g., database query)
            var count = await queryable.Cast<T>().CountAsync(cancellationToken);
            var items = await queryable.Cast<T>()
                .Skip((int)((pageNumber - 1) * pageSize))
                .Take((int)pageSize)
                .ToListAsync(cancellationToken);
            return new PagedResponse<T>((uint)count, pageNumber, pageSize, items.Select(x => x.Adapt<T>()).ToList());
        }
        else
        {
            // If the source is an IEnumerable (e.g., List or IList)
            var enumerable = source.Cast<T>().ToList();
            var count = (uint)enumerable.Count;
            var items = enumerable
                .Skip((int)((pageNumber - 1) * pageSize))
                .Take((int)pageSize)
                .ToList();
            return new PagedResponse<T>(count, pageNumber, pageSize, items.Select(x => x.Adapt<T>()).ToList());
        }
    }

    public static IOrderedEnumerable<T> GetTOrderBy<T>(IEnumerable<T> list, Expression<Func<T, object>> expression,
        bool? ascOrder = false)
    {
        return (bool)ascOrder! ? list.OrderBy(expression.Compile()) : list.OrderByDescending(expression.Compile());
    }

    public static Expression<Func<T, object>> GetTExpressionFunction<T>(string field)
    {
        var param = Expression.Parameter(typeof(T), "entity");
        var property =
            typeof(T).GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            ?? typeof(T).GetProperty("Id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            ?? typeof(T).GetProperty("DateCreated",
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (property == null)
        {
            throw new ArgumentException($"Property '{field}' not found in type {typeof(T).Name}");
        }

        var propertyAccess = Expression.Property(param, property);
        var convert = Expression.Convert(propertyAccess, typeof(object));

        return Expression.Lambda<Func<T, object>>(convert, param);
    }
}