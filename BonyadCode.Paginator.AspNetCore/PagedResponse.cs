namespace BonyadCode.Paginator.AspNetCore;

public record PagedResponse<T>
{
    public uint TotalCount { get; private set; }
    public uint TotalPages { get; }
    public uint PageNumber { get; }
    public uint PageSize { get; set; }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public IList<T> Items { get; private set; }

    internal PagedResponse(uint totalCount, uint pageNumber, uint pageSize, IList<T> items)
    {
        TotalCount = totalCount;
        TotalPages = (uint)Math.Ceiling(totalCount / (double)pageSize);
        PageNumber = pageNumber;
        PageSize = pageSize;
        Items = items;
    }
}