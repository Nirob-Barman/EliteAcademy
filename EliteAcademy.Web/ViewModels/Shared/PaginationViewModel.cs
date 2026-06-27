namespace EliteAcademy.Web.ViewModels.Shared;

public class PaginationViewModel
{
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public string Action { get; set; } = "";
    public string? Controller { get; set; }
}
