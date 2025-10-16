using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using PhotoGallery.Albums;
using PhotoGallery.Photos;
using PhotoGallery.PublicWeb.Models;
using Volo.Abp.Application.Dtos;
using Volo.Abp.DependencyInjection;

namespace PhotoGallery.PublicWeb.Services;

/// <summary>
/// Service for aggregating data for the home page
/// </summary>
public class HomePageService : IHomePageService, ITransientDependency
{
    private readonly IAlbumAppService _albumAppService;
    private readonly IPhotoAppService _photoAppService;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<HomePageService> _logger;

    // Cache keys
    private const string CacheKeyFeaturedAlbums = "HomePage_FeaturedAlbums";
    private const string CacheKeyHeroPhotos = "HomePage_HeroPhotos";
    private const string CacheKeyHomeData = "HomePage_Data";

    public HomePageService(
        IAlbumAppService albumAppService,
        IPhotoAppService photoAppService,
        IMapper mapper,
        IMemoryCache cache,
        ILogger<HomePageService> logger)
    {
        _albumAppService = albumAppService;
        _photoAppService = photoAppService;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<HomeViewModel> GetHomePageDataAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKeyHomeData, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            
            _logger.LogInformation("Building home page data...");

            var viewModel = new HomeViewModel
            {
                Name = "Photography",
                IntroductionText = "Welcome to my photography portfolio. Explore my collection of photos capturing moments from my biking adventures and everyday life.",
                FeaturedAlbums = await GetFeaturedAlbumsAsync(3),
                HeroPhotos = await GetHeroPhotosAsync(5)
            };

            // Get total counts
            var allAlbumsRequest = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 1,
                SkipCount = 0
            };
            
            var albumsResult = await _albumAppService.GetListAsync(allAlbumsRequest);
            viewModel.TotalAlbums = (int)albumsResult.TotalCount;

            var photosRequest = new GetPhotosInput
            {
                MaxResultCount = 1,
                SkipCount = 0
            };
            
            var photosResult = await _photoAppService.GetListAsync(photosRequest);
            viewModel.TotalPhotos = (int)photosResult.TotalCount;

            _logger.LogInformation("Home page data built: {AlbumCount} albums, {PhotoCount} photos", 
                viewModel.TotalAlbums, viewModel.TotalPhotos);

            return viewModel;
        }) ?? new HomeViewModel();
    }

    public async Task<List<FeaturedAlbumViewModel>> GetFeaturedAlbumsAsync(int count = 3)
    {
        var cacheKey = $"{CacheKeyFeaturedAlbums}_{count}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            
            _logger.LogInformation("Fetching {Count} featured albums...", count);

            var request = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = count,
                SkipCount = 0,
                Sorting = "DisplayOrder"
            };

            var albumsResult = await _albumAppService.GetListAsync(request);
            var featuredAlbums = new List<FeaturedAlbumViewModel>();

            foreach (var albumDto in albumsResult.Items)
            {
                var featuredAlbum = _mapper.Map<AlbumDto, FeaturedAlbumViewModel>(albumDto);
                featuredAlbum.PhotoCount = albumDto.PhotoCount;
                
                // Set cover photo URL (prefer a random photo from album; fallback to configured cover)
                if (albumDto.PhotoCount > 0)
                {
                    var randomReq = new GetRandomPhotosInput { Count = 1, IncludeAlbumId = albumDto.Id };
                    var random = await _photoAppService.GetRandomPhotosAsync(randomReq);
                    if (random.Any())
                    {
                        featuredAlbum.CoverPhotoUrl = random.First().FilePath;
                    }
                    else if (!string.IsNullOrEmpty(albumDto.CoverImagePath))
                    {
                        featuredAlbum.CoverPhotoUrl = albumDto.CoverImagePath;
                    }
                }
                else if (!string.IsNullOrEmpty(albumDto.CoverImagePath))
                {
                    featuredAlbum.CoverPhotoUrl = albumDto.CoverImagePath;
                }

                featuredAlbums.Add(featuredAlbum);
            }

            _logger.LogInformation("Retrieved {Count} featured albums", featuredAlbums.Count);

            return featuredAlbums;
        }) ?? new List<FeaturedAlbumViewModel>();
    }

    public async Task<List<RandomPhotoViewModel>> GetHeroPhotosAsync(int count = 5)
    {
        var cacheKey = $"{CacheKeyHeroPhotos}_{count}";
        
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            
            _logger.LogInformation("Fetching {Count} random hero photos...", count);

            var request = new GetRandomPhotosInput
            {
                Count = count
            };

            var randomPhotos = await _photoAppService.GetRandomPhotosAsync(request);
            var heroPhotos = new List<RandomPhotoViewModel>();

            foreach (var photoDto in randomPhotos)
            {
                var heroPhoto = _mapper.Map<PhotoDto, RandomPhotoViewModel>(photoDto);
                heroPhoto.Title = photoDto.FileName;
                heroPhoto.AlbumName = photoDto.AlbumName;
                
                heroPhotos.Add(heroPhoto);
            }

            _logger.LogInformation("Retrieved {Count} hero photos", heroPhotos.Count);

            return heroPhotos;
        }) ?? new List<RandomPhotoViewModel>();
    }
}
