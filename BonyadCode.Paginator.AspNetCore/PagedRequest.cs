namespace BonyadCode.Paginator.AspNetCore;

/// <summary>
/// Represents a request for paginated data, including page number, page size, sorting order, and sorting field.
/// </summary>
public record PagedRequest
{
    /// <summary>
    /// The page number to retrieve. Defaults to 1.
    /// </summary>
    public uint? PageNumber { get; set; } = 1;

    /// <summary>
    /// The number of items per page. Defaults to 20.
    /// </summary>
    public uint? PageSize { get; set; } = 20;

    /// <summary>
    /// Specifies whether sorting should be in ascending order. Defaults to false.
    /// </summary>
    public bool? AscendingOrder { get; set; } = false;

    /// <summary>
    /// The field by which the data should be sorted.
    /// </summary>
    public string? OrderBy { get; set; }
}