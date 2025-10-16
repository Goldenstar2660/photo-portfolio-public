using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Volo.Abp.Application.Services;

namespace PhotoGallery.Services
{
    public interface IFileUploadService : IApplicationService
    {
        /// <summary>
        /// Uploads a photo file and generates thumbnail
        /// </summary>
        /// <param name="file">The uploaded file</param>
        /// <param name="albumId">The album ID for organizing files</param>
        /// <returns>Upload result containing file paths and metadata</returns>
        Task<FileUploadResult> UploadPhotoAsync(IFormFile file, Guid albumId);

        /// <summary>
        /// Deletes photo file and its thumbnail
        /// </summary>
        /// <param name="filePath">Original file path</param>
        /// <param name="thumbnailPath">Thumbnail file path</param>
        Task DeletePhotoAsync(string filePath, string thumbnailPath);

        /// <summary>
        /// Generates secure filename to prevent conflicts and security issues
        /// </summary>
        /// <param name="originalFileName">Original filename from upload</param>
        /// <param name="albumId">Album ID for organization</param>
        /// <returns>Secure filename</returns>
        string GenerateSecureFileName(string originalFileName, Guid albumId);

        /// <summary>
        /// Validates uploaded file for security and format requirements
        /// </summary>
        /// <param name="file">File to validate</param>
        /// <returns>True if file is valid</returns>
        Task<bool> ValidateFileAsync(IFormFile file);
    }

    public class FileUploadResult
    {
        public string FilePath { get; set; } = string.Empty;
        public string ThumbnailPath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public DateTime? DateTaken { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? CameraModel { get; set; }
        public double? Aperture { get; set; }
        public string? ShutterSpeed { get; set; }
        public int? Iso { get; set; }
        public double? FocalLength { get; set; }
    }
}