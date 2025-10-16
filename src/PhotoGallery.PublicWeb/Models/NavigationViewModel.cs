namespace PhotoGallery.PublicWeb.Models;

/// <summary>
/// View model for navigation menu with album topics
/// </summary>
public class NavigationViewModel
{
    /// <summary>
    /// List of unique album topics for the Albums dropdown
    /// </summary>
    public List<AlbumTopicViewModel> AlbumTopics { get; set; } = new();

    /// <summary>
    /// Currently active page for highlighting in navigation
    /// </summary>
    public string ActivePage { get; set; } = string.Empty;
}

/// <summary>
/// View model for an album topic in the navigation dropdown
/// </summary>
public class AlbumTopicViewModel
{
    /// <summary>
    /// Topic/category name (e.g., "Nature", "Urban", "Portraits")
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// Number of albums in this topic
    /// </summary>
    public int AlbumCount { get; set; }

    /// <summary>
    /// List of albums under this topic
    /// </summary>
    public List<AlbumSummaryViewModel> Albums { get; set; } = new();
}

/// <summary>
/// Summarized view of an album for navigation dropdown
/// </summary>
public class AlbumSummaryViewModel
{
    /// <summary>
    /// Album ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Album name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Number of photos in this album
    /// </summary>
    public int PhotoCount { get; set; }
}
