namespace PhotoGallery.PublicWeb.Models;

/// <summary>
/// View model for the home page, containing featured albums and random photos for the hero section
/// </summary>
public class HomeViewModel : PersonalInfoViewModel
{
    /// <summary>
    /// Featured albums to display on the home page (2-3 albums)
    /// </summary>
    public List<FeaturedAlbumViewModel> FeaturedAlbums { get; set; } = new();

    /// <summary>
    /// Random photos for the hero image carousel
    /// </summary>
    public List<RandomPhotoViewModel> HeroPhotos { get; set; } = new();

    /// <summary>
    /// Introduction text to display on the home page
    /// </summary>
    public string IntroductionText { get; set; } = string.Empty;

    /// <summary>
    /// Total number of albums available
    /// </summary>
    public int TotalAlbums { get; set; }

    /// <summary>
    /// Total number of photos available
    /// </summary>
    public int TotalPhotos { get; set; }
}
