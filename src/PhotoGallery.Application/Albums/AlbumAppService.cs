using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using PhotoGallery.Photos;
using PhotoGallery.Services;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace PhotoGallery.Albums
{
    public class AlbumAppService : PhotoGalleryAppService, IAlbumAppService
    {
        private readonly IRepository<Album, Guid> _albumRepository;
        private readonly IRepository<Photo, Guid> _photoRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly ICacheKeyService _cacheKeyService;
    private readonly PhotoGallery.Application.Configuration.CacheSettings _cacheSettings;

        public AlbumAppService(
            IRepository<Album, Guid> albumRepository,
            IRepository<Photo, Guid> photoRepository,
            IMemoryCache memoryCache,
            ICacheKeyService cacheKeyService,
            PhotoGallery.Application.Configuration.CacheSettings cacheSettings)
        {
            _albumRepository = albumRepository;
            _photoRepository = photoRepository;
            _memoryCache = memoryCache;
            _cacheKeyService = cacheKeyService;
            _cacheSettings = cacheSettings;
        }

        public virtual async Task<PagedResultDto<AlbumDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            // Check if caching is enabled
            if (!_cacheSettings.GetListCacheEnabled)
            {
                return await GetListFromDatabaseAsync(input);
            }

            // Generate cache key
            var cacheKey = _cacheKeyService.GenerateListCacheKey(
                nameof(AlbumAppService), 
                nameof(GetListAsync), 
                input);

            // Try to get from cache
            if (_memoryCache.TryGetValue(cacheKey, out PagedResultDto<AlbumDto>? cachedResult) && cachedResult != null)
            {
                return cachedResult;
            }

            // Get from database
            var result = await GetListFromDatabaseAsync(input);

            // Cache the result
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheSettings.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = _cacheSettings.SlidingExpiration,
                Priority = CacheItemPriority.Normal
            };
            _memoryCache.Set(cacheKey, result, cacheOptions);

            return result;
        }

        private async Task<PagedResultDto<AlbumDto>> GetListFromDatabaseAsync(PagedAndSortedResultRequestDto input)
        {
            var queryable = await _albumRepository.GetQueryableAsync();
            
            var query = queryable
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name);

            var totalCount = await AsyncExecuter.CountAsync(query);

            var albums = await AsyncExecuter.ToListAsync(
                query.PageBy(input.SkipCount, input.MaxResultCount)
            );

            // Get photo counts for all albums in this page
            var albumIds = albums.Select(a => a.Id).ToList();
            var photoQueryable = await _photoRepository.GetQueryableAsync();
            var photoCounts = photoQueryable
                .Where(p => albumIds.Contains(p.AlbumId))
                .GroupBy(p => p.AlbumId)
                .Select(g => new { AlbumId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.AlbumId, x => x.Count);

            var albumDtos = albums.Select(album => new AlbumDto
            {
                Id = album.Id,
                Name = album.Name,
                Topic = album.Topic,
                Description = album.Description,
                DisplayOrder = album.DisplayOrder,
                CoverImagePath = album.CoverImagePath,
                PhotoCount = photoCounts.ContainsKey(album.Id) ? photoCounts[album.Id] : 0,
                CreationTime = album.CreationTime,
                CreatorId = album.CreatorId,
                LastModificationTime = album.LastModificationTime,
                LastModifierId = album.LastModifierId
            }).ToList();

            return new PagedResultDto<AlbumDto>(totalCount, albumDtos);
        }

        /// <summary>
        /// Invalidate all album-related cache entries
        /// </summary>
        private async Task InvalidateAlbumCacheAsync()
        {
            var cacheKeys = await _cacheKeyService.GetInvalidationKeysAsync(nameof(AlbumAppService));
            foreach (var key in cacheKeys)
            {
                _memoryCache.Remove(key);
            }
        }

        public virtual async Task<AlbumDto> GetAsync(Guid id)
        {
            var album = await _albumRepository.GetAsync(id);
            
            // Get photo count for this album
            var photoQueryable = await _photoRepository.GetQueryableAsync();
            var photoCount = await AsyncExecuter.CountAsync(
                photoQueryable.Where(p => p.AlbumId == id)
            );
            
            return new AlbumDto
            {
                Id = album.Id,
                Name = album.Name,
                Topic = album.Topic,
                Description = album.Description,
                DisplayOrder = album.DisplayOrder,
                CoverImagePath = album.CoverImagePath,
                PhotoCount = photoCount,
                CreationTime = album.CreationTime,
                CreatorId = album.CreatorId,
                LastModificationTime = album.LastModificationTime,
                LastModifierId = album.LastModifierId
            };
        }

        public virtual async Task<AlbumDto> CreateAsync(CreateAlbumDto input)
        {
            var album = new Album(
                GuidGenerator.Create(),
                input.Name,
                input.Topic,
                input.Description,
                input.DisplayOrder,
                input.CoverImagePath
            );

            album = await _albumRepository.InsertAsync(album, autoSave: true);

            // Invalidate cache after creating new album
            await InvalidateAlbumCacheAsync();

            // New albums start with 0 photos
            return new AlbumDto
            {
                Id = album.Id,
                Name = album.Name,
                Topic = album.Topic,
                Description = album.Description,
                DisplayOrder = album.DisplayOrder,
                CoverImagePath = album.CoverImagePath,
                PhotoCount = 0,
                CreationTime = album.CreationTime,
                CreatorId = album.CreatorId,
                LastModificationTime = album.LastModificationTime,
                LastModifierId = album.LastModifierId
            };
        }

        public virtual async Task<AlbumDto> UpdateAsync(Guid id, UpdateAlbumDto input)
        {
            var album = await _albumRepository.GetAsync(id);

            album.UpdateDetails(input.Name, input.Topic, input.Description);
            album.SetDisplayOrder(input.DisplayOrder);

            if (!string.IsNullOrEmpty(input.CoverImagePath))
            {
                album.SetCoverImage(input.CoverImagePath);
            }

            album = await _albumRepository.UpdateAsync(album, autoSave: true);

            // Invalidate cache after updating album
            await InvalidateAlbumCacheAsync();

            // Get current photo count for this album
            var photoQueryable = await _photoRepository.GetQueryableAsync();
            var photoCount = await AsyncExecuter.CountAsync(
                photoQueryable.Where(p => p.AlbumId == id)
            );

            return new AlbumDto
            {
                Id = album.Id,
                Name = album.Name,
                Topic = album.Topic,
                Description = album.Description,
                DisplayOrder = album.DisplayOrder,
                CoverImagePath = album.CoverImagePath,
                PhotoCount = photoCount,
                CreationTime = album.CreationTime,
                CreatorId = album.CreatorId,
                LastModificationTime = album.LastModificationTime,
                LastModifierId = album.LastModifierId
            };
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            await _albumRepository.DeleteAsync(id);
            
            // Invalidate cache after deleting album
            await InvalidateAlbumCacheAsync();
        }

        public virtual async Task ReorderAsync(ReorderAlbumsDto input)
        {
            var albumIds = input.AlbumOrders.Select(x => x.Id).ToList();
            var albums = await _albumRepository.GetListAsync(x => albumIds.Contains(x.Id));

            foreach (var album in albums)
            {
                var orderInfo = input.AlbumOrders.First(x => x.Id == album.Id);
                album.SetDisplayOrder(orderInfo.DisplayOrder);
            }

            await _albumRepository.UpdateManyAsync(albums, autoSave: true);
            
            // Invalidate cache after reordering albums
            await InvalidateAlbumCacheAsync();
        }

        public virtual async Task<List<string>> GetTopicsAsync()
        {
            var queryable = await _albumRepository.GetQueryableAsync();
            
            return await AsyncExecuter.ToListAsync(
                queryable
                    .Select(x => x.Topic)
                    .Distinct()
                    .OrderBy(x => x)
            );
        }

        public virtual async Task<PagedResultDto<AlbumDto>> GetByTopicAsync(string topic, PagedAndSortedResultRequestDto input)
        {
            var queryable = await _albumRepository.GetQueryableAsync();
            
            var query = queryable
                .Where(x => x.Topic == topic)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name);

            var totalCount = await AsyncExecuter.CountAsync(query);

            var albums = await AsyncExecuter.ToListAsync(
                query.PageBy(input.SkipCount, input.MaxResultCount)
            );

            var albumDtos = albums.Select(album => new AlbumDto
            {
                Id = album.Id,
                Name = album.Name,
                Topic = album.Topic,
                Description = album.Description,
                DisplayOrder = album.DisplayOrder,
                CoverImagePath = album.CoverImagePath,
                PhotoCount = album.Photos?.Count ?? 0,
                CreationTime = album.CreationTime,
                CreatorId = album.CreatorId,
                LastModificationTime = album.LastModificationTime,
                LastModifierId = album.LastModifierId
            }).ToList();

            return new PagedResultDto<AlbumDto>(totalCount, albumDtos);
        }
       
    }
}