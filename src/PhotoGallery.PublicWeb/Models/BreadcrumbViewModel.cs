namespace PhotoGallery.PublicWeb.Models;

/// <summary>
/// View model for breadcrumb navigation
/// </summary>
public class BreadcrumbViewModel
{
    public List<BreadcrumbItemViewModel> Items { get; set; } = new();
}

/// <summary>
/// Individual breadcrumb item
/// </summary>
public class BreadcrumbItemViewModel
{
    public string Text { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
