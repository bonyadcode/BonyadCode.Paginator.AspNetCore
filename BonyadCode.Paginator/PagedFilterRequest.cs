namespace BonyadCode.Paginator;

public record PagedFilterRequest : PagedRequest
{
    // Base
    public List<string>? IdList { get; set; }
    public DateTime? DateCreatedFrom { get; set; }
    public DateTime? DateCreatedTo { get; set; }
    public DateTime? DateModifiedFrom { get; set; }
    public DateTime? DateModifiedTo { get; set; }
}