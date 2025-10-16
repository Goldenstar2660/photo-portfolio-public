namespace PhotoGallery.PublicWeb.Models;

/// <summary>
/// View model for a random photo displayed in the hero carousel on the home page
/// </summary>
public class RandomPhotoViewModel
{
    /// <summary>
    /// Unique identifier for the photo
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Photo title/caption
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// URL of the full-size photo for hero display
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Album name this photo belongs to
    /// </summary>
    public string AlbumName { get; set; } = string.Empty;

    /// <summary>
    /// Album ID this photo belongs to
    /// </summary>
    public Guid AlbumId { get; set; }

    /// <summary>
    /// Location where the photo was taken (if available)
    /// </summary>
    public string? Location { get; set; }
}
