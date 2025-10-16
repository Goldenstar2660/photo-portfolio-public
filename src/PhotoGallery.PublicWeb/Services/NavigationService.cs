using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using PhotoGallery.Albums;
using PhotoGallery.PublicWeb.Models;
using Volo.Abp.Application.Dtos;
using Volo.Abp.DependencyInjection;

namespace PhotoGallery.PublicWeb.Services;

/// <summary>
/// Service for site navigation data with optimized caching
/// </summary>
public class NavigationService : INavigationService, ITransientDependency
{
    private readonly IAlbumAppService _albumAppService;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<NavigationService> _logger;

    // Cache key for navigation data
    private const string CacheKeyNavigation = "Navigation_AlbumTopics";

    public NavigationService(
        IAlbumAppService albumAppService,
        IMapper mapper,
        IMemoryCache cache,
        ILogger<NavigationService> logger)
    {
        _albumAppService = albumAppService;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Gets album topics grouped for navigation dropdown with 30-minute caching
    /// </summary>
    public async Task<List<AlbumTopicViewModel>> GetAlbumTopicsAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKeyNavigation, async entry =>
        {
            // Cache for 30 minutes - navigation data changes infrequently
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            
            _logger.LogInformation("Fetching album topics for navigation menu");

            try
            {
                // Get all albums ordered by DisplayOrder
                var albumsResult = await _albumAppService.GetListAsync(new PagedAndSortedResultRequestDto
                {
                    MaxResultCount = 1000,
                    Sorting = nameof(AlbumDto.DisplayOrder)
                });

                // Group albums by topic
                var albumsByTopic = albumsResult.Items
                    .GroupBy(a => a.Topic ?? "Other")
                    .OrderBy(g => g.Key)
                    .Select(g => new AlbumTopicViewModel
                    {
                        Topic = g.Key,
                        AlbumCount = g.Count(),
                        Albums = g.Select(a => new AlbumSummaryViewModel
                        {
                            Id = a.Id,
                            Name = a.Name,
                            PhotoCount = a.PhotoCount
                        }).ToList()
                    })
                    .ToList();

                _logger.LogInformation("Successfully fetched {TopicCount} topics with {AlbumCount} albums for navigation",
                    albumsByTopic.Count, albumsResult.Items.Count);

                return albumsByTopic;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching album topics for navigation");
                // Return empty list on error to prevent crashes
                return new List<AlbumTopicViewModel>();
            }
        });
    }
}
