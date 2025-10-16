using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Volo.Abp.Application.Services;

namespace PhotoGallery.Services
{
    public interface IPhotoStorageService : IApplicationService
    {
        /// <summary>
        /// Uploads a photo file to Cloudflare R2 and generates thumbnail
        /// </summary>
        /// <param name="file">The uploaded file</param>
        /// <param name="albumId">The album ID for organizing files</param>
        /// <returns>Upload result containing file paths and metadata</returns>
        Task<FileUploadResult> UploadPhotoAsync(IFormFile file, Guid albumId);

        /// <summary>
        /// Deletes photo file and its thumbnail from Cloudflare R2
        /// </summary>
        /// <param name="filePath">Original file path/key</param>
        /// <param name="thumbnailPath">Thumbnail file path/key</param>
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

        /// <summary>
        /// Gets the CDN URL for a file stored in Cloudflare R2
        /// </summary>
        /// <param name="objectKey">The object key/path in R2</param>
        /// <returns>CDN URL for the file</returns>
        string GetCdnUrl(string objectKey);
    }
}