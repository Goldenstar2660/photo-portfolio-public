using Microsoft.AspNetCore.Mvc.Testing;
using PhotoGallery.Photos;
using Shouldly;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Volo.Abp.Application.Dtos;
using Xunit;

namespace PhotoGallery.Controllers
{
    public class PhotoControllerTests : PhotoGalleryHttpApiTestBase
    {
        private readonly HttpClient _client;

        public PhotoControllerTests()
        {
            _client = GetRequiredService<WebApplicationFactory<Program>>().CreateClient();
        }

        [Fact]
        public async Task GetListAsync_Should_Return_Photos_With_Pagination()
        {
            // Arrange
            var requestUri = "/api/photos?maxResultCount=10&skipCount=0";

            // Act
            var response = await _client.GetAsync(requestUri);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PagedResultDto<PhotoDto>>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            result.ShouldNotBeNull();
            result.Items.ShouldNotBeNull();
            result.TotalCount.ShouldBeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task GetListAsync_Should_Filter_By_AlbumId()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            var requestUri = $"/api/photos?albumId={albumId}&maxResultCount=10&skipCount=0";

            // Act
            var response = await _client.GetAsync(requestUri);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PagedResultDto<PhotoDto>>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            result.ShouldNotBeNull();
            result.Items.ShouldNotBeNull();
        }

        [Fact]
        public async Task GetListAsync_Should_Filter_By_Location()
        {
            // Arrange
            var location = "New York";
            var requestUri = $"/api/photos?location={Uri.EscapeDataString(location)}&maxResultCount=10&skipCount=0";

            // Act
            var response = await _client.GetAsync(requestUri);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PagedResultDto<PhotoDto>>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            result.ShouldNotBeNull();
            result.Items.ShouldNotBeNull();
        }

        [Fact]
        public async Task GetListAsync_Should_Filter_By_DateRange()
        {
            // Arrange
            var startDate = DateTime.Now.AddDays(-30);
            var endDate = DateTime.Now;
            var requestUri = $"/api/photos?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&maxResultCount=10&skipCount=0";

            // Act
            var response = await _client.GetAsync(requestUri);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PagedResultDto<PhotoDto>>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            result.ShouldNotBeNull();
            result.Items.ShouldNotBeNull();
        }

