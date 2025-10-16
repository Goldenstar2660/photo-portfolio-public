using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using PhotoGallery.Albums;
using PhotoGallery.Photos;
using PhotoGallery.PublicWeb.Models;
using Volo.Abp.Application.Dtos;
using Volo.Abp.DependencyInjection;

namespace PhotoGallery.PublicWeb.Services;

/// <summary>
/// Service for providing albums page data
/// </summary>
public class AlbumsPageService : IAlbumsPageService, ITransientDependency
{
    private readonly IAlbumAppService _albumAppService;
    private readonly IPhotoAppService _photoAppService;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AlbumsPageService> _logger;

    private const string CacheKeyPrefix = "AlbumsPage";

    public AlbumsPageService(
        IAlbumAppService albumAppService,
        IPhotoAppService photoAppService,
        IMapper mapper,
        IMemoryCache cache,
        ILogger<AlbumsPageService> logger)
    {
        _albumAppService = albumAppService;
        _photoAppService = photoAppService;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<AlbumsPageResult> GetAlbumsAsync(int pageNumber, int pageSize, string? topic = null)
    {
        var cacheKey = $"{CacheKeyPrefix}_{pageNumber}_{pageSize}_{topic ?? "all"}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);

            _logger.LogInformation("Fetching albums page {Page} with {PageSize} items, topic: {Topic}", 
                pageNumber, pageSize, topic ?? "all");

            var skipCount = (pageNumber - 1) * pageSize;

            var request = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = pageSize,
                SkipCount = skipCount,
                Sorting = "DisplayOrder, Name"
            };

            // TODO: Add topic filtering when AlbumAppService supports it
            var albumsResult = await _albumAppService.GetListAsync(request);

            var albums = new List<AlbumCardViewModel>();

            foreach (var albumDto in albumsResult.Items)
            {
                // Filter by topic if specified (client-side filtering for now)
                if (!string.IsNullOrEmpty(topic) && 
                    !albumDto.Topic.Equals(topic, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var albumCard = _mapper.Map<AlbumDto, AlbumCardViewModel>(albumDto);
                albumCard.PhotoCount = albumDto.PhotoCount;

                // Set cover photo URL: use a random photo from the album if available
                if (albumDto.PhotoCount > 0)
                {
                    var randomReq = new GetRandomPhotosInput { Count = 1, IncludeAlbumId = albumDto.Id };
                    var random = await _photoAppService.GetRandomPhotosAsync(randomReq);
                    if (random.Any())
                    {
                        albumCard.CoverPhotoUrl = random.First().FilePath;
                    }
                    else if (!string.IsNullOrEmpty(albumDto.CoverImagePath))
                    {
                        albumCard.CoverPhotoUrl = albumDto.CoverImagePath;
                    }
                }
                else if (!string.IsNullOrEmpty(albumDto.CoverImagePath))
                {
                    albumCard.CoverPhotoUrl = albumDto.CoverImagePath;
                }

                albums.Add(albumCard);
            }

            var result = new AlbumsPageResult
            {
                Albums = albums,
                TotalCount = (int)albumsResult.TotalCount
            };

            _logger.LogInformation("Retrieved {Count} albums for page {Page} (total: {Total})", 
                albums.Count, pageNumber, result.TotalCount);

            return result;
    }) ?? new AlbumsPageResult();
    }
}
