using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using PhotoGallery.Albums;
using PhotoGallery.Photos;
using PhotoGallery.PublicWeb.Models;
using Volo.Abp.Application.Dtos;
using Volo.Abp.DependencyInjection;

namespace PhotoGallery.PublicWeb.Services;

/// <summary>
/// Service for retrieving album and photo data with caching
/// </summary>
public class AlbumPageService : IAlbumPageService, ITransientDependency
{
    private readonly IAlbumAppService _albumAppService;
    private readonly IPhotoAppService _photoAppService;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AlbumPageService> _logger;

    // Cache keys
    private const string CacheKeyAlbumTopics = "Album_Topics";
    private const string CacheKeyAlbumsByTopic = "Albums_ByTopic";

    public AlbumPageService(
        IAlbumAppService albumAppService,
        IPhotoAppService photoAppService,
        IMapper mapper,
        IMemoryCache cache,
        ILogger<AlbumPageService> logger)
    {
        _albumAppService = albumAppService;
        _photoAppService = photoAppService;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<AlbumViewModel?> GetAlbumByIdAsync(Guid albumId, int page = 1, int pageSize = 50)
    {
        var cacheKey = $"Album_{albumId}_Page{page}_Size{pageSize}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            // Cache for 5 minutes for photo data
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            
            _logger.LogInformation("Fetching album {AlbumId} with pagination (page: {Page}, size: {PageSize})", 
                albumId, page, pageSize);

            try
            {
                // Get album details (cached separately for 15 minutes)
                var albumDto = await GetAlbumDtoAsync(albumId);
                if (albumDto == null)
                {
                    _logger.LogWarning("Album {AlbumId} not found", albumId);
                    return null;
                }

                var albumViewModel = _mapper.Map<AlbumDto, AlbumViewModel>(albumDto);

                // Get photos for this album with pagination
                var photosRequest = new PagedAndSortedResultRequestDto
                {
                    MaxResultCount = pageSize,
                    SkipCount = (page - 1) * pageSize,
                    Sorting = "DisplayOrder"
                };

                var photosResult = await _photoAppService.GetByAlbumAsync(albumId, photosRequest);
                
                albumViewModel.Photos = photosResult.Items
                    .Select(photoDto =>
                    {
                        var photoViewModel = _mapper.Map<PhotoDto, PhotoViewModel>(photoDto);
                        photoViewModel.AlbumName = albumDto.Name;
                        return photoViewModel;
                    })
                    .ToList();

                albumViewModel.TotalPhotoCount = (int)photosResult.TotalCount;
                albumViewModel.CurrentPage = page;
                albumViewModel.PageSize = pageSize;

                _logger.LogInformation("Retrieved album {AlbumName} with {PhotoCount} photos (total: {TotalCount})", 
                    albumViewModel.Name, albumViewModel.Photos.Count, albumViewModel.TotalPhotoCount);

                return albumViewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching album {AlbumId}", albumId);
                return null;
            }
        });
    }

    public async Task<List<AlbumTopicViewModel>> GetAlbumTopicsAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKeyAlbumTopics, async entry =>
        {
            // Cache for 30 minutes (navigation data changes infrequently)
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            
            _logger.LogInformation("Fetching album topics for navigation...");

            var topics = await _albumAppService.GetTopicsAsync();
            var albumTopics = new List<AlbumTopicViewModel>();

            foreach (var topic in topics)
            {
                var topicRequest = new PagedAndSortedResultRequestDto
                {
                    MaxResultCount = 100, // Get all albums for this topic
                    SkipCount = 0,
                    Sorting = "DisplayOrder"
                };

                var albumsResult = await _albumAppService.GetByTopicAsync(topic, topicRequest);
                
                var albumSummaries = albumsResult.Items
                    .Select(albumDto =>
                    {
                        var summary = _mapper.Map<AlbumDto, AlbumSummaryViewModel>(albumDto);
                        summary.PhotoCount = albumDto.PhotoCount;
                        return summary;
                    })
                    .ToList();

                albumTopics.Add(new AlbumTopicViewModel
                {
                    Topic = topic,
                    AlbumCount = albumSummaries.Count,
                    Albums = albumSummaries
                });
            }

            _logger.LogInformation("Retrieved {TopicCount} album topics with {AlbumCount} total albums", 
                albumTopics.Count, albumTopics.Sum(t => t.AlbumCount));

            return albumTopics;
        }) ?? new List<AlbumTopicViewModel>();
    }

    public async Task<Dictionary<string, List<AlbumSummaryViewModel>>> GetAlbumsByTopicAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKeyAlbumsByTopic, async entry =>
        {
            // Cache for 30 minutes
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            
            _logger.LogInformation("Fetching albums grouped by topic...");

            var topics = await GetAlbumTopicsAsync();
            var albumsByTopic = new Dictionary<string, List<AlbumSummaryViewModel>>();

            foreach (var topic in topics)
            {
                albumsByTopic[topic.Topic] = topic.Albums;
            }

            _logger.LogInformation("Retrieved albums for {TopicCount} topics", albumsByTopic.Count);

            return albumsByTopic;
        }) ?? new Dictionary<string, List<AlbumSummaryViewModel>>();
    }

    /// <summary>
    /// Helper method to get album DTO with longer caching (15 minutes for album metadata)
    /// </summary>
    private async Task<AlbumDto?> GetAlbumDtoAsync(Guid albumId)
    {
        var cacheKey = $"AlbumDto_{albumId}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            // Cache album metadata for 15 minutes (changes less frequently than photos)
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            
            try
            {
                return await _albumAppService.GetAsync(albumId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching album DTO for {AlbumId}", albumId);
                return null;
            }
        });
    }
}
