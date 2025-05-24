namespace BonyadCode.Paginator;

public record PagedRequest
{
    public uint? PageNumber { get; set; } = 1;
    public uint? PageSize { get; set; } = 20;
    public bool? AscendingOrder { get; set; } = false;
    public string? OrderBy { get; set; } = "Id";
}