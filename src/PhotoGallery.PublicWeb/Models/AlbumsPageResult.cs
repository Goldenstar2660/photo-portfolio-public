namespace PhotoGallery.PublicWeb.Models;

/// <summary>
/// Result model for albums page data
/// </summary>
public class AlbumsPageResult
{
    /// <summary>
    /// List of album cards for the current page
    /// </summary>
    public List<AlbumCardViewModel> Albums { get; set; } = new();

    /// <summary>
    /// Total number of albums (across all pages)
    /// </summary>
    public int TotalCount { get; set; }
}
