using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PhotoGallery.Albums;
using PhotoGallery.Photos;
using Volo.Abp.Application.Dtos;

namespace PhotoGallery.PublicWeb.Pages;

public class DebugModel : PageModel
{
    private readonly IAlbumAppService _albumAppService;
    private readonly IPhotoAppService _photoAppService;
    private readonly ILogger<DebugModel> _logger;

    public DebugModel(
        IAlbumAppService albumAppService,
        IPhotoAppService photoAppService,
        ILogger<DebugModel> logger)
    {
        _albumAppService = albumAppService;
        _photoAppService = photoAppService;
        _logger = logger;
    }

    public PagedResultDto<AlbumDto> Albums { get; set; } = new();
    public Dictionary<string, PagedResultDto<PhotoDto>> PhotosByAlbum { get; set; } = new();
    public string ErrorMessage { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Debug page: Fetching all albums");
            
            // Get all albums
            Albums = await _albumAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 100,
                Sorting = nameof(AlbumDto.DisplayOrder)
            });

            _logger.LogInformation("Debug page: Found {Count} albums", Albums.TotalCount);

            // Get photos for each album
            foreach (var album in Albums.Items)
            {
                _logger.LogInformation("Debug page: Fetching photos for album {AlbumName}", album.Name);
                var photos = await _photoAppService.GetByAlbumAsync(album.Id, new PagedAndSortedResultRequestDto
                {
                    MaxResultCount = 100
                });
                PhotosByAlbum[album.Name] = photos;
                _logger.LogInformation("Debug page: Album {AlbumName} has {PhotoCount} photos", album.Name, photos.TotalCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading debug information");
            ErrorMessage = $"Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
        }
    }
}
