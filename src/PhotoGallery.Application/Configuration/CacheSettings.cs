using System;
namespace PhotoGallery.Application.Configuration
{
    public class CacheSettings
    {
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan SlidingExpiration { get; set; } = TimeSpan.FromMinutes(2);
    public TimeSpan AbsoluteExpirationRelativeToNow { get; set; } = TimeSpan.FromMinutes(15);
    public bool GetListCacheEnabled { get; set; } = true;
    public string CacheKeyPrefix { get; set; } = "PhotoGallery";
    }
}
