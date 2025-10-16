# Data Model Design

## Overview

This document defines the data structures for the PhotoGallery public website, including ViewModels for Razor Pages, direct service integration, and data transformation patterns. Designed for .NET 9 with modern C# features including primary constructors, required properties, and improved nullable reference types.

## Direct Service Integration Approach

Instead of HTTP API calls, the public web project directly references the backend application services for optimal performance:

### Service Dependencies
- **PhotoGallery.Application** - Direct reference for application services
- **PhotoGallery.Application.Contracts** - Shared DTOs and interfaces  
- **PhotoGallery.EntityFrameworkCore** - Database context for dependency injection
- **Shared Infrastructure** - Logging, caching, and configuration

### Benefits
- **Performance**: Eliminates HTTP serialization/deserialization overhead
- **Type Safety**: Compile-time checking of service contracts
- **Simplified Error Handling**: Direct exception handling without HTTP layer
- **Transactional Integrity**: Same-process database transactions
- **Debugging**: Direct debugging through service layers

## Backend API Data Contracts

Based on the backend API specification analysis, the following DTOs are available:

### AlbumDto (from PhotoGallery.Application.Contracts)
```csharp
public class AlbumDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }          // max 100 chars, unique
    public string Topic { get; set; }         // max 50 chars
    public string Description { get; set; }   // max 500 chars, optional
    public int DisplayOrder { get; set; }
    public string CoverImagePath { get; set; } // optional
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}
```

### PhotoDto (from PhotoGallery.Application.Contracts)
```csharp
public class PhotoDto
{
    public Guid Id { get; set; }
    public Guid AlbumId { get; set; }
    public string FileName { get; set; }      // original filename
    public string FilePath { get; set; }      // server path
    public string ThumbnailPath { get; set; } // optional thumbnail
    public long FileSize { get; set; }        // bytes
    public string Caption { get; set; }       // max 200 chars, optional
    public string Location { get; set; }      // max 100 chars, optional
    public int DisplayOrder { get; set; }
    public DateTime DateTaken { get; set; }
    public DateTime CreationTime { get; set; }
}
```

## Public Web ViewModels

### HomeViewModel
Purpose: Aggregates data for the home page display

```csharp
public class HomeViewModel
{
    public required PersonalInfoViewModel PersonalInfo { get; set; }
    public required List<RandomPhotoViewModel> RandomPhotos { get; set; }
    public required List<FeaturedAlbumViewModel> FeaturedAlbums { get; set; }
}

public class PersonalInfoViewModel
{
    public string Name { get; set; } = "Your Name";
    public string ProfileImagePath { get; set; } = "/img/profile.jpg";
    public string BackgroundImagePath { get; set; } = "/img/hero-bg.jpg";
    public string ShortBio { get; set; } = "Photography enthusiast with passion for biking and drone photography";
}

public record RandomPhotoViewModel(
    Guid PhotoId,
    string ThumbnailPath,
    string FilePath,
    string? Caption,
    string AlbumName,
    Guid AlbumId,
    string? Location
);

public record FeaturedAlbumViewModel(
    Guid AlbumId,
    string Name,
    string Topic,
    string? CoverImagePath,
    int PhotoCount
);
```

### AboutViewModel
Purpose: Static content for the about page

```csharp
public class AboutViewModel
{
    public PersonalInfoViewModel PersonalInfo { get; set; }
    public List<HobbyViewModel> Hobbies { get; set; }
    public List<StatisticViewModel> Statistics { get; set; }
}

public class HobbyViewModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string IconClass { get; set; } // FontAwesome icon class
}

public class StatisticViewModel
{
    public string Label { get; set; }
    public string Value { get; set; }
    public string IconClass { get; set; }
}
```

### PhotoDetailViewModel
Purpose: Enhanced photo data for lightbox display (User Story 2)

```csharp
public class PhotoDetailViewModel
{
    public required Guid PhotoId { get; set; }
    public required string FilePath { get; set; }
    public required string FileName { get; set; }
    public string? Caption { get; set; }
    public string? Location { get; set; }
    public required DateTime DateTaken { get; set; }
    public required long FileSize { get; set; }
    public string FormattedDate => DateTaken.ToString("MMMM dd, yyyy");
    public string FormattedFileSize => FormatFileSize(FileSize);
    
    // Navigation within album
    public Guid? PreviousPhotoId { get; set; }
    public Guid? NextPhotoId { get; set; }
    public int PhotoIndex { get; set; }
    public int TotalPhotos { get; set; }
    
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB"];
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
```

