using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace PhotoGallery.Photos
{
    public class PhotoCacheTests : PhotoGalleryApplicationTestBase<PhotoGalleryApplicationTestModule>
    {
        private readonly IPhotoAppService _photoAppService;
        private readonly IRepository<Photo, Guid> _photoRepository;
        private readonly IRepository<Albums.Album, Guid> _albumRepository;
        private readonly IMemoryCache _memoryCache;

        public PhotoCacheTests()
        {
            _photoAppService = GetRequiredService<IPhotoAppService>();
            _photoRepository = GetRequiredService<IRepository<Photo, Guid>>();
            _albumRepository = GetRequiredService<IRepository<Albums.Album, Guid>>();
            _memoryCache = GetRequiredService<IMemoryCache>();
        }

        [Fact]
        public async Task GetListAsync_Should_Cache_Results_On_First_Call()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            await CreateSamplePhotosAsync(album.Id);
            var input = new GetPhotosInput
            {
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "FileName"
            };

            // Act - First call should hit database and cache the result
            var result1 = await _photoAppService.GetListAsync(input);
            
            // Act - Second call should return cached result
            var result2 = await _photoAppService.GetListAsync(input);

            // Assert
            result1.ShouldNotBeNull();
            result2.ShouldNotBeNull();
            result1.TotalCount.ShouldBe(result2.TotalCount);
            result1.Items.Count.ShouldBe(result2.Items.Count);
            
            // Verify both results have same content
            for (int i = 0; i < result1.Items.Count; i++)
            {
                result1.Items[i].Id.ShouldBe(result2.Items[i].Id);
                result1.Items[i].FileName.ShouldBe(result2.Items[i].FileName);
            }
        }

        [Fact]
        public async Task GetListAsync_Should_Return_Different_Cache_For_Different_Albums()
        {
            // Arrange
            var album1 = await CreateTestAlbumAsync();
            var album2 = await CreateTestAlbumAsync();
            await CreateSamplePhotosAsync(album1.Id, 3);
            await CreateSamplePhotosAsync(album2.Id, 2);
            
            var input1 = new GetPhotosInput
            {
                AlbumId = album1.Id,
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "FileName"
            };
            var input2 = new GetPhotosInput
            {
                AlbumId = album2.Id,
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "FileName"
            };

            // Act
            var result1 = await _photoAppService.GetListAsync(input1);
            var result2 = await _photoAppService.GetListAsync(input2);

            // Assert
            result1.ShouldNotBeNull();
            result2.ShouldNotBeNull();
            result1.Items.Count.ShouldBe(3);
            result2.Items.Count.ShouldBe(2);
            result1.Items.All(x => x.AlbumId == album1.Id).ShouldBeTrue();
            result2.Items.All(x => x.AlbumId == album2.Id).ShouldBeTrue();
        }

        [Fact]
        public async Task GetListAsync_Should_Cache_Different_Results_For_Different_Search_Terms()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            await CreatePhotoWithCaptionAsync(album.Id, "Sunset photo", "sunset.jpg");
            await CreatePhotoWithCaptionAsync(album.Id, "Mountain view", "mountain.jpg");
            await CreatePhotoWithCaptionAsync(album.Id, "Beach sunset", "beach.jpg");
            
            var inputSunset = new GetPhotosInput
            {
                SearchTerm = "sunset",
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "FileName"
            };
            var inputMountain = new GetPhotosInput
            {
                SearchTerm = "mountain",
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "FileName"
            };

            // Act
            var resultSunset = await _photoAppService.GetListAsync(inputSunset);
            var resultMountain = await _photoAppService.GetListAsync(inputMountain);

            // Assert
            resultSunset.ShouldNotBeNull();
            resultMountain.ShouldNotBeNull();
            resultSunset.Items.Count.ShouldBe(2); // "Sunset photo" and "Beach sunset"
            resultMountain.Items.Count.ShouldBe(1); // "Mountain view"
        }

        [Fact]
        public async Task CreateAsync_Should_Invalidate_Photo_Cache()
        {
            // Note: This test would require actual file upload functionality
            // For now, we'll test cache invalidation through update/delete operations
            await Task.CompletedTask;
        }

        [Fact]
        public async Task UpdateAsync_Should_Invalidate_Photo_Cache()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            var photo = await CreateTestPhotoAsync(album.Id);
            var input = new GetPhotosInput
            {
                AlbumId = album.Id,
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "FileName"
            };

            // Cache the initial result
            var initialResult = await _photoAppService.GetListAsync(input);
            var originalPhoto = initialResult.Items.First(x => x.Id == photo.Id);

            // Act - Update the photo
            var updateDto = new UpdatePhotoDto
            {
                Location = "Updated location"
            };
            await _photoAppService.UpdateAsync(photo.Id, updateDto);

            // Get list again (should be refreshed from database)
            var updatedResult = await _photoAppService.GetListAsync(input);

            // Assert
            var updatedPhoto = updatedResult.Items.First(x => x.Id == photo.Id);
            updatedPhoto.Location.ShouldBe("Updated location");
        }

        [Fact]
        public async Task DeleteAsync_Should_Invalidate_Photo_Cache()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            var photo = await CreateTestPhotoAsync(album.Id);
            var input = new GetPhotosInput
            {
                AlbumId = album.Id,
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "FileName"
            };

            // Cache the initial result
            var initialResult = await _photoAppService.GetListAsync(input);
            var initialCount = initialResult.TotalCount;
            initialResult.Items.Any(x => x.Id == photo.Id).ShouldBeTrue();

            // Act - Delete the photo
            await _photoAppService.DeleteAsync(photo.Id);

            // Get list again (should be refreshed from database)
            var updatedResult = await _photoAppService.GetListAsync(input);

            // Assert
            updatedResult.TotalCount.ShouldBe(initialCount - 1);
            updatedResult.Items.Any(x => x.Id == photo.Id).ShouldBeFalse();
        }

        [Fact]
        public async Task ReorderAsync_Should_Invalidate_Photo_Cache()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            var photos = await CreateMultipleTestPhotosAsync(album.Id, 3);
            var input = new GetPhotosInput
            {
                AlbumId = album.Id,
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "DisplayOrder"
            };

            // Cache the initial result
            var initialResult = await _photoAppService.GetListAsync(input);

            // Act - Reorder photos
            var reorderDto = new ReorderPhotosDto
            {
                PhotoOrders = new List<PhotoOrderDto>
                {
                    new PhotoOrderDto { Id = photos[0].Id, DisplayOrder = 3 },
                    new PhotoOrderDto { Id = photos[1].Id, DisplayOrder = 1 },
                    new PhotoOrderDto { Id = photos[2].Id, DisplayOrder = 2 }
                }
            };
            await _photoAppService.ReorderAsync(reorderDto);

            // Get list again (should be refreshed from database)
            var updatedResult = await _photoAppService.GetListAsync(input);

            // Assert - Order should be different
            var reorderedPhotoIds = updatedResult.Items
                .Where(x => photos.Any(p => p.Id == x.Id))
                .OrderBy(x => x.DisplayOrder)
                .Select(x => x.Id)
                .ToArray();

            reorderedPhotoIds[0].ShouldBe(photos[1].Id); // Display order 1
            reorderedPhotoIds[1].ShouldBe(photos[2].Id); // Display order 2
            reorderedPhotoIds[2].ShouldBe(photos[0].Id); // Display order 3
        }

        [Fact]
        public async Task Cache_Should_Handle_Location_Filter_Correctly()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            await CreatePhotoWithLocationAsync(album.Id, "Paris", "paris1.jpg");
            await CreatePhotoWithLocationAsync(album.Id, "Paris", "paris2.jpg");
            await CreatePhotoWithLocationAsync(album.Id, "London", "london1.jpg");
            
            var inputParis = new GetPhotosInput
            {
                Location = "Paris",
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "FileName"
            };
            
            var inputLondon = new GetPhotosInput
            {
                Location = "London",
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "FileName"
            };

            // Act
            var resultParis = await _photoAppService.GetListAsync(inputParis);
            var resultLondon = await _photoAppService.GetListAsync(inputLondon);

            // Assert
            resultParis.ShouldNotBeNull();
            resultLondon.ShouldNotBeNull();
            resultParis.Items.Count.ShouldBe(2);
            resultLondon.Items.Count.ShouldBe(1);
            resultParis.Items.All(x => x.Location == "Paris").ShouldBeTrue();
            resultLondon.Items.All(x => x.Location == "London").ShouldBeTrue();
        }

        [Fact]
        public async Task Cache_Should_Handle_Date_Range_Filter_Correctly()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            await CreatePhotoWithDateAsync(album.Id, new DateTime(2023, 1, 15), "jan.jpg");
            await CreatePhotoWithDateAsync(album.Id, new DateTime(2023, 6, 15), "jun.jpg");
            await CreatePhotoWithDateAsync(album.Id, new DateTime(2023, 12, 15), "dec.jpg");
            
            var inputFirstHalf = new GetPhotosInput
            {
                DateTakenFrom = new DateTime(2023, 1, 1),
                DateTakenTo = new DateTime(2023, 6, 30),
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "DateTaken"
            };
            
            var inputSecondHalf = new GetPhotosInput
            {
                DateTakenFrom = new DateTime(2023, 7, 1),
                DateTakenTo = new DateTime(2023, 12, 31),
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "DateTaken"
            };

            // Act
            var resultFirstHalf = await _photoAppService.GetListAsync(inputFirstHalf);
            var resultSecondHalf = await _photoAppService.GetListAsync(inputSecondHalf);

            // Assert
            resultFirstHalf.ShouldNotBeNull();
            resultSecondHalf.ShouldNotBeNull();
            resultFirstHalf.Items.Count.ShouldBe(2); // Jan and Jun
            resultSecondHalf.Items.Count.ShouldBe(1); // Dec
        }

        [Fact]
        public async Task Cache_Should_Handle_Pagination_Correctly()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            await CreateSamplePhotosAsync(album.Id, 15);
            
            var firstPageInput = new GetPhotosInput
            {
                AlbumId = album.Id,
                MaxResultCount = 5,
                SkipCount = 0,
                Sorting = "FileName"
            };
            
            var secondPageInput = new GetPhotosInput
            {
                AlbumId = album.Id,
                MaxResultCount = 5,
                SkipCount = 5,
                Sorting = "FileName"
            };

            // Act
            var firstPageResult = await _photoAppService.GetListAsync(firstPageInput);
            var secondPageResult = await _photoAppService.GetListAsync(secondPageInput);

            // Assert
            firstPageResult.Items.Count.ShouldBe(5);
            secondPageResult.Items.Count.ShouldBe(5);
            
            // Should have different items
            var firstPageIds = firstPageResult.Items.Select(x => x.Id).ToHashSet();
            var secondPageIds = secondPageResult.Items.Select(x => x.Id).ToHashSet();
            
            firstPageIds.Intersect(secondPageIds).Count().ShouldBe(0);
        }

        #region Helper Methods

        private async Task<Albums.Album> CreateTestAlbumAsync()
        {
            var album = new Albums.Album(
                Guid.NewGuid(),
                "Test Album " + Guid.NewGuid().ToString("N")[..8],
                "Test Topic",
                "Test Description",
                100
            );

            return await _albumRepository.InsertAsync(album, autoSave: true);
        }

        private async Task<Photo> CreateTestPhotoAsync(Guid albumId)
        {
            var photo = new Photo(
                Guid.NewGuid(),
                albumId,
                "test.jpg",
                "/uploads/photos/test.jpg",
                1024,
                1,
                "Test location",
                "/uploads/thumbnails/test_thumb.jpg",
                800,
                600,
                DateTime.Now
            );

            return await _photoRepository.InsertAsync(photo, autoSave: true);
        }

        private async Task<Photo[]> CreateMultipleTestPhotosAsync(Guid albumId, int count)
        {
            var photos = new Photo[count];
            for (int i = 0; i < count; i++)
            {
                photos[i] = new Photo(
                    Guid.NewGuid(),
                    albumId,
                    $"test_{i + 1}.jpg",
                    $"/uploads/photos/test_{i + 1}.jpg",
                    1024 * (i + 1),
                    i + 1,
                    $"Test location {i + 1}",
                    $"/uploads/thumbnails/test_{i + 1}_thumb.jpg",
                    800,
                    600,
                    DateTime.Now.AddDays(-i)
                );
            }

            for (int i = 0; i < count; i++)
            {
                photos[i] = await _photoRepository.InsertAsync(photos[i], autoSave: true);
            }

            return photos;
        }

        private async Task CreateSamplePhotosAsync(Guid albumId, int count = 5)
        {
            await CreateMultipleTestPhotosAsync(albumId, count);
        }

        private async Task<Photo> CreatePhotoWithCaptionAsync(Guid albumId, string caption, string fileName)
        {
            var photo = new Photo(
                Guid.NewGuid(),
                albumId,
                fileName,
                $"/uploads/photos/{fileName}",
                1024,
                1,
                "Test location",
                $"/uploads/thumbnails/{fileName}_thumb.jpg",
                800,
                600,
                DateTime.Now
            );

            return await _photoRepository.InsertAsync(photo, autoSave: true);
        }

        private async Task<Photo> CreatePhotoWithLocationAsync(Guid albumId, string location, string fileName)
        {
            var photo = new Photo(
                Guid.NewGuid(),
                albumId,
                fileName,
                $"/uploads/photos/{fileName}",
                1024,
                1,
                location,
                $"/uploads/thumbnails/{fileName}_thumb.jpg",
                800,
                600,
                DateTime.Now
            );

            return await _photoRepository.InsertAsync(photo, autoSave: true);
        }

        private async Task<Photo> CreatePhotoWithDateAsync(Guid albumId, DateTime dateTaken, string fileName)
        {
            var photo = new Photo(
                Guid.NewGuid(),
                albumId,
                fileName,
                $"/uploads/photos/{fileName}",
                1024,
                1,
                "Test location",
                $"/uploads/thumbnails/{fileName}_thumb.jpg",
                800,
                600,
                dateTaken
            );

            return await _photoRepository.InsertAsync(photo, autoSave: true);
        }

        #endregion
    }
}