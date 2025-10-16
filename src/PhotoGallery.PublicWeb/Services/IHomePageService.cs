using PhotoGallery.PublicWeb.Models;

namespace PhotoGallery.PublicWeb.Services;

/// <summary>
/// Service for aggregating data for the home page
/// </summary>
public interface IHomePageService
{
    /// <summary>
    /// Gets the complete home page view model with featured albums and hero photos
    /// </summary>
    /// <returns>Home page view model with all data populated</returns>
    Task<HomeViewModel> GetHomePageDataAsync();

    /// <summary>
    /// Gets featured albums for the home page (2-3 albums)
    /// </summary>
    /// <param name="count">Number of featured albums to retrieve (default: 3)</param>
    /// <returns>List of featured album view models</returns>
    Task<List<FeaturedAlbumViewModel>> GetFeaturedAlbumsAsync(int count = 3);

    /// <summary>
    /// Gets random photos for the hero carousel
    /// </summary>
    /// <param name="count">Number of random photos to retrieve (default: 5)</param>
    /// <returns>List of random photo view models</returns>
    Task<List<RandomPhotoViewModel>> GetHeroPhotosAsync(int count = 5);
}