### AlbumViewModel
Purpose: Displays album details with photos and location filtering

```csharp
public class AlbumViewModel
{
    public AlbumDto Album { get; set; }
    public List<PhotoViewModel> Photos { get; set; }
    public List<string> Locations { get; set; }
    public string SelectedLocation { get; set; }
    public List<PhotoViewModel> FeaturedPhotos { get; set; } // Top 3 random photos
    public PaginationViewModel Pagination { get; set; }
}

public class PhotoViewModel
{
    public required Guid PhotoId { get; set; }
    public required string FileName { get; set; }
    public required string ThumbnailPath { get; set; }
    public required string FilePath { get; set; }
    public string? Caption { get; set; }
    public string? Location { get; set; }
    public required DateTime DateTaken { get; set; }
    public required long FileSize { get; set; }
    
    public string FormattedFileSize => FormatFileSize(FileSize);
    
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB"];
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

public class PaginationViewModel
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}
```

### NavigationViewModel
Purpose: Shared data for site navigation

```csharp
public class NavigationViewModel
{
    public List<AlbumDropdownItemViewModel> Albums { get; set; }
    public List<TopicGroupViewModel> TopicGroups { get; set; }
}

public class AlbumDropdownItemViewModel
{
    public Guid AlbumId { get; set; }
    public string Name { get; set; }
    public string Topic { get; set; }
    public string Url { get; set; }
}

public class TopicGroupViewModel
{
    public string Topic { get; set; }
    public List<AlbumDropdownItemViewModel> Albums { get; set; }
}
```

## API Service Interfaces

## Direct Application Service Integration

### IAlbumAppService (from PhotoGallery.Application.Contracts)
```csharp
public interface IAlbumAppService : IApplicationService
{
    Task<PagedResultDto<AlbumDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<AlbumDto> GetAsync(Guid id);
    Task<AlbumDto> CreateAsync(CreateAlbumDto input);
    Task<AlbumDto> UpdateAsync(Guid id, UpdateAlbumDto input);
    Task DeleteAsync(Guid id);
    Task<List<string>> GetTopicsAsync();
    Task<List<AlbumDto>> GetByTopicAsync(string topic);
}

public interface IPhotoAppService : IApplicationService
{
    Task<PagedResultDto<PhotoDto>> GetListByAlbumAsync(Guid albumId, PagedAndSortedResultRequestDto input);
    Task<List<PhotoDto>> GetRandomPhotosAsync(int count);
    Task<PhotoDto> GetAsync(Guid id);
    Task DeleteAsync(Guid id);
    Task<List<string>> GetLocationsByAlbumAsync(Guid albumId);
}
```

### Service Adapter Pattern
```csharp
// Adapter interface for the public web application
public interface IGalleryService
{
    Task<List<AlbumDto>> GetAlbumsAsync(CancellationToken cancellationToken = default);
    Task<AlbumDto?> GetAlbumAsync(Guid albumId, CancellationToken cancellationToken = default);
    Task<List<string>> GetTopicsAsync(CancellationToken cancellationToken = default);
    Task<List<AlbumDto>> GetAlbumsByTopicAsync(string topic, CancellationToken cancellationToken = default);
    
    Task<List<PhotoDto>> GetPhotosByAlbumAsync(Guid albumId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<List<PhotoDto>> GetRandomPhotosAsync(int count = 6, CancellationToken cancellationToken = default);
    Task<PhotoDto?> GetPhotoAsync(Guid photoId, CancellationToken cancellationToken = default);
    Task<List<string>> GetLocationsByAlbumAsync(Guid albumId, CancellationToken cancellationToken = default);
}

// Implementation that wraps the ABP application services
public class GalleryService(
    IAlbumAppService albumAppService,
    IPhotoAppService photoAppService,
    IMemoryCache cache,
    ILogger<GalleryService> logger) : IGalleryService
{
    public async Task<List<AlbumDto>> GetAlbumsAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "albums:all";
        
        if (cache.TryGetValue(cacheKey, out List<AlbumDto>? cachedAlbums))
        {
            return cachedAlbums;
        }

        try
        {
            var result = await albumAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 1000, // Get all albums for public display
                Sorting = "DisplayOrder"
            });

            var albums = result.Items.ToList();
            cache.Set(cacheKey, albums, TimeSpan.FromMinutes(15));
            
            return albums;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving albums");
            return [];
        }
    }

    public async Task<List<PhotoDto>> GetRandomPhotosAsync(int count = 6, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"photos:random:{count}";
        
        if (cache.TryGetValue(cacheKey, out List<PhotoDto>? cachedPhotos))
        {
            return cachedPhotos;
        }

        try
        {
            var photos = await photoAppService.GetRandomPhotosAsync(count);
            cache.Set(cacheKey, photos, TimeSpan.FromMinutes(5));
            
            return photos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving random photos");
            return [];
        }
    }
    
    // Additional method implementations...
}
```

