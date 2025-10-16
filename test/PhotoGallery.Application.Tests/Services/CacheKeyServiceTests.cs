using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Xunit;

namespace PhotoGallery.Services
{
    public class CacheKeyServiceTests : PhotoGalleryApplicationTestBase<PhotoGalleryApplicationTestModule>
    {
        private readonly ICacheKeyService _cacheKeyService;
        private readonly IMemoryCache _memoryCache;

        public CacheKeyServiceTests()
        {
            _cacheKeyService = GetRequiredService<ICacheKeyService>();
            _memoryCache = GetRequiredService<IMemoryCache>();
        }

        [Fact]
        public void GenerateListCacheKey_Should_Return_Consistent_Key_For_Same_Parameters()
        {
            // Arrange
            var serviceName = "AlbumAppService";
            var methodName = "GetListAsync";
            var parameters = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "Name"
            };

            // Act
            var cacheKey1 = _cacheKeyService.GenerateListCacheKey(serviceName, methodName, parameters);
            var cacheKey2 = _cacheKeyService.GenerateListCacheKey(serviceName, methodName, parameters);

            // Assert
            cacheKey1.ShouldBe(cacheKey2);
            cacheKey1.ShouldNotBeNull();
            cacheKey1.ShouldNotBeEmpty();
        }

        [Fact]
        public void GenerateListCacheKey_Should_Return_Different_Keys_For_Different_Parameters()
        {
            // Arrange
            var serviceName = "AlbumAppService";
            var methodName = "GetListAsync";
            var parameters1 = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "Name"
            };
            var parameters2 = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 20,
                SkipCount = 0,
                Sorting = "Name"
            };

            // Act
            var cacheKey1 = _cacheKeyService.GenerateListCacheKey(serviceName, methodName, parameters1);
            var cacheKey2 = _cacheKeyService.GenerateListCacheKey(serviceName, methodName, parameters2);

            // Assert
            cacheKey1.ShouldNotBe(cacheKey2);
        }

        [Fact]
        public void GenerateListCacheKey_Should_Return_Different_Keys_For_Different_Services()
        {
            // Arrange
            var methodName = "GetListAsync";
            var parameters = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "Name"
            };

            // Act
            var cacheKey1 = _cacheKeyService.GenerateListCacheKey("AlbumAppService", methodName, parameters);
            var cacheKey2 = _cacheKeyService.GenerateListCacheKey("PhotoAppService", methodName, parameters);

            // Assert
            cacheKey1.ShouldNotBe(cacheKey2);
        }

        [Fact]
        public void GenerateListCacheKey_Should_Handle_Null_Parameters()
        {
            // Arrange
            var serviceName = "AlbumAppService";
            var methodName = "GetListAsync";

            // Act
            var cacheKey = _cacheKeyService.GenerateListCacheKey(serviceName, methodName, null!);

            // Assert
            cacheKey.ShouldNotBeNull();
            cacheKey.ShouldNotBeEmpty();
            cacheKey.ShouldContain("null");
        }

        [Fact]
        public void GenerateCacheKeyPrefix_Should_Return_Service_Prefix()
        {
            // Arrange
            var serviceName = "AlbumAppService";

            // Act
            var prefix = _cacheKeyService.GenerateCacheKeyPrefix(serviceName);

            // Assert
            prefix.ShouldBe("AlbumAppService:");
            prefix.ShouldStartWith(serviceName);
        }

        [Fact]
        public async Task GetInvalidationKeysAsync_Should_Return_Registered_Keys()
        {
            // Arrange
            var serviceName = "AlbumAppService";
            var methodName = "GetListAsync";
            var parameters1 = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "Name"
            };
            var parameters2 = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 20,
                SkipCount = 0,
                Sorting = "DisplayOrder"
            };

            // Act - Generate cache keys to register them
            var cacheKey1 = _cacheKeyService.GenerateListCacheKey(serviceName, methodName, parameters1);
            var cacheKey2 = _cacheKeyService.GenerateListCacheKey(serviceName, methodName, parameters2);

            // Get invalidation keys
            var invalidationKeys = await _cacheKeyService.GetInvalidationKeysAsync(serviceName);

            // Assert
            invalidationKeys.ShouldNotBeNull();
            invalidationKeys.Length.ShouldBeGreaterThanOrEqualTo(2);
            invalidationKeys.ShouldContain(cacheKey1);
            invalidationKeys.ShouldContain(cacheKey2);
        }

        [Fact]
        public async Task GetInvalidationKeysAsync_Should_Return_Empty_Array_For_Unknown_Service()
        {
            // Arrange
            var unknownServiceName = "UnknownService";

            // Act
            var invalidationKeys = await _cacheKeyService.GetInvalidationKeysAsync(unknownServiceName);

            // Assert
            invalidationKeys.ShouldNotBeNull();
            invalidationKeys.Length.ShouldBe(0);
        }

        [Fact]
        public void GenerateListCacheKey_Should_Include_Service_And_Method_Names()
        {
            // Arrange
            var serviceName = "AlbumAppService";
            var methodName = "GetListAsync";
            var parameters = new PagedAndSortedResultRequestDto();

            // Act
            var cacheKey = _cacheKeyService.GenerateListCacheKey(serviceName, methodName, parameters);

            // Assert
            cacheKey.ShouldContain(serviceName);
            cacheKey.ShouldContain(methodName);
            cacheKey.ShouldContain(":");
        }

        [Fact]
        public void GenerateListCacheKey_Should_Handle_Complex_Parameters()
        {
            // Arrange
            var serviceName = "PhotoAppService";
            var methodName = "GetListAsync";
            var parameters = new
            {
                AlbumId = Guid.NewGuid(),
                Location = "Paris",
                DateTakenFrom = new DateTime(2023, 1, 1),
                DateTakenTo = new DateTime(2023, 12, 31),
                MaxResultCount = 25,
                SkipCount = 50,
                Sorting = "DateTaken desc"
            };

            // Act
            var cacheKey1 = _cacheKeyService.GenerateListCacheKey(serviceName, methodName, parameters);
            var cacheKey2 = _cacheKeyService.GenerateListCacheKey(serviceName, methodName, parameters);

            // Assert
            cacheKey1.ShouldBe(cacheKey2);
            cacheKey1.ShouldNotBeNull();
            cacheKey1.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task GenerateListCacheKey_Should_Register_Key_In_Memory_Cache()
        {
            // Arrange
            var serviceName = "TestService";
            var methodName = "GetListAsync";
            var parameters = new { Test = "Value" };

            // Act
            var cacheKey = _cacheKeyService.GenerateListCacheKey(serviceName, methodName, parameters);

            // Assert - Verify the key is registered by checking invalidation keys
            var invalidationKeys = await _cacheKeyService.GetInvalidationKeysAsync(serviceName);
            
            invalidationKeys.ShouldContain(cacheKey);
        }
    }
}