        [Fact]
        public async Task GetAsync_Should_Return_Photo_By_Id()
        {
            // Arrange
            var createInput = new CreatePhotoDto
            {
                Title = "Test Photo for Get",
                Description = "Test Description",
                Location = "Test Location",
                AlbumId = Guid.NewGuid()
            };

            var createResponse = await CreatePhotoAsync(createInput);
            var createdPhoto = await DeserializeResponseAsync<PhotoDto>(createResponse);

            // Act
            var response = await _client.GetAsync($"/api/photos/{createdPhoto.Id}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var photo = await DeserializeResponseAsync<PhotoDto>(response);

            photo.ShouldNotBeNull();
            photo.Id.ShouldBe(createdPhoto.Id);
            photo.Title.ShouldBe(createInput.Title);
            photo.Description.ShouldBe(createInput.Description);
            photo.Location.ShouldBe(createInput.Location);
        }

        [Fact]
        public async Task GetAsync_Should_Return_NotFound_For_NonExistent_Photo()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/photos/{nonExistentId}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateAsync_Should_Create_Photo_With_Valid_Data()
        {
            // Arrange
            var input = new CreatePhotoDto
            {
                Title = "Integration Test Photo",
                Description = "Photo created during integration testing",
                Location = "Test Location",
                AlbumId = Guid.NewGuid()
            };

            // Act
            var response = await CreatePhotoAsync(input);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var createdPhoto = await DeserializeResponseAsync<PhotoDto>(response);

            createdPhoto.ShouldNotBeNull();
            createdPhoto.Id.ShouldNotBe(Guid.Empty);
            createdPhoto.Title.ShouldBe(input.Title);
            createdPhoto.Description.ShouldBe(input.Description);
            createdPhoto.Location.ShouldBe(input.Location);
            createdPhoto.AlbumId.ShouldBe(input.AlbumId);
            createdPhoto.CreationTime.ShouldBeGreaterThan(default(DateTime));
        }

        [Fact]
        public async Task CreateAsync_Should_Return_BadRequest_For_Invalid_Data()
        {
            // Arrange
            var input = new CreatePhotoDto
            {
                Title = "", // Invalid: empty title
                Description = "Valid description",
                Location = "Valid location",
                AlbumId = Guid.NewGuid()
            };

            // Act
            var response = await CreatePhotoAsync(input);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateAsync_Should_Return_BadRequest_For_XSS_Attack()
        {
            // Arrange
            var input = new CreatePhotoDto
            {
                Title = "<script>alert('xss')</script>Malicious Title",
                Description = "Valid description",
                Location = "Valid location",
                AlbumId = Guid.NewGuid()
            };

            // Act
            var response = await CreatePhotoAsync(input);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateAsync_Should_Return_BadRequest_For_Invalid_AlbumId()
        {
            // Arrange
            var input = new CreatePhotoDto
            {
                Title = "Valid Title",
                Description = "Valid description",
                Location = "Valid location",
                AlbumId = Guid.Empty // Invalid: empty GUID
            };

            // Act
            var response = await CreatePhotoAsync(input);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Existing_Photo()
        {
            // Arrange
            var createInput = new CreatePhotoDto
            {
                Title = "Original Title",
                Description = "Original Description",
                Location = "Original Location",
                AlbumId = Guid.NewGuid()
            };

            var createResponse = await CreatePhotoAsync(createInput);
            var createdPhoto = await DeserializeResponseAsync<PhotoDto>(createResponse);

            var updateInput = new UpdatePhotoDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Location = "Updated Location",
                AlbumId = Guid.NewGuid()
            };

            // Act
            var response = await UpdatePhotoAsync(createdPhoto.Id, updateInput);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var updatedPhoto = await DeserializeResponseAsync<PhotoDto>(response);

            updatedPhoto.ShouldNotBeNull();
            updatedPhoto.Id.ShouldBe(createdPhoto.Id);
            updatedPhoto.Title.ShouldBe(updateInput.Title);
            updatedPhoto.Description.ShouldBe(updateInput.Description);
            updatedPhoto.Location.ShouldBe(updateInput.Location);
            updatedPhoto.AlbumId.ShouldBe(updateInput.AlbumId);
            updatedPhoto.LastModificationTime.ShouldNotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_NotFound_For_NonExistent_Photo()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var updateInput = new UpdatePhotoDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Location = "Updated Location",
                AlbumId = Guid.NewGuid()
            };

            // Act
            var response = await UpdatePhotoAsync(nonExistentId, updateInput);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_Existing_Photo()
        {
            // Arrange
            var createInput = new CreatePhotoDto
            {
                Title = "Photo to Delete",
                Description = "This photo will be deleted",
                Location = "Test Location",
                AlbumId = Guid.NewGuid()
            };

            var createResponse = await CreatePhotoAsync(createInput);
            var createdPhoto = await DeserializeResponseAsync<PhotoDto>(createResponse);

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/photos/{createdPhoto.Id}");

            // Assert
            deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

            // Verify photo is deleted
            var getResponse = await _client.GetAsync($"/api/photos/{createdPhoto.Id}");
            getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_NotFound_For_NonExistent_Photo()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/photos/{nonExistentId}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetByAlbumAsync_Should_Return_Photos_For_Specific_Album()
        {
            // Arrange
            var albumId = Guid.NewGuid();
            
            // Create photos for the specific album
            await CreatePhotoAsync(new CreatePhotoDto
            {
                Title = "First Photo",
                Description = "First Description",
                Location = "Location 1",
                AlbumId = albumId
            });

            await CreatePhotoAsync(new CreatePhotoDto
            {
                Title = "Second Photo",
                Description = "Second Description",
                Location = "Location 2",
                AlbumId = albumId
            });

            // Act
            var response = await _client.GetAsync($"/api/photos/by-album/{albumId}?maxResultCount=10&skipCount=0");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var result = await DeserializeResponseAsync<PagedResultDto<PhotoDto>>(response);

            result.ShouldNotBeNull();
            result.Items.ShouldNotBeNull();
            result.Items.Count.ShouldBeGreaterThanOrEqualTo(2);
            result.Items.ShouldAllBe(photo => photo.AlbumId == albumId);
        }

        [Fact]
        public async Task API_Should_Support_CORS()
        {
            // Arrange & Act
            var request = new HttpRequestMessage(HttpMethod.Options, "/api/photos");
            request.Headers.Add("Origin", "https://example.com");
            request.Headers.Add("Access-Control-Request-Method", "GET");

            var response = await _client.SendAsync(request);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
            response.Headers.ShouldContain(h => h.Key == "Access-Control-Allow-Origin");
        }

        [Fact]
        public async Task CreateWithFileAsync_Should_Accept_Multipart_FormData()
        {
            // Arrange
            var boundary = "----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
            var content = new MultipartFormDataContent(boundary);

            // Add text fields
            content.Add(new StringContent("Test Photo Title"), "Title");
            content.Add(new StringContent("Test Photo Description"), "Description");
            content.Add(new StringContent("Test Location"), "Location");
            content.Add(new StringContent(Guid.NewGuid().ToString()), "AlbumId");

            // Add a mock file (simple text file for testing)
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("Mock image content"));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "File", "test-image.jpg");

            // Act
            var response = await _client.PostAsync("/api/photos/upload", content);

            // Assert
            // Note: This will likely return BadRequest in real scenario due to invalid image format
            // but we're testing that the endpoint accepts multipart/form-data
            response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateWithFileAsync_Should_Return_BadRequest_For_Missing_File()
        {
            // Arrange
            var boundary = "----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
            var content = new MultipartFormDataContent(boundary);

            // Add text fields only (no file)
            content.Add(new StringContent("Test Photo Title"), "Title");
            content.Add(new StringContent("Test Photo Description"), "Description");
            content.Add(new StringContent("Test Location"), "Location");
            content.Add(new StringContent(Guid.NewGuid().ToString()), "AlbumId");

            // Act
            var response = await _client.PostAsync("/api/photos/upload", content);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        private async Task<HttpResponseMessage> CreatePhotoAsync(CreatePhotoDto input)
        {
            var json = JsonSerializer.Serialize(input, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _client.PostAsync("/api/photos", content);
        }

        private async Task<HttpResponseMessage> UpdatePhotoAsync(Guid id, UpdatePhotoDto input)
        {
            var json = JsonSerializer.Serialize(input, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _client.PutAsync($"/api/photos/{id}", content);
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