## Data Transformation Patterns

### DTO to ViewModel Mapping
Using AutoMapper profiles for consistent transformation:

```csharp
public class PublicWebMappingProfile : Profile
{
    public PublicWebMappingProfile()
    {
        CreateMap<AlbumDto, AlbumDropdownItemViewModel>()
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => $"/Albums/{src.Id}"));
            
        CreateMap<PhotoDto, PhotoViewModel>();
        
        CreateMap<PhotoDto, RandomPhotoViewModel>()
            .ForMember(dest => dest.PhotoId, opt => opt.MapFrom(src => src.Id));
    }
}
```

### Page-Specific Data Aggregation
Service classes that compose multiple API calls:

```csharp
public interface IHomePageService
{
    Task<HomeViewModel> GetHomePageDataAsync();
}

public interface IAlbumPageService
{
    Task<AlbumViewModel> GetAlbumPageDataAsync(Guid albumId, string location = null, int page = 1);
    Task<NavigationViewModel> GetNavigationDataAsync();
}
```

## Caching Strategy

### Cache Keys and Expiration
```csharp
public static class CacheKeys
{
    public const string AllAlbums = "albums:all";
    public const string AlbumsByTopic = "albums:topic:{0}";
    public const string AlbumDetails = "album:{0}";
    public const string AlbumPhotos = "album:{0}:photos:page:{1}";
    public const string RandomPhotos = "photos:random:{0}";
    public const string Navigation = "navigation:data";
    
    public static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan NavigationExpiry = TimeSpan.FromHours(1);
    public static readonly TimeSpan RandomPhotosExpiry = TimeSpan.FromMinutes(5);
}
```

## Error Handling Models

### API Error Response
```csharp
public class ApiErrorResponse
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; }
    public string CorrelationId { get; set; }
}

public class ApiResult<T>
{
    public T Data { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; }
}
```

### Fallback Data Models
```csharp
public static class FallbackData
{
    public static readonly List<AlbumDropdownItemViewModel> DefaultAlbums = new()
    {
        new() { AlbumId = Guid.Empty, Name = "Gallery Unavailable", Topic = "System", Url = "#" }
    };
    
    public static readonly HomeViewModel DefaultHomeData = new()
    {
        PersonalInfo = new PersonalInfoViewModel(),
        RandomPhotos = new List<RandomPhotoViewModel>(),
        FeaturedAlbums = new List<FeaturedAlbumViewModel>()
    };
}
```

## Validation and Business Rules

### Client-Side Validation Models
```csharp
public class PhotoSearchViewModel
{
    [StringLength(100)]
    public string Location { get; set; }
    
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
    
    [Range(1, 50)]
    public int PageSize { get; set; } = 20;
}
```

### Data Consistency Rules
1. **Album Navigation**: Albums must have valid IDs and non-empty names
2. **Photo Display**: Photos must have valid thumbnail paths or fallback images
3. **Location Filtering**: Location strings are trimmed and case-insensitive
4. **Pagination**: Page numbers are 1-based with reasonable size limits
5. **Caching**: Cache invalidation based on data freshness requirements

## Performance Considerations

### Lazy Loading Strategy
- Images load progressively with intersection observer
- API calls are paginated to limit data transfer
- ViewModels include only necessary data for each page

### Memory Management
- Cache entries have appropriate expiration times
- Large image collections use pagination
- Dispose pattern for HttpClient and resources

### SEO Optimization
- ViewModels include metadata for page titles and descriptions
- Structured data markup for photo galleries
- Server-side rendering ensures content is crawlable