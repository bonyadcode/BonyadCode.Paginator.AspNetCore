namespace BonyadCode.Paginator.AspNetCore;

public record PagedRequest
{
    public uint PageNumber { get; set; } = 1;
    public uint PageSize { get; set; } = 20;
    public bool AscOrder { get; set; } = true;
    public string SortBy { get; set; } = "Id";
}