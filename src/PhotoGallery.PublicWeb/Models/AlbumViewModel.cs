namespace PhotoGallery.PublicWeb.Models;

/// <summary>
/// View model for displaying an album with its photos
/// </summary>
public class AlbumViewModel
{
    /// <summary>
    /// Unique identifier for the album
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Album name/title
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Album topic/category (e.g., "Nature", "Urban", "Portraits")
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// Album description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Date when the album was created
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Photos in this album
    /// </summary>
    public List<PhotoViewModel> Photos { get; set; } = new();

    /// <summary>
    /// Total number of photos in the album (for pagination)
    /// </summary>
    public int TotalPhotoCount { get; set; }

    /// <summary>
    /// Current page number (for pagination)
    /// </summary>
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// Number of photos per page
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Whether this album is empty
    /// </summary>
    public bool IsEmpty => TotalPhotoCount == 0;
}
