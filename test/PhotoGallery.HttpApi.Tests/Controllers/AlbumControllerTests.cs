using Microsoft.AspNetCore.Mvc.Testing;
using PhotoGallery.Albums;
using Shouldly;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Volo.Abp.Application.Dtos;
using Xunit;

namespace PhotoGallery.Controllers
{
    public class AlbumControllerTests : PhotoGalleryHttpApiTestBase
    {
        private readonly HttpClient _client;

        public AlbumControllerTests()
        {
            _client = GetRequiredService<WebApplicationFactory<Program>>().CreateClient();
        }

        [Fact]
        public async Task GetListAsync_Should_Return_Albums_With_Pagination()
        {
            // Arrange
            var requestUri = "/api/albums?maxResultCount=10&skipCount=0";

            // Act
            var response = await _client.GetAsync(requestUri);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PagedResultDto<AlbumDto>>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            result.ShouldNotBeNull();
            result.Items.ShouldNotBeNull();
            result.TotalCount.ShouldBeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task GetListAsync_Should_Filter_By_Topic()
        {
            // Arrange
            var requestUri = "/api/albums?topic=Nature&maxResultCount=10&skipCount=0";

            // Act
            var response = await _client.GetAsync(requestUri);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PagedResultDto<AlbumDto>>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            result.ShouldNotBeNull();
            result.Items.ShouldNotBeNull();
        }

        [Fact]
        public async Task GetAsync_Should_Return_Album_By_Id()
        {
            // Arrange
            var createInput = new CreateAlbumDto
            {
                Title = "Test Album for Get",
                Description = "Test Description",
                Topic = "Travel"
            };

            var createResponse = await CreateAlbumAsync(createInput);
            var createdAlbum = await DeserializeResponseAsync<AlbumDto>(createResponse);

            // Act
            var response = await _client.GetAsync($"/api/albums/{createdAlbum.Id}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var album = await DeserializeResponseAsync<AlbumDto>(response);

            album.ShouldNotBeNull();
            album.Id.ShouldBe(createdAlbum.Id);
            album.Title.ShouldBe(createInput.Title);
            album.Description.ShouldBe(createInput.Description);
            album.Topic.ShouldBe(createInput.Topic);
        }

        [Fact]
        public async Task GetAsync_Should_Return_NotFound_For_NonExistent_Album()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/albums/{nonExistentId}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateAsync_Should_Create_Album_With_Valid_Data()
        {
            // Arrange
            var input = new CreateAlbumDto
            {
                Title = "Integration Test Album",
                Description = "Album created during integration testing",
                Topic = "Technology"
            };

            // Act
            var response = await CreateAlbumAsync(input);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var createdAlbum = await DeserializeResponseAsync<AlbumDto>(response);

            createdAlbum.ShouldNotBeNull();
            createdAlbum.Id.ShouldNotBe(Guid.Empty);
            createdAlbum.Title.ShouldBe(input.Title);
            createdAlbum.Description.ShouldBe(input.Description);
            createdAlbum.Topic.ShouldBe(input.Topic);
            createdAlbum.CreationTime.ShouldBeGreaterThan(default(DateTime));
        }

        [Fact]
        public async Task CreateAsync_Should_Return_BadRequest_For_Invalid_Data()
        {
            // Arrange
            var input = new CreateAlbumDto
            {
                Title = "", // Invalid: empty title
                Description = "Valid description",
                Topic = "Valid Topic"
            };

            // Act
            var response = await CreateAlbumAsync(input);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateAsync_Should_Return_BadRequest_For_XSS_Attack()
        {
            // Arrange
            var input = new CreateAlbumDto
            {
                Title = "<script>alert('xss')</script>Malicious Title",
                Description = "Valid description",
                Topic = "Valid Topic"
            };

            // Act
            var response = await CreateAlbumAsync(input);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Existing_Album()
        {
            // Arrange
            var createInput = new CreateAlbumDto
            {
                Title = "Original Title",
                Description = "Original Description",
                Topic = "Original Topic"
            };

            var createResponse = await CreateAlbumAsync(createInput);
            var createdAlbum = await DeserializeResponseAsync<AlbumDto>(createResponse);

            var updateInput = new UpdateAlbumDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Topic = "Updated Topic"
            };

            // Act
            var response = await UpdateAlbumAsync(createdAlbum.Id, updateInput);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var updatedAlbum = await DeserializeResponseAsync<AlbumDto>(response);

            updatedAlbum.ShouldNotBeNull();
            updatedAlbum.Id.ShouldBe(createdAlbum.Id);
            updatedAlbum.Title.ShouldBe(updateInput.Title);
            updatedAlbum.Description.ShouldBe(updateInput.Description);
            updatedAlbum.Topic.ShouldBe(updateInput.Topic);
            updatedAlbum.LastModificationTime.ShouldNotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_NotFound_For_NonExistent_Album()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var updateInput = new UpdateAlbumDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Topic = "Updated Topic"
            };

            // Act
            var response = await UpdateAlbumAsync(nonExistentId, updateInput);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_Existing_Album()
        {
            // Arrange
            var createInput = new CreateAlbumDto
            {
                Title = "Album to Delete",
                Description = "This album will be deleted",
                Topic = "Test"
            };

            var createResponse = await CreateAlbumAsync(createInput);
            var createdAlbum = await DeserializeResponseAsync<AlbumDto>(createResponse);

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/albums/{createdAlbum.Id}");

            // Assert
            deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

            // Verify album is deleted
            var getResponse = await _client.GetAsync($"/api/albums/{createdAlbum.Id}");
            getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_NotFound_For_NonExistent_Album()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/albums/{nonExistentId}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetByTopicAsync_Should_Return_Albums_For_Specific_Topic()
        {
            // Arrange
            var topic = "TestTopic" + Guid.NewGuid().ToString("N")[..8];
            
            // Create albums with the specific topic
            await CreateAlbumAsync(new CreateAlbumDto
            {
                Title = "First Album",
                Description = "First Description",
                Topic = topic
            });

            await CreateAlbumAsync(new CreateAlbumDto
            {
                Title = "Second Album",
                Description = "Second Description",
                Topic = topic
            });

            // Act
            var response = await _client.GetAsync($"/api/albums/by-topic/{topic}?maxResultCount=10&skipCount=0");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await DeserializeResponseAsync<PagedResultDto<AlbumDto>>(response);

            result.ShouldNotBeNull();
            result.Items.ShouldNotBeNull();
            result.Items.Count.ShouldBeGreaterThanOrEqualTo(2);
            result.Items.ShouldAllBe(album => album.Topic == topic);
        }

        [Fact]
        public async Task API_Should_Support_CORS()
        {
            // Arrange & Act
            var request = new HttpRequestMessage(HttpMethod.Options, "/api/albums");
            request.Headers.Add("Origin", "https://example.com");
            request.Headers.Add("Access-Control-Request-Method", "GET");

            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
            response.Headers.ShouldContain(h => h.Key == "Access-Control-Allow-Origin");
        }

        private async Task<HttpResponseMessage> CreateAlbumAsync(CreateAlbumDto input)
        {
            var json = JsonSerializer.Serialize(input, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _client.PostAsync("/api/albums", content);
        }

        private async Task<HttpResponseMessage> UpdateAlbumAsync(Guid id, UpdateAlbumDto input)
        {
            var json = JsonSerializer.Serialize(input, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _client.PutAsync($"/api/albums/{id}", content);
        }

        private async Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
    }
}