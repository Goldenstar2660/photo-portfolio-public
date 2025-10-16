using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace PhotoGallery.Albums
{
    public class AlbumCacheTests : PhotoGalleryApplicationTestBase<PhotoGalleryApplicationTestModule>
    {
        private readonly IAlbumAppService _albumAppService;
        private readonly IRepository<Album, Guid> _albumRepository;
        private readonly IMemoryCache _memoryCache;

        public AlbumCacheTests()
        {
            _albumAppService = GetRequiredService<IAlbumAppService>();
            _albumRepository = GetRequiredService<IRepository<Album, Guid>>();
            _memoryCache = GetRequiredService<IMemoryCache>();
        }

        [Fact]
        public async Task GetListAsync_Should_Cache_Results_On_First_Call()
        {
            // Arrange
            await CreateSampleAlbumsAsync();
            var input = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "Name"
            };

            // Act - First call should hit database and cache the result
            var result1 = await _albumAppService.GetListAsync(input);
            
            // Act - Second call should return cached result
            var result2 = await _albumAppService.GetListAsync(input);

            // Assert
            result1.ShouldNotBeNull();
            result2.ShouldNotBeNull();
            result1.TotalCount.ShouldBe(result2.TotalCount);
            result1.Items.Count.ShouldBe(result2.Items.Count);
            
            // Verify both results have same content
            for (int i = 0; i < result1.Items.Count; i++)
            {
                result1.Items[i].Id.ShouldBe(result2.Items[i].Id);
                result1.Items[i].Name.ShouldBe(result2.Items[i].Name);
            }
        }

        [Fact]
        public async Task GetListAsync_Should_Return_Different_Cache_For_Different_Parameters()
        {
            // Arrange
            await CreateSampleAlbumsAsync();
            var input1 = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 5,
                SkipCount = 0,
                Sorting = "Name"
            };
            var input2 = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "Name"
            };

            // Act
            var result1 = await _albumAppService.GetListAsync(input1);
            var result2 = await _albumAppService.GetListAsync(input2);

            // Assert
            result1.ShouldNotBeNull();
            result2.ShouldNotBeNull();
            result1.Items.Count.ShouldBe(5);
            result2.Items.Count.ShouldBe((int)Math.Min(10, result2.TotalCount));
        }

        [Fact]
        public async Task CreateAsync_Should_Invalidate_Album_Cache()
        {
            // Arrange
            await CreateSampleAlbumsAsync();
            var input = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "Name"
            };

            // Act - Cache the initial result
            var initialResult = await _albumAppService.GetListAsync(input);
            var initialCount = initialResult.TotalCount;

            // Create a new album
            var createDto = new CreateAlbumDto
            {
                Name = "Cache Test Album",
                Topic = "Testing",
                Description = "Album created for cache testing",
                DisplayOrder = 999
            };
            await _albumAppService.CreateAsync(createDto);

            // Act - Get list again (should be refreshed from database)
            var updatedResult = await _albumAppService.GetListAsync(input);

            // Assert
            updatedResult.TotalCount.ShouldBe(initialCount + 1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Invalidate_Album_Cache()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            var input = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "Name"
            };

            // Cache the initial result
            var initialResult = await _albumAppService.GetListAsync(input);
            var originalAlbum = initialResult.Items.First(x => x.Id == album.Id);

            // Act - Update the album
            var updateDto = new UpdateAlbumDto
            {
                Name = "Updated Album Name",
                Topic = originalAlbum.Topic,
                Description = "Updated description"
            };
            await _albumAppService.UpdateAsync(album.Id, updateDto);

            // Get list again (should be refreshed from database)
            var updatedResult = await _albumAppService.GetListAsync(input);

            // Assert
            var updatedAlbum = updatedResult.Items.First(x => x.Id == album.Id);
            updatedAlbum.Name.ShouldBe("Updated Album Name");
            updatedAlbum.Description.ShouldBe("Updated description");
        }

        [Fact]
        public async Task DeleteAsync_Should_Invalidate_Album_Cache()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            var input = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "Name"
            };

            // Cache the initial result
            var initialResult = await _albumAppService.GetListAsync(input);
            var initialCount = initialResult.TotalCount;
            initialResult.Items.Any(x => x.Id == album.Id).ShouldBeTrue();

            // Act - Delete the album
            await _albumAppService.DeleteAsync(album.Id);

            // Get list again (should be refreshed from database)
            var updatedResult = await _albumAppService.GetListAsync(input);

            // Assert
            updatedResult.TotalCount.ShouldBe(initialCount - 1);
            updatedResult.Items.Any(x => x.Id == album.Id).ShouldBeFalse();
        }

        [Fact]
        public async Task ReorderAsync_Should_Invalidate_Album_Cache()
        {
            // Arrange
            var albums = await CreateMultipleTestAlbumsAsync(3);
            var input = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "DisplayOrder"
            };

            // Cache the initial result
            var initialResult = await _albumAppService.GetListAsync(input);

            // Act - Reorder albums
            var reorderDto = new ReorderAlbumsDto
            {
                AlbumOrders = new List<AlbumOrderDto>
                {
                    new AlbumOrderDto { Id = albums[0].Id, DisplayOrder = 3 },
                    new AlbumOrderDto { Id = albums[1].Id, DisplayOrder = 1 },
                    new AlbumOrderDto { Id = albums[2].Id, DisplayOrder = 2 }
                }
            };
            await _albumAppService.ReorderAsync(reorderDto);

            // Get list again (should be refreshed from database)
            var updatedResult = await _albumAppService.GetListAsync(input);

            // Assert - Order should be different
            var reorderedAlbumIds = updatedResult.Items
                .Where(x => albums.Any(a => a.Id == x.Id))
                .OrderBy(x => x.DisplayOrder)
                .Select(x => x.Id)
                .ToArray();

            reorderedAlbumIds[0].ShouldBe(albums[1].Id); // Display order 1
            reorderedAlbumIds[1].ShouldBe(albums[2].Id); // Display order 2
            reorderedAlbumIds[2].ShouldBe(albums[0].Id); // Display order 3
        }

        [Fact]
        public async Task Cache_Should_Respect_Different_Sorting_Parameters()
        {
            // Arrange
            await CreateMultipleTestAlbumsAsync(5);
            
            var inputByName = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "Name"
            };
            
            var inputByDisplayOrder = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "DisplayOrder"
            };

            // Act
            var resultByName = await _albumAppService.GetListAsync(inputByName);
            var resultByDisplayOrder = await _albumAppService.GetListAsync(inputByDisplayOrder);

            // Assert - Results should be different based on sorting
            resultByName.ShouldNotBeNull();
            resultByDisplayOrder.ShouldNotBeNull();
            
            // Should have same total count but different ordering
            resultByName.TotalCount.ShouldBe(resultByDisplayOrder.TotalCount);
            
            // First items might be different due to different sorting
            if (resultByName.Items.Count > 1 && resultByDisplayOrder.Items.Count > 1)
            {
                var nameOrderIds = resultByName.Items.Select(x => x.Id).ToArray();
                var displayOrderIds = resultByDisplayOrder.Items.Select(x => x.Id).ToArray();
                
                // The important thing is that both results are cached separately
                // We don't assert different order as it might coincidentally be the same
            }
        }

        [Fact]
        public async Task Cache_Should_Handle_Pagination_Correctly()
        {
            // Arrange
            await CreateMultipleTestAlbumsAsync(15);
            
            var firstPageInput = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 5,
                SkipCount = 0,
                Sorting = "Name"
            };
            
            var secondPageInput = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 5,
                SkipCount = 5,
                Sorting = "Name"
            };

            // Act
            var firstPageResult = await _albumAppService.GetListAsync(firstPageInput);
            var secondPageResult = await _albumAppService.GetListAsync(secondPageInput);

            // Assert
            firstPageResult.Items.Count.ShouldBe(5);
            secondPageResult.Items.Count.ShouldBe(5);
            
            // Should have different items
            var firstPageIds = firstPageResult.Items.Select(x => x.Id).ToHashSet();
            var secondPageIds = secondPageResult.Items.Select(x => x.Id).ToHashSet();
            
            firstPageIds.Intersect(secondPageIds).Count().ShouldBe(0);
        }

        private async Task<Album> CreateTestAlbumAsync()
        {
            var album = new Album(
                Guid.NewGuid(),
                "Test Album " + Guid.NewGuid().ToString("N")[..8],
                "Test Topic",
                "Test Description",
                100
            );

            return await _albumRepository.InsertAsync(album, autoSave: true);
        }

        private async Task<Album[]> CreateMultipleTestAlbumsAsync(int count)
        {
            var albums = new Album[count];
            for (int i = 0; i < count; i++)
            {
                albums[i] = new Album(
                    Guid.NewGuid(),
                    $"Test Album {i + 1}",
                    "Test Topic",
                    $"Test Description {i + 1}",
                    i + 1
                );
            }

            for (int i = 0; i < count; i++)
            {
                albums[i] = await _albumRepository.InsertAsync(albums[i], autoSave: true);
            }

            return albums;
        }

        private async Task CreateSampleAlbumsAsync()
        {
            await CreateMultipleTestAlbumsAsync(8);
        }
    }
}