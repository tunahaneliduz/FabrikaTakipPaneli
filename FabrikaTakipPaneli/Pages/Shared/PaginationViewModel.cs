namespace FabrikaTakipPaneli.Pages.Shared;

public class PaginationViewModel
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string PageName { get; set; } = "Index";
    public IDictionary<string, string?> RouteData { get; set; } = new Dictionary<string, string?>();
}
