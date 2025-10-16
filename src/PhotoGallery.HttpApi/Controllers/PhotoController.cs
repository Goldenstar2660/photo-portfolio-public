using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PhotoGallery.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace PhotoGallery.Photos
{
    /// <summary>
    /// Photo management API endpoints
    /// </summary>
    [ApiController]
    [Route("api/photos")]
    [Produces("application/json")]
    public class PhotoController : AbpControllerBase
    {
        private readonly IPhotoAppService _photoAppService;

        public PhotoController(IPhotoAppService photoAppService)
        {
            _photoAppService = photoAppService;
        }

        /// <summary>
        /// Get a paginated and filtered list of photos
        /// </summary>
        /// <param name="input">Filter, pagination and sorting parameters</param>
        /// <returns>Paginated list of photos</returns>
        /// <response code="200">Returns the filtered photo list</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<PhotoDto>), StatusCodes.Status200OK)]
        public async Task<PagedResultDto<PhotoDto>> GetListAsync([FromQuery] GetPhotosInput input)
        {
            return await _photoAppService.GetListAsync(input);
        }

        /// <summary>
        /// Get random photos for display
        /// </summary>
        /// <param name="input">Random selection parameters</param>
        /// <returns>List of random photos</returns>
        /// <response code="200">Returns random photos</response>
        [HttpGet("random")]
        [ProducesResponseType(typeof(List<PhotoDto>), StatusCodes.Status200OK)]
        public async Task<List<PhotoDto>> GetRandomPhotosAsync([FromQuery] GetRandomPhotosInput input)
        {
            return await _photoAppService.GetRandomPhotosAsync(input);
        }

        /// <summary>
        /// Get a specific photo by ID
        /// </summary>
        /// <param name="id">Photo ID</param>
        /// <returns>Photo details</returns>
        /// <response code="200">Returns the photo</response>
        /// <response code="404">Photo not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PhotoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<PhotoDto> GetAsync(Guid id)
        {
            return await _photoAppService.GetAsync(id);
        }

        /// <summary>
        /// Upload a new photo to an album
        /// </summary>
        /// <param name="input">Photo upload data including file</param>
        /// <returns>Created photo</returns>
        /// <response code="201">Photo uploaded successfully</response>
        /// <response code="400">Invalid file or input data</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Insufficient permissions</response>
        [HttpPost]
        [Authorize(PhotoGalleryPermissions.Photos.Create)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(PhotoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<PhotoDto> CreateAsync([FromForm] CreatePhotoDto input)
        {
            return await _photoAppService.CreateAsync(input);
        }

        /// <summary>
        /// Update photo metadata (caption, location, display order)
        /// </summary>
        /// <param name="id">Photo ID</param>
        /// <param name="input">Photo update data</param>
        /// <returns>Updated photo</returns>
        /// <response code="200">Photo updated successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Insufficient permissions</response>
        /// <response code="404">Photo not found</response>
        [HttpPut("{id}")]
        [Authorize(PhotoGalleryPermissions.Photos.Edit)]
        [ProducesResponseType(typeof(PhotoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<PhotoDto> UpdateAsync(Guid id, [FromBody] UpdatePhotoDto input)
        {
            return await _photoAppService.UpdateAsync(id, input);
        }

        /// <summary>
        /// Delete a photo and its files
        /// </summary>
        /// <param name="id">Photo ID</param>
        /// <response code="204">Photo deleted successfully</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Insufficient permissions</response>
        /// <response code="404">Photo not found</response>
        [HttpDelete("{id}")]
        [Authorize(PhotoGalleryPermissions.Photos.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task DeleteAsync(Guid id)
        {
            await _photoAppService.DeleteAsync(id);
        }

        /// <summary>
        /// Reorder photos within an album
        /// </summary>
        /// <param name="input">New photo order configuration</param>
        /// <response code="200">Photos reordered successfully</response>
        /// <response code="400">Invalid reorder data</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Insufficient permissions</response>
        [HttpPost("reorder")]
        [Authorize(PhotoGalleryPermissions.Photos.Edit)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task ReorderAsync([FromBody] ReorderPhotosDto input)
        {
            await _photoAppService.ReorderAsync(input);
        }

        /// <summary>
        /// Get photos from a specific album
        /// </summary>
        /// <param name="albumId">Album ID</param>
        /// <param name="input">Pagination and sorting parameters</param>
        /// <returns>Paginated list of photos in the album</returns>
        /// <response code="200">Returns photos from the album</response>
        /// <response code="404">Album not found</response>
        [HttpGet("by-album/{albumId}")]
        [ProducesResponseType(typeof(PagedResultDto<PhotoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<PagedResultDto<PhotoDto>> GetByAlbumAsync(Guid albumId, [FromQuery] PagedAndSortedResultRequestDto input)
        {
            return await _photoAppService.GetByAlbumAsync(albumId, input);
        }

        /// <summary>
        /// Get all unique locations from photos
        /// </summary>
        /// <param name="albumId">Optional album ID to filter locations</param>
        /// <returns>List of unique locations</returns>
        /// <response code="200">Returns the list of locations</response>
        [HttpGet("locations")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public async Task<List<string>> GetLocationsAsync([FromQuery] Guid? albumId = null)
        {
            return await _photoAppService.GetLocationsAsync(albumId);
        }
    }
}