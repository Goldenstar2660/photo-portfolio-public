using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Jpeg;
using IoDirectory = System.IO.Directory;  // Alias to avoid ambiguity
using Volo.Abp;
using PhotoGallery.Services;

namespace PhotoGallery.Application.Services
{
    public class FileUploadService : PhotoGalleryAppService, IFileUploadService
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<FileUploadService> _logger;
        
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
        private const int ThumbnailWidth = 300;
        private const int ThumbnailHeight = 200;

        public FileUploadService(IHostEnvironment hostEnvironment, ILogger<FileUploadService> logger)
        {
            _hostEnvironment = hostEnvironment;
            _logger = logger;
        }

        public async Task<FileUploadResult> UploadPhotoAsync(IFormFile file, Guid albumId)
        {
            if (!await ValidateFileAsync(file))
            {
                throw new UserFriendlyException("Invalid file format or size");
            }

            try
            {
                // Generate secure file name
                var fileName = GenerateSecureFileName(file.FileName, albumId);
                var uploadPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "uploads", "photos", albumId.ToString());
                
                // Ensure directory exists
                System.IO.Directory.CreateDirectory(uploadPath);
                
                var filePath = Path.Combine(uploadPath, fileName);
                var thumbnailPath = Path.Combine(uploadPath, "thumb_" + fileName);

                // Save original file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var result = new FileUploadResult
                {
                    FilePath = Path.Combine("uploads", "photos", albumId.ToString(), fileName).Replace('\\', '/'),
                    ThumbnailPath = Path.Combine("uploads", "photos", albumId.ToString(), "thumb_" + fileName).Replace('\\', '/'),
                    FileName = fileName,
                    FileSize = file.Length
                };

                // Extract image dimensions and EXIF data
                await ExtractImageMetadataAsync(filePath, result);

                // Generate thumbnail
                await GenerateThumbnailAsync(filePath, thumbnailPath);

                _logger.LogInformation("Successfully uploaded photo {FileName} for album {AlbumId}", fileName, albumId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photo for album {AlbumId}", albumId);
                throw new UserFriendlyException("Failed to upload photo. Please try again.");
            }
        }

        public async Task DeletePhotoAsync(string filePath, string thumbnailPath)
        {
            try
            {
                var fullFilePath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", filePath.Replace('/', '\\'));
                var fullThumbnailPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", thumbnailPath?.Replace('/', '\\') ?? "");

                // Delete original file
                if (File.Exists(fullFilePath))
                {
                    File.Delete(fullFilePath);
                    _logger.LogInformation("Deleted photo file: {FilePath}", fullFilePath);
                }

                // Delete thumbnail
                if (!string.IsNullOrEmpty(thumbnailPath) && File.Exists(fullThumbnailPath))
                {
                    File.Delete(fullThumbnailPath);
                    _logger.LogInformation("Deleted thumbnail file: {ThumbnailPath}", fullThumbnailPath);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting photo files: {FilePath}, {ThumbnailPath}", filePath, thumbnailPath);
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
                using var image = Image.FromStream(stream);
                // If we can create an Image object, it's a valid image file
                return await Task.FromResult(true);
            }
            catch
            {
                return false;
            }
        }

        private async Task ExtractImageMetadataAsync(string filePath, FileUploadResult result)
        {
            try
            {
                // Extract basic image dimensions
                using var image = Image.FromFile(filePath);
                result.Width = image.Width;
                result.Height = image.Height;

                // Extract EXIF data
                var directories = ImageMetadataReader.ReadMetadata(filePath);

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

                    // Camera model
                    var ifd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
                    if (ifd0 != null && ifd0.ContainsTag(ExifDirectoryBase.TagModel))
                    {
                        result.CameraModel = ifd0.GetDescription(ExifDirectoryBase.TagModel);
                    }

                    // F-Number (aperture)
                    if (exifSubIfdDirectory.ContainsTag(ExifDirectoryBase.TagFNumber))
                    {
                        var fnum = exifSubIfdDirectory.GetRational(ExifDirectoryBase.TagFNumber);
                        if (fnum.Denominator != 0)
                        {
                            result.Aperture = (double)fnum.Numerator / fnum.Denominator;
                        }
                    }

                    // Exposure time (shutter speed)
                    if (exifSubIfdDirectory.ContainsTag(ExifDirectoryBase.TagExposureTime))
                    {
                        var exposure = exifSubIfdDirectory.GetRational(ExifDirectoryBase.TagExposureTime);
                        // Format as fractional seconds if < 1s
                        if (exposure.Denominator != 0)
                        {
                            var seconds = (double)exposure.Numerator / exposure.Denominator;
                            result.ShutterSpeed = seconds >= 1 ? $"{seconds:0.#} s" : $"1/{Math.Round(1/seconds):0} s";
                        }
                    }

                    // ISO
                    if (exifSubIfdDirectory.ContainsTag(ExifDirectoryBase.TagIsoEquivalent))
                    {
                        var iso = exifSubIfdDirectory.GetInt32(ExifDirectoryBase.TagIsoEquivalent);
                        result.Iso = iso;
                    }

                    // Focal length
                    if (exifSubIfdDirectory.ContainsTag(ExifDirectoryBase.TagFocalLength))
                    {
                        var fl = exifSubIfdDirectory.GetRational(ExifDirectoryBase.TagFocalLength);
                        if (fl.Denominator != 0)
                        {
                            result.FocalLength = (double)fl.Numerator / fl.Denominator;
                        }
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
                _logger.LogWarning(ex, "Failed to extract EXIF data from {FilePath}", filePath);
                // Don't throw exception, just log the warning and continue
            }
        }

        private async Task GenerateThumbnailAsync(string originalPath, string thumbnailPath)
        {
            try
            {
                using var originalImage = Image.FromFile(originalPath);
                
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

                using var thumbnail = new Bitmap(thumbWidth, thumbHeight);
                using var graphics = Graphics.FromImage(thumbnail);
                
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                graphics.DrawImage(originalImage, 0, 0, thumbWidth, thumbHeight);

                // Save thumbnail with JPEG format and good quality
                var jpegCodec = ImageCodecInfo.GetImageDecoders()
                    .FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
                
                if (jpegCodec != null)
                {
                    var encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 85L);
                    thumbnail.Save(thumbnailPath, jpegCodec, encoderParams);
                }
                else
                {
                    thumbnail.Save(thumbnailPath, ImageFormat.Jpeg);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate thumbnail for {OriginalPath}", originalPath);
                throw new UserFriendlyException("Failed to generate thumbnail");
            }
        }
    }
}