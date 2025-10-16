using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;
using Xunit;
using NSubstitute;
using PhotoGallery.Services;
using PhotoGallery.Albums;

namespace PhotoGallery.Photos
{
    public class PhotoAppServiceTests : PhotoGalleryApplicationTestBase<PhotoGalleryApplicationTestModule>
    {
        private readonly IPhotoAppService _photoAppService;
        private readonly IRepository<Photo, Guid> _photoRepository;
        private readonly IRepository<Album, Guid> _albumRepository;
        private readonly IFileUploadService _fileUploadService;

        public PhotoAppServiceTests()
        {
            _photoAppService = GetRequiredService<IPhotoAppService>();
            _photoRepository = GetRequiredService<IRepository<Photo, Guid>>();
            _albumRepository = GetRequiredService<IRepository<Album, Guid>>();
            _fileUploadService = Substitute.For<IFileUploadService>();
        }

        [Fact]
        public async Task GetListAsync_Should_Return_Paginated_Photos()
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

            // Act
            var result = await _photoAppService.GetListAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.TotalCount.ShouldBeGreaterThan(0);
            result.Items.ShouldNotBeEmpty();
            result.Items.Count.ShouldBeLessThanOrEqualTo(10);
        }

        [Fact]
        public async Task GetListAsync_Should_Filter_By_Album()
        {
            // Arrange
            var album1 = await CreateTestAlbumAsync("Album 1");
            var album2 = await CreateTestAlbumAsync("Album 2");
            await CreateTestPhotoAsync(album1.Id, "Photo1.jpg");
            await CreateTestPhotoAsync(album1.Id, "Photo2.jpg");
            await CreateTestPhotoAsync(album2.Id, "Photo3.jpg");

            var input = new GetPhotosInput
            {
                AlbumId = album1.Id,
                MaxResultCount = 10
            };

            // Act
            var result = await _photoAppService.GetListAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.TotalCount.ShouldBe(2);
            result.Items.ShouldAllBe(p => p.AlbumId == album1.Id);
        }

        [Fact]
        public async Task GetListAsync_Should_Filter_By_Location()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            await CreateTestPhotoAsync(album.Id, "Photo1.jpg", location: "Paris");
            await CreateTestPhotoAsync(album.Id, "Photo2.jpg", location: "London");
            await CreateTestPhotoAsync(album.Id, "Photo3.jpg", location: "Paris");

            var input = new GetPhotosInput
            {
                Location = "Paris",
                MaxResultCount = 10
            };

