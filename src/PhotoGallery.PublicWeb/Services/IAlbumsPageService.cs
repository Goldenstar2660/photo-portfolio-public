using PhotoGallery.PublicWeb.Models;

namespace PhotoGallery.PublicWeb.Services;

/// <summary>
/// Service interface for albums page data
/// </summary>
public interface IAlbumsPageService
{
    /// <summary>
    /// Gets paginated list of albums with optional topic filter
    /// </summary>
    /// <param name="pageNumber">Current page number (1-based)</param>
    /// <param name="pageSize">Number of albums per page</param>
    /// <param name="topic">Optional topic filter</param>
    /// <returns>Albums page result with albums and total count</returns>
    Task<AlbumsPageResult> GetAlbumsAsync(int pageNumber, int pageSize, string? topic = null);
}
