using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace PhotoGallery.Services
{
    /// <summary>
    /// Service for generating consistent cache keys for application services
    /// </summary>
    public interface ICacheKeyService : IApplicationService
    {
        /// <summary>
        /// Generate a cache key for GetList operations
        /// </summary>
        /// <param name="serviceName">Name of the application service</param>
        /// <param name="methodName">Name of the method (e.g., "GetListAsync")</param>
        /// <param name="parameters">Method parameters object</param>
        /// <param name="userId">Current user ID for user-specific caching</param>
        /// <returns>A unique cache key string</returns>
    string GenerateListCacheKey(string serviceName, string methodName, object parameters);

        /// <summary>
        /// Generate a cache key prefix for invalidation patterns
        /// </summary>
        /// <param name="serviceName">Name of the application service</param>
        /// <returns>Cache key prefix for pattern matching</returns>
        string GenerateCacheKeyPrefix(string serviceName);

        /// <summary>
        /// Get cache keys for invalidation based on service name
        /// </summary>
        /// <param name="serviceName">Name of the application service</param>
        /// <returns>Array of cache key patterns to invalidate</returns>
        Task<string[]> GetInvalidationKeysAsync(string serviceName);
    }
}