using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace BonyadCode.Paginator;

public record PagedResponse<T>
{
    public uint ItemCount { get; private set; }
    public uint PageCount { get; }
    public uint PageNumber { get; }
    public uint PageSize { get; set; }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < PageCount;

    public IList<T> Items { get; private set; }

    internal PagedResponse(uint itemCount, uint pageNumber, uint pageSize, IList<T> items)
    {
        ItemCount = itemCount;
        PageCount = (uint)Math.Ceiling(itemCount / (double)pageSize);
        PageNumber = pageNumber;
        PageSize = pageSize;
        Items = items;
    }

    // Factory method for IQueryable (EF or LINQ)
    public static async Task<PagedResponse<T>> CreateAsync(
        IQueryable<T> source,
        uint pageNumber,
        uint pageSize,
        bool? ascendingOrder = null,
        string? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source is IQueryable queryable)
        {
            // If the source is IQueryable (e.g., database query)
            var count = await queryable.Cast<T>().CountAsync(cancellationToken);
            List<T> list;

            if (ascendingOrder == null)
            {
                list = await queryable.Cast<T>()
                    .Skip((int)((pageNumber - 1) * pageSize))
                    .Take((int)pageSize)
                    .ToListAsync(cancellationToken);
            }
            else
            {
                list = (bool)ascendingOrder
                    ? await queryable.Cast<T>()
                        .Order()
                        .Skip((int)((pageNumber - 1) * pageSize))
                        .Take((int)pageSize)
                        .ToListAsync(cancellationToken)
                    : await queryable.Cast<T>()
                        .OrderDescending()
                        .Skip((int)((pageNumber - 1) * pageSize))
                        .Take((int)pageSize)
                        .ToListAsync(cancellationToken);
            }

            var items = new List<T>();
            items.AddRange(list);
            return new PagedResponse<T>((uint)count, pageNumber, pageSize, items);
        }
        else
        {
            // If the source is an IEnumerable (e.g., List or IList)
            var enumerable = source.Cast<T>().ToList();
            var count = (uint)enumerable.Count;
            IOrderedEnumerable<T>? orderedEnumerable;

            if (ascendingOrder != null)
            {
                orderedEnumerable = (bool)ascendingOrder
                    ? enumerable
                        .Order()
                    : enumerable
                        .OrderDescending();
            }
            
            var list = ascendingOrder == null ? 
                enumerable
                    .Skip((int)((pageNumber - 1) * pageSize))
                .Take((int)pageSize)
                .ToList()
                : orderedEnumerable
                .Skip((int)((pageNumber - 1) * pageSize))
                .Take((int)pageSize)
                .ToList();

            var items = new List<T>();
            items.AddRange(list);
            return new PagedResponse<T>(count, pageNumber, pageSize, items);
        }
    }

    // Factory method for IEnumerable (Lists, in-memory collections)
    public static PagedResponse<T> Create(
        IEnumerable<T> source,
        uint pageNumber,
        uint pageSize,
        bool? ascendingOrder = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source is IQueryable queryable)
        {
            // If the source is IQueryable (e.g., database query)
            var count = queryable.Cast<T>().Count();
            List<T> list;

            if (ascendingOrder == null)
            {
                list = queryable.Cast<T>()
                    .Skip((int)((pageNumber - 1) * pageSize))
                    .Take((int)pageSize)
                    .ToList();
            }
            else
            {
                list = (bool)ascendingOrder
                    ? queryable.Cast<T>()
                        .Order()
                        .Skip((int)((pageNumber - 1) * pageSize))
                        .Take((int)pageSize)
                        .ToList()
                    : queryable.Cast<T>()
                        .OrderDescending()
                        .Skip((int)((pageNumber - 1) * pageSize))
                        .Take((int)pageSize)
                        .ToList();
            }

            var items = new List<T>();
            items.AddRange(list);
            return new PagedResponse<T>((uint)count, pageNumber, pageSize, items);
        }
        else
        {
            // If the source is an IEnumerable (e.g., List or IList)
            var enumerable = source.ToList();
            var count = (uint)enumerable.Count;
            List<T> list;

            if (ascendingOrder == null)
            {
                list = enumerable
                    .Skip((int)((pageNumber - 1) * pageSize))
                    .Take((int)pageSize)
                    .ToList();
            }
            else
            {
                list = (bool)ascendingOrder
                    ? enumerable
                        .Order()
                        .Skip((int)((pageNumber - 1) * pageSize))
                        .Take((int)pageSize)
                        .ToList()
                    : enumerable
                        .OrderDescending()
                        .Skip((int)((pageNumber - 1) * pageSize))
                        .Take((int)pageSize)
                        .ToList();
            }

            var items = new List<T>();
            items.AddRange(list);
            return new PagedResponse<T>(count, pageNumber, pageSize, items);
        }
    }
    
    public static Expression<Func<T, object>>? GetTExpressionFunction<T>(string field)
    {
        var param = Expression.Parameter(typeof(T), "entity");
        var property =
            typeof(T).GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            ?? typeof(T).GetProperty("Id", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            ?? typeof(T).GetProperty("DateCreated",
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (property == null)
            return null;

        var propertyAccess = Expression.Property(param, property);
        var convert = Expression.Convert(propertyAccess, typeof(object));

        return Expression.Lambda<Func<T, object>>(convert, param);
    }
}