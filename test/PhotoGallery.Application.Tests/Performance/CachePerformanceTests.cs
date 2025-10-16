using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace PhotoGallery.Performance
{
    public class CachePerformanceTests : PhotoGalleryApplicationTestBase<PhotoGalleryApplicationTestModule>
    {
        private readonly Albums.IAlbumAppService _albumAppService;
        private readonly Photos.IPhotoAppService _photoAppService;
        private readonly IRepository<Albums.Album, Guid> _albumRepository;
        private readonly IRepository<Photos.Photo, Guid> _photoRepository;
        private readonly IMemoryCache _memoryCache;

        public CachePerformanceTests()
        {
            _albumAppService = GetRequiredService<Albums.IAlbumAppService>();
            _photoAppService = GetRequiredService<Photos.IPhotoAppService>();
            _albumRepository = GetRequiredService<IRepository<Albums.Album, Guid>>();
            _photoRepository = GetRequiredService<IRepository<Photos.Photo, Guid>>();
            _memoryCache = GetRequiredService<IMemoryCache>();
        }

        [Fact]
        public async Task Album_GetListAsync_Should_Be_Faster_With_Cache()
        {
            // Arrange
            await CreateLargeDatasetAsync();
            var input = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 20,
                SkipCount = 0,
                Sorting = "Name"
            };

            // Clear any existing cache
            _memoryCache.Remove(GetCacheKeyForAlbumListAsync(input));

            // Act & Measure - First call (database hit)
            var stopwatch = Stopwatch.StartNew();
            var result1 = await _albumAppService.GetListAsync(input);
            stopwatch.Stop();
            var firstCallTime = stopwatch.ElapsedMilliseconds;

            // Act & Measure - Second call (cache hit)
            stopwatch.Restart();
            var result2 = await _albumAppService.GetListAsync(input);
            stopwatch.Stop();
            var secondCallTime = stopwatch.ElapsedMilliseconds;

            // Assert
            result1.ShouldNotBeNull();
            result2.ShouldNotBeNull();
            result1.TotalCount.ShouldBe(result2.TotalCount);
            
            // Cache hit should be significantly faster
            secondCallTime.ShouldBeLessThan(firstCallTime);
            
            // Cache hit should be under 10ms for reasonable performance
            secondCallTime.ShouldBeLessThan(10);
            
            // Log performance for visibility
            System.Diagnostics.Debug.WriteLine($"Album Cache - First call (DB): {firstCallTime}ms");
            System.Diagnostics.Debug.WriteLine($"Album Cache - Second call (Cache): {secondCallTime}ms");
            System.Diagnostics.Debug.WriteLine($"Album Cache - Performance improvement: {(double)firstCallTime / secondCallTime:F2}x faster");
        }

        [Fact]
        public async Task Photo_GetListAsync_Should_Be_Faster_With_Cache()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            await CreateLargePhotoDatasetAsync(album.Id);
            var input = new Photos.GetPhotosInput
            {
                AlbumId = album.Id,
                MaxResultCount = 20,
                SkipCount = 0,
                Sorting = "FileName"
            };

            // Clear any existing cache
            _memoryCache.Remove(GetCacheKeyForPhotoListAsync(input));

            // Act & Measure - First call (database hit)
            var stopwatch = Stopwatch.StartNew();
            var result1 = await _photoAppService.GetListAsync(input);
            stopwatch.Stop();
            var firstCallTime = stopwatch.ElapsedMilliseconds;

            // Act & Measure - Second call (cache hit)
            stopwatch.Restart();
            var result2 = await _photoAppService.GetListAsync(input);
            stopwatch.Stop();
            var secondCallTime = stopwatch.ElapsedMilliseconds;

            // Assert
            result1.ShouldNotBeNull();
            result2.ShouldNotBeNull();
            result1.TotalCount.ShouldBe(result2.TotalCount);
            
            // Cache hit should be significantly faster
            secondCallTime.ShouldBeLessThan(firstCallTime);
            
            // Cache hit should be under 10ms for reasonable performance
            secondCallTime.ShouldBeLessThan(10);
            
            // Log performance for visibility
            System.Diagnostics.Debug.WriteLine($"Photo Cache - First call (DB): {firstCallTime}ms");
            System.Diagnostics.Debug.WriteLine($"Photo Cache - Second call (Cache): {secondCallTime}ms");
            System.Diagnostics.Debug.WriteLine($"Photo Cache - Performance improvement: {(double)firstCallTime / secondCallTime:F2}x faster");
        }

        [Fact]
        public async Task Multiple_Concurrent_Cache_Hits_Should_Be_Fast()
        {
            // Arrange
            await CreateLargeDatasetAsync();
            var input = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 20,
                SkipCount = 0,
                Sorting = "Name"
            };

            // Prime the cache
            await _albumAppService.GetListAsync(input);

            // Act & Measure - Multiple concurrent cache hits
            var tasks = new List<Task<PagedResultDto<Albums.AlbumDto>>>();
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(_albumAppService.GetListAsync(input));
            }
            
            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();
            var totalTime = stopwatch.ElapsedMilliseconds;

            // Assert
            results.Length.ShouldBe(10);
            results.All(r => r != null).ShouldBeTrue();
            results.All(r => r.TotalCount == results[0].TotalCount).ShouldBeTrue();
            
            // All 10 concurrent cache hits should complete in under 50ms total
            totalTime.ShouldBeLessThan(50);
            
            System.Diagnostics.Debug.WriteLine($"10 concurrent cache hits completed in: {totalTime}ms");
            System.Diagnostics.Debug.WriteLine($"Average time per cache hit: {(double)totalTime / 10:F2}ms");
        }

        [Fact]
        public async Task Cache_Performance_Should_Scale_With_Different_Parameters()
        {
            // Arrange
            await CreateLargeDatasetAsync();
            var measurements = new List<(string scenario, long cacheTime)>();

            var scenarios = new[]
            {
                ("Small page size", new PagedAndSortedResultRequestDto { MaxResultCount = 5, SkipCount = 0, Sorting = "Name" }),
                ("Medium page size", new PagedAndSortedResultRequestDto { MaxResultCount = 20, SkipCount = 0, Sorting = "Name" }),
                ("Large page size", new PagedAndSortedResultRequestDto { MaxResultCount = 50, SkipCount = 0, Sorting = "Name" }),
                ("Different sorting", new PagedAndSortedResultRequestDto { MaxResultCount = 20, SkipCount = 0, Sorting = "DisplayOrder" }),
                ("With pagination", new PagedAndSortedResultRequestDto { MaxResultCount = 20, SkipCount = 20, Sorting = "Name" })
            };

            foreach (var (scenario, input) in scenarios)
            {
                // Prime cache
                await _albumAppService.GetListAsync(input);
                
                // Measure cache hit
                var stopwatch = Stopwatch.StartNew();
                await _albumAppService.GetListAsync(input);
                stopwatch.Stop();
                
                measurements.Add((scenario, stopwatch.ElapsedMilliseconds));
            }

            // Assert - All cache hits should be fast regardless of parameters
            foreach (var (scenario, cacheTime) in measurements)
            {
                cacheTime.ShouldBeLessThan(10, $"Cache hit for {scenario} took {cacheTime}ms");
                System.Diagnostics.Debug.WriteLine($"{scenario}: {cacheTime}ms");
            }
        }

        [Fact]
        public async Task Photo_Cache_Performance_With_Complex_Filters()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            await CreateLargePhotoDatasetWithVariedDataAsync(album.Id);

            var filterScenarios = new[]
            {
                ("No filters", new Photos.GetPhotosInput { MaxResultCount = 20, SkipCount = 0, Sorting = "FileName" }),
                ("Album filter", new Photos.GetPhotosInput { AlbumId = album.Id, MaxResultCount = 20, SkipCount = 0, Sorting = "FileName" }),
                ("Location filter", new Photos.GetPhotosInput { Location = "Paris", MaxResultCount = 20, SkipCount = 0, Sorting = "FileName" }),
                ("Date range filter", new Photos.GetPhotosInput { 
                    DateTakenFrom = DateTime.Today.AddDays(-30), 
                    DateTakenTo = DateTime.Today, 
                    MaxResultCount = 20, 
                    SkipCount = 0, 
                    Sorting = "DateTaken" 
                }),
                ("Search term filter", new Photos.GetPhotosInput { SearchTerm = "test", MaxResultCount = 20, SkipCount = 0, Sorting = "FileName" })
            };

            var measurements = new List<(string scenario, long firstCall, long cacheHit)>();

            foreach (var (scenario, input) in filterScenarios)
            {
                // Clear cache
                _memoryCache.Remove(GetCacheKeyForPhotoListAsync(input));
                
                // Measure first call (DB hit)
                var stopwatch = Stopwatch.StartNew();
                await _photoAppService.GetListAsync(input);
                stopwatch.Stop();
                var firstCall = stopwatch.ElapsedMilliseconds;
                
                // Measure cache hit
                stopwatch.Restart();
                await _photoAppService.GetListAsync(input);
                stopwatch.Stop();
                var cacheHit = stopwatch.ElapsedMilliseconds;
                
                measurements.Add((scenario, firstCall, cacheHit));
            }

            // Assert and log
            foreach (var (scenario, firstCall, cacheHit) in measurements)
            {
                cacheHit.ShouldBeLessThan(firstCall, $"Cache hit should be faster than DB hit for {scenario}");
                cacheHit.ShouldBeLessThan(10, $"Cache hit for {scenario} should be under 10ms");
                
                System.Diagnostics.Debug.WriteLine($"{scenario}: DB={firstCall}ms, Cache={cacheHit}ms, Improvement={firstCall / Math.Max(cacheHit, 1)}x");
            }
        }

        [Fact]
        public async Task Cache_Should_Handle_High_Load_Efficiently()
        {
            // Arrange
            await CreateLargeDatasetAsync();
            var input = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 20,
                SkipCount = 0,
                Sorting = "Name"
            };

            // Prime the cache
            await _albumAppService.GetListAsync(input);

            // Act - Simulate high load with many concurrent requests
            var tasks = new List<Task<PagedResultDto<Albums.AlbumDto>>>();
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(_albumAppService.GetListAsync(input));
            }
            
            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();
            var totalTime = stopwatch.ElapsedMilliseconds;

            // Assert
            results.Length.ShouldBe(100);
            results.All(r => r != null).ShouldBeTrue();
            results.All(r => r.TotalCount == results[0].TotalCount).ShouldBeTrue();
            
            // 100 concurrent cache hits should complete reasonably fast
            totalTime.ShouldBeLessThan(500); // 500ms for 100 requests = ~5ms average
            
            System.Diagnostics.Debug.WriteLine($"100 concurrent cache hits completed in: {totalTime}ms");
            System.Diagnostics.Debug.WriteLine($"Average time per request: {(double)totalTime / 100:F2}ms");
            System.Diagnostics.Debug.WriteLine($"Requests per second: {100.0 / totalTime * 1000:F0}");
        }

        #region Helper Methods

        private async Task CreateLargeDatasetAsync()
        {
            // Create 50 albums to simulate a larger dataset
            var albums = new List<Albums.Album>();
            for (int i = 1; i <= 50; i++)
            {
                var album = new Albums.Album(
                    Guid.NewGuid(),
                    $"Performance Test Album {i:D2}",
                    $"Topic {i % 5 + 1}", // 5 different topics
                    $"Description for album {i}",
                    i
                );
                albums.Add(album);
            }
            
            foreach (var album in albums)
            {
                await _albumRepository.InsertAsync(album, autoSave: false);
            }
        }

        private async Task<Albums.Album> CreateTestAlbumAsync()
        {
            var album = new Albums.Album(
                Guid.NewGuid(),
                "Performance Test Album",
                "Performance",
                "Album for performance testing",
                1
            );
            return await _albumRepository.InsertAsync(album, autoSave: true);
        }

        private async Task CreateLargePhotoDatasetAsync(Guid albumId)
        {
            // Create 100 photos to simulate a larger dataset
            for (int i = 1; i <= 100; i++)
            {
                var photo = new Photos.Photo(
                    Guid.NewGuid(),
                    albumId,
                    $"performance_test_{i:D3}.jpg",
                    $"/uploads/photos/performance_test_{i:D3}.jpg",
                    1024 * i,
                    i,
                    "Test Location",
                    $"/uploads/thumbnails/performance_test_{i:D3}_thumb.jpg",
                    800,
                    600,
                    DateTime.Today.AddDays(-i)
                );
                await _photoRepository.InsertAsync(photo, autoSave: true);
            }
        }

        private async Task CreateLargePhotoDatasetWithVariedDataAsync(Guid albumId)
        {
            var locations = new[] { "Paris", "London", "Tokyo", "New York", "Sydney" };
            var searchTerms = new[] { "test", "performance", "benchmark", "cache", "speed" };
            
            // Create 200 photos with varied data for filtering tests
            for (int i = 1; i <= 200; i++)
            {
                var photo = new Photos.Photo(
                    Guid.NewGuid(),
                    albumId,
                    $"varied_test_{i:D3}.jpg",
                    $"/uploads/photos/varied_test_{i:D3}.jpg",
                    1024 * i,
                    i,
                    locations[i % locations.Length],
                    $"/uploads/thumbnails/varied_test_{i:D3}_thumb.jpg",
                    800,
                    600,
                    DateTime.Today.AddDays(-i % 100) // Spread across 100 days
                );
                await _photoRepository.InsertAsync(photo, autoSave: true);
            }
        }

        private string GetCacheKeyForAlbumListAsync(PagedAndSortedResultRequestDto input)
        {
            var cacheKeyService = GetRequiredService<Services.ICacheKeyService>();
            return cacheKeyService.GenerateListCacheKey("AlbumAppService", "GetListAsync", input);
        }

        private string GetCacheKeyForPhotoListAsync(Photos.GetPhotosInput input)
        {
            var cacheKeyService = GetRequiredService<Services.ICacheKeyService>();
            return cacheKeyService.GenerateListCacheKey("PhotoAppService", "GetListAsync", input);
        }

        #endregion
    }
}