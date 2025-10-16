using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PhotoGallery.Permissions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace PhotoGallery.Albums
{
    /// <summary>
    /// Album management API endpoints
    /// </summary>
    [ApiController]
    [Route("api/albums")]
    [Produces("application/json")]
    public class AlbumController : AbpControllerBase
    {
        private readonly IAlbumAppService _albumAppService;

        public AlbumController(IAlbumAppService albumAppService)
        {
            _albumAppService = albumAppService;
        }

        /// <summary>
        /// Get a paginated list of albums
        /// </summary>
        /// <param name="input">Pagination and sorting parameters</param>
        /// <returns>Paginated list of albums</returns>
        /// <response code="200">Returns the paginated album list</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<AlbumDto>), StatusCodes.Status200OK)]
        public async Task<PagedResultDto<AlbumDto>> GetListAsync([FromQuery] PagedAndSortedResultRequestDto input)
        {
            return await _albumAppService.GetListAsync(input);
        }

        /// <summary>
        /// Get a specific album by ID
        /// </summary>
        /// <param name="id">Album ID</param>
        /// <returns>Album details</returns>
        /// <response code="200">Returns the album</response>
        /// <response code="404">Album not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AlbumDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<AlbumDto> GetAsync(Guid id)
        {
            return await _albumAppService.GetAsync(id);
        }

        /// <summary>
        /// Create a new album
        /// </summary>
        /// <param name="input">Album creation data</param>
        /// <returns>Created album</returns>
        /// <response code="201">Album created successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Insufficient permissions</response>
        [HttpPost]
        [Authorize(PhotoGalleryPermissions.Albums.Create)]
        [ProducesResponseType(typeof(AlbumDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<AlbumDto> CreateAsync([FromBody] CreateAlbumDto input)
        {
            return await _albumAppService.CreateAsync(input);
        }

        /// <summary>
        /// Update an existing album
        /// </summary>
        /// <param name="id">Album ID</param>
        /// <param name="input">Album update data</param>
        /// <returns>Updated album</returns>
        /// <response code="200">Album updated successfully</response>
        /// <response code="400">Invalid input data</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Insufficient permissions</response>
        /// <response code="404">Album not found</response>
        [HttpPut("{id}")]
        [Authorize(PhotoGalleryPermissions.Albums.Edit)]
        [ProducesResponseType(typeof(AlbumDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<AlbumDto> UpdateAsync(Guid id, [FromBody] UpdateAlbumDto input)
        {
            return await _albumAppService.UpdateAsync(id, input);
        }

        /// <summary>
        /// Delete an album
        /// </summary>
        /// <param name="id">Album ID</param>
        /// <response code="204">Album deleted successfully</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Insufficient permissions</response>
        /// <response code="404">Album not found</response>
        [HttpDelete("{id}")]
        [Authorize(PhotoGalleryPermissions.Albums.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task DeleteAsync(Guid id)
        {
            await _albumAppService.DeleteAsync(id);
        }

        /// <summary>
        /// Reorder albums by changing their display order
        /// </summary>
        /// <param name="input">New album order configuration</param>
        /// <response code="200">Albums reordered successfully</response>
        /// <response code="400">Invalid reorder data</response>
        /// <response code="401">Authentication required</response>
        /// <response code="403">Insufficient permissions</response>
        [HttpPost("reorder")]
        [Authorize(PhotoGalleryPermissions.Albums.Edit)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task ReorderAsync([FromBody] ReorderAlbumsDto input)
        {
            await _albumAppService.ReorderAsync(input);
        }

        /// <summary>
        /// Get all unique topics from albums
        /// </summary>
        /// <returns>List of unique topics</returns>
        /// <response code="200">Returns the list of topics</response>
        [HttpGet("topics")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public async Task<List<string>> GetTopicsAsync()
        {
            return await _albumAppService.GetTopicsAsync();
        }

        /// <summary>
        /// Get albums filtered by topic
        /// </summary>
        /// <param name="topic">Topic to filter by</param>
        /// <param name="input">Pagination and sorting parameters</param>
        /// <returns>Paginated list of albums with the specified topic</returns>
        /// <response code="200">Returns the filtered album list</response>
        [HttpGet("by-topic/{topic}")]
        [ProducesResponseType(typeof(PagedResultDto<AlbumDto>), StatusCodes.Status200OK)]
        public async Task<PagedResultDto<AlbumDto>> GetByTopicAsync(string topic, [FromQuery] PagedAndSortedResultRequestDto input)
        {
            return await _albumAppService.GetByTopicAsync(topic, input);
        }
    }
}