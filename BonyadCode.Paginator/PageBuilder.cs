using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace BonyadCode.Paginator;

public class PageBuilder<T>
{
    public uint ItemCount { get; private set; }
    public uint PageCount { get; }
    public uint PageNumber { get; }
    public uint PageSize { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < PageCount;
    public IList<T> Items { get; private set; }
    
    internal static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, Expression<Func<T, object>>>> Cache = new();

    internal PageBuilder(uint itemCount, uint pageNumber, uint pageSize, IList<T> items)
    {
        ItemCount = itemCount;
        PageCount = (uint)Math.Ceiling(itemCount / (double)pageSize);
        PageNumber = pageNumber;
        PageSize = pageSize;
        Items = items;
    }
}
