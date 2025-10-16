using PhotoGallery.PublicWeb.Models;

namespace PhotoGallery.PublicWeb.Services;

/// <summary>
/// Service for retrieving album and photo data with caching
/// </summary>
public interface IAlbumPageService
{
    /// <summary>
    /// Gets an album by ID with all its photos
    /// </summary>
    /// <param name="albumId">Album unique identifier</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of photos per page (default: 50)</param>
    /// <returns>Album view model with photos</returns>
    Task<AlbumViewModel?> GetAlbumByIdAsync(Guid albumId, int page = 1, int pageSize = 50);

    /// <summary>
    /// Gets all album topics with their albums for navigation
    /// </summary>
    /// <returns>List of album topics with album summaries</returns>
    Task<List<AlbumTopicViewModel>> GetAlbumTopicsAsync();

    /// <summary>
    /// Gets all albums grouped by topic
    /// </summary>
    /// <returns>Dictionary of topic to albums</returns>
    Task<Dictionary<string, List<AlbumSummaryViewModel>>> GetAlbumsByTopicAsync();
}
