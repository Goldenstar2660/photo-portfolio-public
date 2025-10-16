namespace PhotoGallery.PublicWeb.Models;

/// <summary>
/// View model for a featured album displayed on the home page
/// </summary>
public class FeaturedAlbumViewModel
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
    /// URL of the cover/thumbnail photo for the album
    /// </summary>
    public string CoverPhotoUrl { get; set; } = string.Empty;

    /// <summary>
    /// Number of photos in this album
    /// </summary>
    public int PhotoCount { get; set; }

    /// <summary>
    /// Date when the album was created
    /// </summary>
    public DateTime CreatedDate { get; set; }
}
