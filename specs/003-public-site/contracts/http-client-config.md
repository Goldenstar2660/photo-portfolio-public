# HTTP Client Configuration

## Service Registration

```csharp
// Program.cs for .NET 9
var builder = WebApplication.CreateBuilder(args);

// HTTP Client configuration with modern patterns
builder.Services.AddHttpClient<IPhotoGalleryApiService, PhotoGalleryApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["PhotoGalleryApi:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "PhotoGallery-PublicWeb/1.0");
})
.AddStandardResilienceHandler(); // .NET 9 simplified resilience

// Memory caching
builder.Services.AddMemoryCache();

// AutoMapper
builder.Services.AddAutoMapper(typeof(PublicWebMappingProfile));

// Application services
builder.Services.AddScoped<IHomePageService, HomePageService>();
builder.Services.AddScoped<IAlbumPageService, AlbumPageService>();
builder.Services.AddScoped<INavigationService, NavigationService>();
```

## Configuration Settings

```json
// appsettings.json
{
  "PhotoGalleryApi": {
    "BaseUrl": "https://localhost:44304/api",
    "Timeout": "00:00:30",
    "RetryAttempts": 3,
    "CircuitBreakerThreshold": 5
  },
  "Caching": {
    "DefaultExpiration": "00:15:00",
    "NavigationExpiration": "01:00:00",
    "RandomPhotosExpiration": "00:05:00"
  },
  "Gallery": {
    "PageSize": 20,
    "MaxPageSize": 50,
    "RandomPhotoCount": 6,
    "FeaturedPhotoCount": 3
  }
}
```

## API Service Interface

```csharp
public interface IPhotoGalleryApiService
{
    Task<ApiResult<PagedResultDto<AlbumDto>>> GetAlbumsAsync(int page = 1, int pageSize = 20);
    Task<ApiResult<AlbumDto>> GetAlbumAsync(Guid albumId);
    Task<ApiResult<List<string>>> GetTopicsAsync();
    Task<ApiResult<PagedResultDto<AlbumDto>>> GetAlbumsByTopicAsync(string topic, int page = 1, int pageSize = 20);
    
    Task<ApiResult<PagedResultDto<PhotoDto>>> GetPhotosByAlbumAsync(Guid albumId, int page = 1, int pageSize = 20, string location = null);
    Task<ApiResult<List<PhotoDto>>> GetRandomPhotosAsync(int count = 6);
    Task<ApiResult<PhotoDto>> GetPhotoAsync(Guid photoId);
    Task<ApiResult<List<string>>> GetLocationsByAlbumAsync(Guid albumId);
}

public class ApiResult<T>
{
    public T Data { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public string CorrelationId { get; set; }

    public static ApiResult<T> Success(T data) => new() { Data = data, IsSuccess = true };
    public static ApiResult<T> Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
```

## Implementation Example

```csharp
public class PhotoGalleryApiService(
    HttpClient httpClient,
    ILogger<PhotoGalleryApiService> logger,
    IMemoryCache cache,
    IConfiguration configuration) : IPhotoGalleryApiService
{
    public async Task<ApiResult<PagedResultDto<AlbumDto>>> GetAlbumsAsync(int page = 1, int pageSize = 20)
    {
        var cacheKey = $"albums:page:{page}:size:{pageSize}";
        
        if (cache.TryGetValue(cacheKey, out PagedResultDto<AlbumDto>? cachedResult))
        {
            return ApiResult<PagedResultDto<AlbumDto>>.Success(cachedResult);
        }

        try
        {
            using var response = await httpClient.GetAsync($"albums?page={page}&pageSize={pageSize}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PagedResultDto<AlbumDto>>();

                var cacheExpiration = configuration.GetValue<TimeSpan>("Caching:DefaultExpiration");
                cache.Set(cacheKey, result, cacheExpiration);

                return ApiResult<PagedResultDto<AlbumDto>>.Success(result!);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogError("API Error: {StatusCode} - {Content}", response.StatusCode, errorContent);
            return ApiResult<PagedResultDto<AlbumDto>>.Failure($"API returned {response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error occurred while fetching albums");
            return ApiResult<PagedResultDto<AlbumDto>>.Failure("Network error occurred");
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Request timeout while fetching albums");
            return ApiResult<PagedResultDto<AlbumDto>>.Failure("Request timeout");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while fetching albums");
            return ApiResult<PagedResultDto<AlbumDto>>.Failure("An unexpected error occurred");
        }
    }

    // Additional method implementations using similar patterns...
}
```

## Error Handling Strategy

```csharp
public class ApiErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiErrorHandlingMiddleware> _logger;

    public ApiErrorHandlingMiddleware(RequestDelegate next, ILogger<ApiErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "text/html";
        
        // Redirect to error page or return error view
        context.Response.Redirect("/Error");
    }
}
```

## Health Check Configuration

```csharp
// Health check for API connectivity
public class PhotoGalleryApiHealthCheck : IHealthCheck
{
    private readonly IPhotoGalleryApiService _apiService;

    public PhotoGalleryApiHealthCheck(IPhotoGalleryApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiService.GetTopicsAsync();
            
            return result.IsSuccess 
                ? HealthCheckResult.Healthy("PhotoGallery API is responsive")
                : HealthCheckResult.Unhealthy("PhotoGallery API returned error", new Exception(result.ErrorMessage));
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PhotoGallery API is unreachable", ex);
        }
    }
}

// Registration in Program.cs
services.AddHealthChecks()
    .AddCheck<PhotoGalleryApiHealthCheck>("photogallery-api");
```