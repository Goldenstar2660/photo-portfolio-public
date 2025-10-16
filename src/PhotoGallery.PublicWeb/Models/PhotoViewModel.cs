namespace PhotoGallery.PublicWeb.Models;

/// <summary>
/// View model for displaying a photo in the album grid
/// </summary>
public class PhotoViewModel
{
    /// <summary>
    /// Unique identifier for the photo
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Photo title (filename)
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Photo description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// URL of the thumbnail image for grid display
    /// </summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL of the full-size image for lightbox display
    /// </summary>
    public string FullSizeUrl { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes (formatted for display)
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Formatted file size string (e.g., "2.5 MB")
    /// </summary>
    public string FileSizeFormatted { get; set; } = string.Empty;

    /// <summary>
    /// Date when the photo was taken
    /// </summary>
    public DateTime? DateTaken { get; set; }

    /// <summary>
    /// Location where the photo was taken
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Image width in pixels
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Image height in pixels
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Formatted file size string (e.g., "2.5 MB")
    /// </summary>
    public string FormattedFileSize { get; set; } = string.Empty;

    /// <summary>
    /// Album ID this photo belongs to
    /// </summary>
    public Guid AlbumId { get; set; }

    /// <summary>
    /// Album name this photo belongs to
    /// </summary>
    public string AlbumName { get; set; } = string.Empty;

    /// <summary>
    /// Display order within the album
    /// </summary>
    public int DisplayOrder { get; set; }

    // EXIF fields
    public string? CameraModel { get; set; }
    public double? Aperture { get; set; }
    public string? ShutterSpeed { get; set; }
    public int? Iso { get; set; }
    public double? FocalLength { get; set; }
}
