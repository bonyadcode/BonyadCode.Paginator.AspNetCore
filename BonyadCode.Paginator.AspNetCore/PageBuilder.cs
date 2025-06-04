using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace BonyadCode.Paginator.AspNetCore;

/// <summary>
/// Constructs paginated data and provides metadata such as total item count, page count, and navigation properties.
/// </summary>
/// <typeparam name="T">The type of items being paginated.</typeparam>
public class PageBuilder<T>
{
    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public uint ItemCount { get; private set; }

    /// <summary>
    /// Total number of pages based on item count and page size.
    /// </summary>
    public uint PageCount { get; }

    /// <summary>
    /// The current page number.
    /// </summary>
    public uint PageNumber { get; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public uint PageSize { get; set; }

    /// <summary>
    /// Indicates whether a previous page exists.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indicates whether a next page exists.
    /// </summary>
    public bool HasNextPage => PageNumber < PageCount;

    /// <summary>
    /// The list of items for the current page.
    /// </summary>
    public IList<T> Items { get; private set; }
    
    /// <summary>
    /// Cache for storing compiled sorting expressions to improve performance.
    /// </summary>
    internal static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, Expression<Func<T, object>>>> Cache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PageBuilder{T}"/> class.
    /// </summary>
    /// <param name="itemCount">Total number of items.</param>
    /// <param name="pageNumber">Current page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="items">List of items for the current page.</param>
    internal PageBuilder(uint itemCount, uint pageNumber, uint pageSize, IList<T> items)
    {
        ItemCount = itemCount;
        PageCount = Math.Max(1, (uint)Math.Ceiling(itemCount / (double)pageSize));
        PageNumber = pageNumber;
        PageSize = pageSize;
        Items = items;
    }
}
