namespace PhotoGallery.PublicWeb.Models;

/// <summary>
/// View model for displaying photo details in lightbox
/// </summary>
public class PhotoDetailViewModel
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string FullSizeUrl { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime? DateTaken { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public long FileSize { get; set; }
    public string FormattedFileSize { get; set; } = string.Empty;
    
    // Navigation
    public int Index { get; set; }
    public int TotalPhotos { get; set; }
    public bool HasPrevious => Index > 0;
    public bool HasNext => Index < TotalPhotos - 1;
}