            // Act
            var result = await _photoAppService.GetListAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.TotalCount.ShouldBe(2);
            result.Items.ShouldAllBe(p => p.Location != null && p.Location.Contains("Paris"));
        }

        [Fact]
        public async Task GetListAsync_Should_Filter_By_Search_Term()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            await CreateTestPhotoAsync(album.Id, "Sunset.jpg");
            await CreateTestPhotoAsync(album.Id, "Mountain.jpg");
            await CreateTestPhotoAsync(album.Id, "Beach.jpg");

            var input = new GetPhotosInput
            {
                SearchTerm = "sunset",
                MaxResultCount = 10
            };

            // Act
            var result = await _photoAppService.GetListAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.TotalCount.ShouldBe(2);
            result.Items.ShouldAllBe(p => 
                p.FileName.Contains("sunset", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetAsync_Should_Return_Photo_By_Id()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            var photo = await CreateTestPhotoAsync(album.Id);

            // Act
            var result = await _photoAppService.GetAsync(photo.Id);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(photo.Id);
            result.FileName.ShouldBe(photo.FileName);
            result.AlbumId.ShouldBe(photo.AlbumId);
            result.AlbumName.ShouldBe(album.Name);
        }

        [Fact]
        public async Task GetAsync_Should_Throw_Exception_For_Non_Existing_Photo()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act & Assert
            await Should.ThrowAsync<Exception>(async () =>
            {
                await _photoAppService.GetAsync(nonExistingId);
            });
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Photo_Metadata()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            var photo = await CreateTestPhotoAsync(album.Id);
            var input = new UpdatePhotoDto
            {
                Location = "Updated Location",
                DisplayOrder = 99
            };

            // Act
            var result = await _photoAppService.UpdateAsync(photo.Id, input);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(photo.Id);
            result.Location.ShouldBe(input.Location);
            result.DisplayOrder.ShouldBe(input.DisplayOrder);

            // Verify in database
            var photoInDb = await _photoRepository.GetAsync(photo.Id);
            photoInDb.Location.ShouldBe(input.Location);
            photoInDb.DisplayOrder.ShouldBe(input.DisplayOrder);
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Photo()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            var photo = await CreateTestPhotoAsync(album.Id);

            // Act
            await _photoAppService.DeleteAsync(photo.Id);

            // Assert
            var photoExists = await _photoRepository.FindAsync(photo.Id);
            photoExists.ShouldBeNull();
        }

        [Fact]
        public async Task ReorderAsync_Should_Update_Display_Orders()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            var photo1 = await CreateTestPhotoAsync(album.Id, "Photo1.jpg", displayOrder: 1);
            var photo2 = await CreateTestPhotoAsync(album.Id, "Photo2.jpg", displayOrder: 2);
            var photo3 = await CreateTestPhotoAsync(album.Id, "Photo3.jpg", displayOrder: 3);

            var input = new ReorderPhotosDto
            {
                PhotoOrders = new List<PhotoOrderDto>
                {
                    new PhotoOrderDto { Id = photo3.Id, DisplayOrder = 1 },
                    new PhotoOrderDto { Id = photo1.Id, DisplayOrder = 2 },
                    new PhotoOrderDto { Id = photo2.Id, DisplayOrder = 3 }
                }
            };

            // Act
            await _photoAppService.ReorderAsync(input);

            // Assert
            var updatedPhoto1 = await _photoRepository.GetAsync(photo1.Id);
            var updatedPhoto2 = await _photoRepository.GetAsync(photo2.Id);
            var updatedPhoto3 = await _photoRepository.GetAsync(photo3.Id);

            updatedPhoto1.DisplayOrder.ShouldBe(2);
            updatedPhoto2.DisplayOrder.ShouldBe(3);
            updatedPhoto3.DisplayOrder.ShouldBe(1);
        }

        [Fact]
        public async Task GetByAlbumAsync_Should_Return_Photos_From_Specific_Album()
        {
            // Arrange
            var album1 = await CreateTestAlbumAsync("Album 1");
            var album2 = await CreateTestAlbumAsync("Album 2");
            await CreateTestPhotoAsync(album1.Id, "Photo1.jpg");
            await CreateTestPhotoAsync(album1.Id, "Photo2.jpg");
            await CreateTestPhotoAsync(album2.Id, "Photo3.jpg");

            var input = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0
            };

            // Act
            var result = await _photoAppService.GetByAlbumAsync(album1.Id, input);

            // Assert
            result.ShouldNotBeNull();
            result.TotalCount.ShouldBe(2);
            result.Items.ShouldAllBe(p => p.AlbumId == album1.Id);
        }

        [Fact]
        public async Task GetLocationsAsync_Should_Return_Distinct_Locations()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            await CreateTestPhotoAsync(album.Id, "Photo1.jpg", location: "Paris");
            await CreateTestPhotoAsync(album.Id, "Photo2.jpg", location: "London");
            await CreateTestPhotoAsync(album.Id, "Photo3.jpg", location: "Paris"); // Duplicate
            await CreateTestPhotoAsync(album.Id, "Photo4.jpg", location: "Tokyo");
            await CreateTestPhotoAsync(album.Id, "Photo5.jpg"); // No location

            // Act
            var result = await _photoAppService.GetLocationsAsync();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(3); // Should have 3 distinct locations
            result.ShouldContain("Paris");
            result.ShouldContain("London");
            result.ShouldContain("Tokyo");
        }

        [Fact]
        public async Task GetLocationsAsync_Should_Filter_By_Album()
        {
            // Arrange
            var album1 = await CreateTestAlbumAsync("Album 1");
            var album2 = await CreateTestAlbumAsync("Album 2");
            await CreateTestPhotoAsync(album1.Id, "Photo1.jpg", location: "Paris");
            await CreateTestPhotoAsync(album1.Id, "Photo2.jpg", location: "London");
            await CreateTestPhotoAsync(album2.Id, "Photo3.jpg", location: "Tokyo");

            // Act
            var result = await _photoAppService.GetLocationsAsync(album1.Id);

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);
            result.ShouldContain("Paris");
            result.ShouldContain("London");
            result.ShouldNotContain("Tokyo");
        }

        [Fact]
        public async Task GetRandomPhotosAsync_Should_Return_Requested_Count()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            await CreateSamplePhotosAsync(album.Id);

            var input = new GetRandomPhotosInput
            {
                Count = 3
            };

            // Act
            var result = await _photoAppService.GetRandomPhotosAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBeLessThanOrEqualTo(3);
        }

        [Fact]
        public async Task GetRandomPhotosAsync_Should_Exclude_Specified_Album()
        {
            // Arrange
            var album1 = await CreateTestAlbumAsync("Album 1");
            var album2 = await CreateTestAlbumAsync("Album 2");
            await CreateTestPhotoAsync(album1.Id, "Photo1.jpg");
            await CreateTestPhotoAsync(album1.Id, "Photo2.jpg");
            await CreateTestPhotoAsync(album2.Id, "Photo3.jpg");

            var input = new GetRandomPhotosInput
            {
                Count = 5,
                ExcludeAlbumId = album1.Id
            };

            // Act
            var result = await _photoAppService.GetRandomPhotosAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldAllBe(p => p.AlbumId != album1.Id);
        }

        #region Helper Methods

        private async Task<Album> CreateTestAlbumAsync(string name = "Test Album")
        {
            var album = new Album(
                Guid.NewGuid(),
                name,
                "Test Topic",
                "Test Description"
            );

            return await _albumRepository.InsertAsync(album, autoSave: true);
        }

        private async Task<Photo> CreateTestPhotoAsync(
            Guid albumId,
            string fileName = "test.jpg",
            string location = "Test Location",
            int displayOrder = 0)
        {
            var photo = new Photo(
                Guid.NewGuid(),
                albumId,
                fileName,
                $"/uploads/photos/{albumId}/{fileName}",
                1024,
                displayOrder,
                location,
                $"/uploads/photos/{albumId}/thumb_{fileName}",
                800,
                600,
                DateTime.UtcNow.AddDays(-1)
            );

            return await _photoRepository.InsertAsync(photo, autoSave: true);
        }

        private async Task CreateSamplePhotosAsync(Guid albumId)
        {
            await CreateTestPhotoAsync(albumId, "Photo1.jpg", "Paris", 1);
            await CreateTestPhotoAsync(albumId, "Photo2.jpg", "London", 2);
            await CreateTestPhotoAsync(albumId, "Photo3.jpg", "Tokyo", 3);
            await CreateTestPhotoAsync(albumId, "Photo4.jpg", "New York", 4);
            await CreateTestPhotoAsync(albumId, "Photo5.jpg", "Sydney", 5);
        }

        #endregion
    }
}