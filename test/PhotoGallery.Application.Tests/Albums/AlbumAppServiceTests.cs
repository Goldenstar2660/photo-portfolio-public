using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;
using Xunit;

namespace PhotoGallery.Albums
{
    public class AlbumAppServiceTests : PhotoGalleryApplicationTestBase<PhotoGalleryApplicationTestModule>
    {
        private readonly IAlbumAppService _albumAppService;
        private readonly IRepository<Album, Guid> _albumRepository;

        public AlbumAppServiceTests()
        {
            _albumAppService = GetRequiredService<IAlbumAppService>();
            _albumRepository = GetRequiredService<IRepository<Album, Guid>>();
        }

        [Fact]
        public async Task GetListAsync_Should_Return_Paginated_Albums()
        {
            // Arrange
            await CreateSampleAlbumsAsync();
            var input = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "Name"
            };

            // Act
            var result = await _albumAppService.GetListAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.TotalCount.ShouldBeGreaterThan(0);
            result.Items.ShouldNotBeEmpty();
            result.Items.Count.ShouldBeLessThanOrEqualTo(10);
        }

        [Fact]
        public async Task GetAsync_Should_Return_Album_By_Id()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();

            // Act
            var result = await _albumAppService.GetAsync(album.Id);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(album.Id);
            result.Name.ShouldBe(album.Name);
            result.Topic.ShouldBe(album.Topic);
            result.Description.ShouldBe(album.Description);
        }

        [Fact]
        public async Task GetAsync_Should_Throw_Exception_For_Non_Existing_Album()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act & Assert
            await Should.ThrowAsync<Exception>(async () =>
            {
                await _albumAppService.GetAsync(nonExistingId);
            });
        }

        [Fact]
        public async Task CreateAsync_Should_Create_Album_With_Valid_Input()
        {
            // Arrange
            var input = new CreateAlbumDto
            {
                Name = "Test Album",
                Topic = "Test Topic",
                Description = "Test Description",
                DisplayOrder = 1
            };

            // Act
            var result = await _albumAppService.CreateAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldNotBe(Guid.Empty);
            result.Name.ShouldBe(input.Name);
            result.Topic.ShouldBe(input.Topic);
            result.Description.ShouldBe(input.Description);
            result.DisplayOrder.ShouldBe(input.DisplayOrder);
            result.PhotoCount.ShouldBe(0);

            // Verify in database
            var albumInDb = await _albumRepository.GetAsync(result.Id);
            albumInDb.ShouldNotBeNull();
            albumInDb.Name.ShouldBe(input.Name);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_Exception_With_Invalid_Input()
        {
            // Arrange
            var input = new CreateAlbumDto
            {
                Name = "", // Invalid: empty name
                Topic = "Test Topic",
                Description = "Test Description"
            };

            // Act & Assert
            await Should.ThrowAsync<AbpValidationException>(async () =>
            {
                await _albumAppService.CreateAsync(input);
            });
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_Exception_With_Long_Name()
        {
            // Arrange
            var input = new CreateAlbumDto
            {
                Name = new string('A', 101), // Invalid: too long
                Topic = "Test Topic",
                Description = "Test Description"
            };

            // Act & Assert
            await Should.ThrowAsync<AbpValidationException>(async () =>
            {
                await _albumAppService.CreateAsync(input);
            });
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Album_With_Valid_Input()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();
            var input = new UpdateAlbumDto
            {
                Name = "Updated Album",
                Topic = "Updated Topic",
                Description = "Updated Description"
            };

            // Act
            var result = await _albumAppService.UpdateAsync(album.Id, input);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(album.Id);
            result.Name.ShouldBe(input.Name);
            result.Topic.ShouldBe(input.Topic);
            result.Description.ShouldBe(input.Description);

            // Verify in database
            var albumInDb = await _albumRepository.GetAsync(album.Id);
            albumInDb.Name.ShouldBe(input.Name);
            albumInDb.Topic.ShouldBe(input.Topic);
            albumInDb.Description.ShouldBe(input.Description);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_Exception_For_Non_Existing_Album()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();
            var input = new UpdateAlbumDto
            {
                Name = "Updated Album",
                Topic = "Updated Topic"
            };

            // Act & Assert
            await Should.ThrowAsync<Exception>(async () =>
            {
                await _albumAppService.UpdateAsync(nonExistingId, input);
            });
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Album()
        {
            // Arrange
            var album = await CreateTestAlbumAsync();

            // Act
            await _albumAppService.DeleteAsync(album.Id);

            // Assert
            var albumExists = await _albumRepository.FindAsync(album.Id);
            albumExists.ShouldBeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_Exception_For_Non_Existing_Album()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act & Assert
            await Should.ThrowAsync<Exception>(async () =>
            {
                await _albumAppService.DeleteAsync(nonExistingId);
            });
        }

        [Fact]
        public async Task ReorderAsync_Should_Update_Display_Orders()
        {
            // Arrange
            var album1 = await CreateTestAlbumAsync("Album 1", "Topic 1", displayOrder: 1);
            var album2 = await CreateTestAlbumAsync("Album 2", "Topic 2", displayOrder: 2);
            var album3 = await CreateTestAlbumAsync("Album 3", "Topic 3", displayOrder: 3);

            var input = new ReorderAlbumsDto
            {
                AlbumOrders = new List<AlbumOrderDto>
                {
                    new AlbumOrderDto { Id = album3.Id, DisplayOrder = 1 },
                    new AlbumOrderDto { Id = album1.Id, DisplayOrder = 2 },
                    new AlbumOrderDto { Id = album2.Id, DisplayOrder = 3 }
                }
            };

            // Act
            await _albumAppService.ReorderAsync(input);

            // Assert
            var updatedAlbum1 = await _albumRepository.GetAsync(album1.Id);
            var updatedAlbum2 = await _albumRepository.GetAsync(album2.Id);
            var updatedAlbum3 = await _albumRepository.GetAsync(album3.Id);

            updatedAlbum1.DisplayOrder.ShouldBe(2);
            updatedAlbum2.DisplayOrder.ShouldBe(3);
            updatedAlbum3.DisplayOrder.ShouldBe(1);
        }

        [Fact]
        public async Task GetTopicsAsync_Should_Return_Distinct_Topics()
        {
            // Arrange
            await CreateTestAlbumAsync("Album 1", "Photography");
            await CreateTestAlbumAsync("Album 2", "Travel");
            await CreateTestAlbumAsync("Album 3", "Photography"); // Duplicate topic
            await CreateTestAlbumAsync("Album 4", "Nature");

            // Act
            var result = await _albumAppService.GetTopicsAsync();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(3); // Should have 3 distinct topics
            result.ShouldContain("Photography");
            result.ShouldContain("Travel");
            result.ShouldContain("Nature");
        }

        [Fact]
        public async Task GetByTopicAsync_Should_Return_Albums_With_Specific_Topic()
        {
            // Arrange
            await CreateTestAlbumAsync("Album 1", "Photography");
            await CreateTestAlbumAsync("Album 2", "Travel");
            await CreateTestAlbumAsync("Album 3", "Photography");
            await CreateTestAlbumAsync("Album 4", "Nature");

            var input = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0
            };

            // Act
            var result = await _albumAppService.GetByTopicAsync("Photography", input);

            // Assert
            result.ShouldNotBeNull();
            result.TotalCount.ShouldBe(2);
            result.Items.Count.ShouldBe(2);
            result.Items.ShouldAllBe(album => album.Topic == "Photography");
        }

        [Fact]
        public async Task GetByTopicAsync_Should_Return_Empty_For_Non_Existing_Topic()
        {
            // Arrange
            await CreateSampleAlbumsAsync();
            var input = new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 10,
                SkipCount = 0
            };

            // Act
            var result = await _albumAppService.GetByTopicAsync("NonExistingTopic", input);

            // Assert
            result.ShouldNotBeNull();
            result.TotalCount.ShouldBe(0);
            result.Items.ShouldBeEmpty();
        }

        #region Helper Methods

        private async Task<Album> CreateTestAlbumAsync(
            string name = "Test Album",
            string topic = "Test Topic",
            string description = "Test Description",
            int displayOrder = 0)
        {
            var album = new Album(
                Guid.NewGuid(),
                name,
                topic,
                description,
                displayOrder
            );

            return await _albumRepository.InsertAsync(album, autoSave: true);
        }

        private async Task CreateSampleAlbumsAsync()
        {
            await CreateTestAlbumAsync("Sample Album 1", "Photography", "First sample album", 1);
            await CreateTestAlbumAsync("Sample Album 2", "Travel", "Second sample album", 2);
            await CreateTestAlbumAsync("Sample Album 3", "Nature", "Third sample album", 3);
        }

        #endregion
    }
}