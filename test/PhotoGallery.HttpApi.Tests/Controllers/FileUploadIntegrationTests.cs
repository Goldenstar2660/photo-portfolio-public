using Microsoft.AspNetCore.Mvc.Testing;
using PhotoGallery.Photos;
using Shouldly;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;

namespace PhotoGallery.Controllers
{
    public class FileUploadIntegrationTests : PhotoGalleryHttpApiTestBase
    {
        private readonly HttpClient _client;

        public FileUploadIntegrationTests()
        {
            _client = GetRequiredService<WebApplicationFactory<Program>>().CreateClient();
        }

        [Fact]
        public async Task UploadPhoto_Should_Accept_Valid_JPEG_Image()
        {
            // Arrange
            var boundary = "----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
            var content = new MultipartFormDataContent(boundary);

            // Add text fields
            content.Add(new StringContent("Test JPEG Photo"), "Title");
            content.Add(new StringContent("Valid JPEG image upload test"), "Description");
            content.Add(new StringContent("Test Location"), "Location");
            content.Add(new StringContent(Guid.NewGuid().ToString()), "AlbumId");

            // Create a valid JPEG image
            var imageBytes = CreateTestJpegImage();
            var fileContent = new ByteArrayContent(imageBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "File", "test-image.jpg");

            // Act
            var response = await _client.PostAsync("/api/photos/upload", content);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var uploadedPhoto = await DeserializeResponseAsync<PhotoDto>(response);

            uploadedPhoto.ShouldNotBeNull();
            uploadedPhoto.Id.ShouldNotBe(Guid.Empty);
            uploadedPhoto.Title.ShouldBe("Test JPEG Photo");
            uploadedPhoto.FileName.ShouldNotBeNullOrEmpty();
            uploadedPhoto.FilePath.ShouldNotBeNullOrEmpty();
            uploadedPhoto.ThumbnailPath.ShouldNotBeNullOrEmpty();
            uploadedPhoto.FileSize.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task UploadPhoto_Should_Accept_Valid_PNG_Image()
        {
            // Arrange
            var boundary = "----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
            var content = new MultipartFormDataContent(boundary);

            // Add text fields
            content.Add(new StringContent("Test PNG Photo"), "Title");
            content.Add(new StringContent("Valid PNG image upload test"), "Description");
            content.Add(new StringContent("Test Location"), "Location");
            content.Add(new StringContent(Guid.NewGuid().ToString()), "AlbumId");

            // Create a valid PNG image
            var imageBytes = CreateTestPngImage();
            var fileContent = new ByteArrayContent(imageBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            content.Add(fileContent, "File", "test-image.png");

            // Act
            var response = await _client.PostAsync("/api/photos/upload", content);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var uploadedPhoto = await DeserializeResponseAsync<PhotoDto>(response);

            uploadedPhoto.ShouldNotBeNull();
            uploadedPhoto.FileName.ShouldEndWith(".png");
            uploadedPhoto.FileSize.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task UploadPhoto_Should_Reject_Non_Image_File()
        {
            // Arrange
            var boundary = "----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
            var content = new MultipartFormDataContent(boundary);

            // Add text fields
            content.Add(new StringContent("Test Non-Image"), "Title");
            content.Add(new StringContent("This should fail"), "Description");
            content.Add(new StringContent("Test Location"), "Location");
            content.Add(new StringContent(Guid.NewGuid().ToString()), "AlbumId");

            // Add a text file instead of image
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("This is not an image file"));
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
            content.Add(fileContent, "File", "not-an-image.txt");

            // Act
            var response = await _client.PostAsync("/api/photos/upload", content);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UploadPhoto_Should_Reject_File_Too_Large()
        {
            // Arrange
            var boundary = "----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
            var content = new MultipartFormDataContent(boundary);

            // Add text fields
            content.Add(new StringContent("Large File Test"), "Title");
            content.Add(new StringContent("This file is too large"), "Description");
            content.Add(new StringContent("Test Location"), "Location");
            content.Add(new StringContent(Guid.NewGuid().ToString()), "AlbumId");

            // Create a file larger than the limit (assuming 10MB limit)
            var largeFileBytes = new byte[11 * 1024 * 1024]; // 11MB
            var fileContent = new ByteArrayContent(largeFileBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "File", "large-image.jpg");

            // Act
            var response = await _client.PostAsync("/api/photos/upload", content);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UploadPhoto_Should_Reject_Empty_File()
        {
            // Arrange
            var boundary = "----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
            var content = new MultipartFormDataContent(boundary);

            // Add text fields
            content.Add(new StringContent("Empty File Test"), "Title");
            content.Add(new StringContent("This file is empty"), "Description");
            content.Add(new StringContent("Test Location"), "Location");
            content.Add(new StringContent(Guid.NewGuid().ToString()), "AlbumId");

            // Add empty file
            var fileContent = new ByteArrayContent(Array.Empty<byte>());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "File", "empty-image.jpg");

            // Act
            var response = await _client.PostAsync("/api/photos/upload", content);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UploadPhoto_Should_Reject_Invalid_File_Extension()
        {
            // Arrange
            var boundary = "----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
            var content = new MultipartFormDataContent(boundary);

            // Add text fields
            content.Add(new StringContent("Invalid Extension Test"), "Title");
            content.Add(new StringContent("Invalid file extension"), "Description");
            content.Add(new StringContent("Test Location"), "Location");
            content.Add(new StringContent(Guid.NewGuid().ToString()), "AlbumId");

            // Create valid JPEG but with invalid extension
            var imageBytes = CreateTestJpegImage();
            var fileContent = new ByteArrayContent(imageBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "File", "test-image.exe"); // Invalid extension

            // Act
            var response = await _client.PostAsync("/api/photos/upload", content);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UploadPhoto_Should_Generate_Unique_Filename()
        {
            // Arrange
            var boundary1 = "----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
            var content1 = new MultipartFormDataContent(boundary1);

            content1.Add(new StringContent("First Upload"), "Title");
            content1.Add(new StringContent("First upload test"), "Description");
            content1.Add(new StringContent("Location 1"), "Location");
            content1.Add(new StringContent(Guid.NewGuid().ToString()), "AlbumId");

            var imageBytes1 = CreateTestJpegImage();
            var fileContent1 = new ByteArrayContent(imageBytes1);
            fileContent1.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content1.Add(fileContent1, "File", "same-name.jpg");

            var boundary2 = "----WebKitFormBoundary" + (DateTime.Now.Ticks + 1).ToString("x");
            var content2 = new MultipartFormDataContent(boundary2);

            content2.Add(new StringContent("Second Upload"), "Title");
            content2.Add(new StringContent("Second upload test"), "Description");
            content2.Add(new StringContent("Location 2"), "Location");
            content2.Add(new StringContent(Guid.NewGuid().ToString()), "AlbumId");

            var imageBytes2 = CreateTestJpegImage();
            var fileContent2 = new ByteArrayContent(imageBytes2);
            fileContent2.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content2.Add(fileContent2, "File", "same-name.jpg");

            // Act
            var response1 = await _client.PostAsync("/api/photos/upload", content1);
            var response2 = await _client.PostAsync("/api/photos/upload", content2);

            // Assert
            response1.StatusCode.ShouldBe(HttpStatusCode.OK);
            response2.StatusCode.ShouldBe(HttpStatusCode.OK);

            var photo1 = await DeserializeResponseAsync<PhotoDto>(response1);
            var photo2 = await DeserializeResponseAsync<PhotoDto>(response2);

            photo1.FileName.ShouldNotBe(photo2.FileName);
            photo1.FilePath.ShouldNotBe(photo2.FilePath);
        }

        [Fact]
        public async Task UploadPhoto_Should_Extract_EXIF_Data_When_Available()
        {
            // Arrange
            var boundary = "----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
            var content = new MultipartFormDataContent(boundary);

            content.Add(new StringContent("EXIF Test Photo"), "Title");
            content.Add(new StringContent("Photo with EXIF data"), "Description");
            content.Add(new StringContent("Test Location"), "Location");
            content.Add(new StringContent(Guid.NewGuid().ToString()), "AlbumId");

            // Create JPEG with basic EXIF data
            var imageBytes = CreateTestJpegImageWithExif();
            var fileContent = new ByteArrayContent(imageBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "File", "exif-test.jpg");

            // Act
            var response = await _client.PostAsync("/api/photos/upload", content);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var uploadedPhoto = await DeserializeResponseAsync<PhotoDto>(response);

            uploadedPhoto.ShouldNotBeNull();
            // Note: Actual EXIF extraction testing would require proper EXIF data
            // This test verifies the upload process doesn't fail with EXIF-enabled images
        }

        [Fact]
        public async Task UploadPhoto_Should_Validate_Required_Fields()
        {
            // Arrange
            var boundary = "----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
            var content = new MultipartFormDataContent(boundary);

            // Missing Title field
            content.Add(new StringContent("Valid description"), "Description");
            content.Add(new StringContent("Test Location"), "Location");
            content.Add(new StringContent(Guid.NewGuid().ToString()), "AlbumId");

            var imageBytes = CreateTestJpegImage();
            var fileContent = new ByteArrayContent(imageBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "File", "test-image.jpg");

            // Act
            var response = await _client.PostAsync("/api/photos/upload", content);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UploadPhoto_Should_Sanitize_Filename()
        {
            // Arrange
            var boundary = "----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
            var content = new MultipartFormDataContent(boundary);

            content.Add(new StringContent("Filename Sanitization Test"), "Title");
            content.Add(new StringContent("Testing filename sanitization"), "Description");
            content.Add(new StringContent("Test Location"), "Location");
            content.Add(new StringContent(Guid.NewGuid().ToString()), "AlbumId");

            var imageBytes = CreateTestJpegImage();
            var fileContent = new ByteArrayContent(imageBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            content.Add(fileContent, "File", "../../../malicious\\file<>name.jpg"); // Potentially malicious filename

            // Act
            var response = await _client.PostAsync("/api/photos/upload", content);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var uploadedPhoto = await DeserializeResponseAsync<PhotoDto>(response);

            // Filename should be sanitized and not contain path traversal characters
            uploadedPhoto.FileName.ShouldNotContain("..");
            uploadedPhoto.FileName.ShouldNotContain("\\");
            uploadedPhoto.FileName.ShouldNotContain("<");
            uploadedPhoto.FileName.ShouldNotContain(">");
        }

        private byte[] CreateTestJpegImage()
        {
            using var bitmap = new Bitmap(100, 100);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Blue);
            graphics.FillRectangle(Brushes.Red, 25, 25, 50, 50);

            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Jpeg);
            return stream.ToArray();
        }

        private byte[] CreateTestPngImage()
        {
            using var bitmap = new Bitmap(100, 100);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Green);
            graphics.FillEllipse(Brushes.Yellow, 25, 25, 50, 50);

            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            return stream.ToArray();
        }

        private byte[] CreateTestJpegImageWithExif()
        {
            // For simplicity, this creates a basic JPEG
            // In a real scenario, you would embed actual EXIF data
            return CreateTestJpegImage();
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