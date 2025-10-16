using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Jpeg;
using Volo.Abp;
using Amazon.S3;
using Amazon.S3.Model;
using PhotoGallery.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace PhotoGallery.Application.Photos
{
    public class PhotoStorageService : PhotoGalleryAppService, IPhotoStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PhotoStorageService> _logger;
        private readonly IAmazonS3? _s3Client;
        
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
        private const int ThumbnailWidth = 300;
        private const int ThumbnailHeight = 200;

        public PhotoStorageService(
            IConfiguration configuration,
            ILogger<PhotoStorageService> logger,
            IAmazonS3? s3Client = null)
        {
            _configuration = configuration;
            _logger = logger;
            _s3Client = s3Client;
        }

        public async Task<FileUploadResult> UploadPhotoAsync(IFormFile file, Guid albumId)
        {
            if (_s3Client == null)
            {
                throw new UserFriendlyException("Photo storage service is not configured. Cannot upload photos.");
            }

            if (!await ValidateFileAsync(file))
            {
                throw new UserFriendlyException("Invalid file format or size");
            }

            try
            {
                // Generate secure file name
                var fileName = GenerateSecureFileName(file.FileName, albumId);
                var bucketName = _configuration["Cloudflare:Bucket"];
                
                if (string.IsNullOrEmpty(bucketName))
                {
                    throw new UserFriendlyException("Cloudflare bucket configuration is missing");
                }
                
                var objectKey = fileName;
                var thumbnailKey = $"thumb_{fileName}";

                // Prepare upload metadata
                var result = new FileUploadResult
                {
                    FileName = fileName,
                    FileSize = file.Length,
                    FilePath = objectKey,
                    ThumbnailPath = thumbnailKey
                };

                // Upload original file to Cloudflare R2
                using (var stream = file.OpenReadStream())
                {
                    await UploadToCloudflareAsync(bucketName, objectKey, stream, file.ContentType);
                }

                // Extract metadata before creating thumbnail
                using (var metadataStream = file.OpenReadStream())
                {
                    await ExtractImageMetadataFromStreamAsync(metadataStream, result);
                }

                // Generate and upload thumbnail
                using (var thumbnailStream = file.OpenReadStream())
                {
                    await GenerateAndUploadThumbnailAsync(thumbnailStream, bucketName, thumbnailKey);
                }

                _logger.LogInformation("Successfully uploaded photo {FileName} to Cloudflare R2 for album {AlbumId}", fileName, albumId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photo to Cloudflare R2 for album {AlbumId}", albumId);
                throw new UserFriendlyException("Failed to upload photo. Please try again.");
            }
        }

        public async Task DeletePhotoAsync(string filePath, string thumbnailPath)
        {
            if (_s3Client == null)
            {
                _logger.LogWarning("Photo storage service is not configured, cannot delete files from cloud storage");
                return;
            }

            try
            {
                var bucketName = _configuration["Cloudflare:Bucket"];
                
                if (string.IsNullOrEmpty(bucketName))
                {
                    _logger.LogWarning("Cloudflare bucket configuration is missing, cannot delete files");
                    return;
                }

                // Delete original file
                if (!string.IsNullOrEmpty(filePath))
                {
                    await DeleteFromCloudflareAsync(bucketName, filePath);
                    _logger.LogInformation("Deleted photo file from Cloudflare R2: {FilePath}", filePath);
                }

                // Delete thumbnail
                if (!string.IsNullOrEmpty(thumbnailPath))
                {
                    await DeleteFromCloudflareAsync(bucketName, thumbnailPath);
                    _logger.LogInformation("Deleted thumbnail file from Cloudflare R2: {ThumbnailPath}", thumbnailPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting photo files from Cloudflare R2: {FilePath}, {ThumbnailPath}", filePath, thumbnailPath);
                // Don't throw exception for file deletion errors as the database operation should still succeed
            }
        }

        public string GenerateSecureFileName(string originalFileName, Guid albumId)
        {
            if (string.IsNullOrEmpty(originalFileName))
            {
                throw new ArgumentException("Original filename cannot be null or empty", nameof(originalFileName));
            }

            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new UserFriendlyException($"File extension {extension} is not allowed");
            }

            // Generate secure filename with timestamp and GUID to prevent conflicts
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var uniqueId = Guid.NewGuid().ToString("N")[..8]; // First 8 characters
            var safeFileName = Path.GetFileNameWithoutExtension(originalFileName)
                .Replace(" ", "_")
                .Replace("-", "_");
            
            // Remove any potentially dangerous characters
            var cleanFileName = new string(safeFileName.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
            if (string.IsNullOrEmpty(cleanFileName))
            {
                cleanFileName = "photo";
            }

            return $"{timestamp}_{uniqueId}_{cleanFileName}{extension}";
        }

        public async Task<bool> ValidateFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            // Check file size
            if (file.Length > MaxFileSize)
            {
                return false;
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return false;
            }

            // Check MIME type
            if (!AllowedMimeTypes.Contains(file.ContentType))
            {
                return false;
            }

            // Validate file content by trying to read it as an image
            try
            {
                using var stream = file.OpenReadStream();
                using var image = Image.Load(stream);
                // If we can create an Image object, it's a valid image file
                return await Task.FromResult(true);
            }
            catch
            {
                return false;
            }
        }

        public string GetCdnUrl(string objectKey)
        {
            var cdnDomain = _configuration["Cloudflare:CdnDomain"];
            if (string.IsNullOrEmpty(cdnDomain))
            {
                // Fallback to direct R2 URL if CDN is not configured
                var accountId = _configuration["Cloudflare:AccountId"];
                var bucketName = _configuration["Cloudflare:Bucket"];
                return $"https://{accountId}.r2.cloudflarestorage.com/{bucketName}/{objectKey}";
            }
            
            return $"https://{cdnDomain}/{objectKey}";
        }

        private async Task UploadToCloudflareAsync(string bucketName, string objectKey, Stream fileStream, string contentType)
        {
            if (_s3Client == null)
            {
                throw new InvalidOperationException("S3 client is not configured");
            }

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                InputStream = fileStream,
                ContentType = contentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            var response = await _s3Client.PutObjectAsync(request);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new UserFriendlyException($"Failed to upload file to Cloudflare R2. Status: {response.HttpStatusCode}");
            }
        }

        private async Task DeleteFromCloudflareAsync(string bucketName, string objectKey)
        {
            if (_s3Client == null)
            {
                throw new InvalidOperationException("S3 client is not configured");
            }

            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey
            };

            await _s3Client.DeleteObjectAsync(request);
        }

        private async Task ExtractImageMetadataFromStreamAsync(Stream imageStream, FileUploadResult result)
        {
            try
            {
                // Extract basic image dimensions
                using var image = Image.Load(imageStream);
                result.Width = image.Width;
                result.Height = image.Height;

                // Reset stream position for metadata extraction
                imageStream.Position = 0;

                // Extract EXIF data
                var directories = ImageMetadataReader.ReadMetadata(imageStream);

                // Try to get date taken from EXIF
                var exifSubIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                if (exifSubIfdDirectory != null)
                {
                    if (exifSubIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTime, out var dateTaken))
                    {
                        result.DateTaken = dateTaken;
                    }
                    else if (exifSubIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dateOriginal))
                    {
                        result.DateTaken = dateOriginal;
                    }
                }

                // Try to get GPS location
                var gpsDirectory = directories.OfType<MetadataExtractor.Formats.Exif.GpsDirectory>().FirstOrDefault();
                if (gpsDirectory != null && gpsDirectory.HasTagName(MetadataExtractor.Formats.Exif.GpsDirectory.TagLatitude)
                    && gpsDirectory.TryGetGeoLocation(out var geoLocation) && !geoLocation.IsZero)
                {
                    result.Location = $"{geoLocation.Latitude:F6}, {geoLocation.Longitude:F6}";
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract EXIF data from image stream");
                // Don't throw exception, just log the warning and continue
            }
        }

        private async Task GenerateAndUploadThumbnailAsync(Stream originalStream, string bucketName, string thumbnailKey)
        {
            try
            {
                using var originalImage = Image.Load(originalStream);
                
                // Calculate thumbnail dimensions maintaining aspect ratio
                var aspectRatio = (double)originalImage.Width / originalImage.Height;
                int thumbWidth, thumbHeight;

                if (aspectRatio > (double)ThumbnailWidth / ThumbnailHeight)
                {
                    // Image is wider than thumbnail ratio
                    thumbWidth = ThumbnailWidth;
                    thumbHeight = (int)(ThumbnailWidth / aspectRatio);
                }
                else
                {
                    // Image is taller than thumbnail ratio
                    thumbHeight = ThumbnailHeight;
                    thumbWidth = (int)(ThumbnailHeight * aspectRatio);
                }

                // Resize the image to create thumbnail
                originalImage.Mutate(x => x.Resize(thumbWidth, thumbHeight));

                // Save thumbnail to memory stream
                using var thumbnailStream = new MemoryStream();
                
                // Save as JPEG with quality settings
                var jpegEncoder = new JpegEncoder { Quality = 85 };
                await originalImage.SaveAsync(thumbnailStream, jpegEncoder);

                thumbnailStream.Position = 0;

                // Upload thumbnail to Cloudflare R2
                await UploadToCloudflareAsync(bucketName, thumbnailKey, thumbnailStream, "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate and upload thumbnail for key {ThumbnailKey}", thumbnailKey);
                throw new UserFriendlyException("Failed to generate thumbnail");
            }
        }
    }
}