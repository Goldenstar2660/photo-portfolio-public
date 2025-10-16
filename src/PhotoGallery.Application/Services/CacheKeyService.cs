using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace PhotoGallery.Services
{
    /// <summary>
    /// Service for generating consistent cache keys for application services
    /// </summary>
    public class CacheKeyService : ApplicationService, ICacheKeyService, ITransientDependency
    {
        private readonly IMemoryCache _memoryCache;
        private const string KEY_SEPARATOR = ":";
        private const string CACHE_KEYS_REGISTRY = "CacheKeysRegistry";

        public CacheKeyService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Generate a cache key for GetList operations
        /// </summary>
        public string GenerateListCacheKey(string serviceName, string methodName, object parameters)
        {
            var parameterHash = GenerateParameterHash(parameters);
            var cacheKey = $"{serviceName}{KEY_SEPARATOR}{methodName}{KEY_SEPARATOR}{parameterHash}";
            // Register this key for later invalidation
            RegisterCacheKey(serviceName, cacheKey);
            return cacheKey;
        }

        /// <summary>
        /// Generate a cache key prefix for invalidation patterns
        /// </summary>
        public string GenerateCacheKeyPrefix(string serviceName)
        {
            return $"{serviceName}{KEY_SEPARATOR}";
        }

        /// <summary>
        /// Get cache keys for invalidation based on service name
        /// </summary>
        public async Task<string[]> GetInvalidationKeysAsync(string serviceName)
        {
            return await Task.FromResult(GetRegisteredKeys(serviceName));
        }

        /// <summary>
        /// Generate a hash for method parameters to ensure consistent cache keys
        /// </summary>
        private string GenerateParameterHash(object parameters)
        {
            if (parameters == null)
                return "null";

            try
            {
                // Serialize parameters to JSON for consistent hashing
                var json = JsonSerializer.Serialize(parameters, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                // Generate MD5 hash of the JSON string
                using (var md5 = MD5.Create())
                {
                    var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(json));
                    return Convert.ToHexString(hash).ToLowerInvariant();
                }
            }
            catch
            {
                // Fallback to simple string representation
                return parameters.ToString()?.GetHashCode().ToString("x8") ?? "error";
            }
        }

        /// <summary>
        /// Register a cache key for later invalidation tracking
        /// </summary>
        private void RegisterCacheKey(string serviceName, string cacheKey)
        {
            var registryKey = $"{CACHE_KEYS_REGISTRY}{KEY_SEPARATOR}{serviceName}";
            
            if (!_memoryCache.TryGetValue(registryKey, out HashSet<string>? keys) || keys == null)
            {
                keys = new HashSet<string>();
            }
            
            keys.Add(cacheKey);
            
            // Store the registry with a longer expiration than individual cache items
            _memoryCache.Set(registryKey, keys, TimeSpan.FromHours(1));
        }

        /// <summary>
        /// Get all registered cache keys for a service
        /// </summary>
        private string[] GetRegisteredKeys(string serviceName)
        {
            var registryKey = $"{CACHE_KEYS_REGISTRY}{KEY_SEPARATOR}{serviceName}";
            
            if (_memoryCache.TryGetValue(registryKey, out HashSet<string>? keys) && keys != null)
            {
                return keys.ToArray();
            }
            
            return Array.Empty<string>();
        }
    }
}