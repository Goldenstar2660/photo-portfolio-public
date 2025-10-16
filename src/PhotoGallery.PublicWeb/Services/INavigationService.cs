using PhotoGallery.PublicWeb.Models;

namespace PhotoGallery.PublicWeb.Services;

/// <summary>
/// Service for site navigation data with caching
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Gets album topics grouped for navigation dropdown with 30-minute caching
    /// </summary>
    /// <returns>List of album topics with their albums</returns>
    Task<List<AlbumTopicViewModel>> GetAlbumTopicsAsync();
}
