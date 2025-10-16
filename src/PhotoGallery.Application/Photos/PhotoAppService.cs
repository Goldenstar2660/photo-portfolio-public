using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using System.Linq.Dynamic.Core;
using PhotoGallery.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Volo.Abp;

namespace PhotoGallery.Photos
{
    public class PhotoAppService : PhotoGalleryAppService, IPhotoAppService
    {
        private readonly IRepository<Photo, Guid> _photoRepository;
        private readonly IRepository<Albums.Album, Guid> _albumRepository;
        private readonly IPhotoStorageService? _photoStorageService;
        private readonly IMemoryCache _memoryCache;
        private readonly ICacheKeyService _cacheKeyService;
        private readonly PhotoGallery.Application.Configuration.CacheSettings _cacheSettings;

        public PhotoAppService(
            IRepository<Photo, Guid> photoRepository,
            IRepository<Albums.Album, Guid> albumRepository,
            IMemoryCache memoryCache,
            ICacheKeyService cacheKeyService,
            PhotoGallery.Application.Configuration.CacheSettings cacheSettings,
            IPhotoStorageService? photoStorageService = null)
        {
            _photoRepository = photoRepository;
            _albumRepository = albumRepository;
            _photoStorageService = photoStorageService;
            _memoryCache = memoryCache;
            _cacheKeyService = cacheKeyService;
            _cacheSettings = cacheSettings;
        }

        public virtual async Task<PagedResultDto<PhotoDto>> GetListAsync(GetPhotosInput input)
        {
            // Check if caching is enabled
            if (!_cacheSettings.GetListCacheEnabled)
            {
                return await GetListFromDatabaseAsync(input);
            }

            // Generate cache key
            var cacheKey = _cacheKeyService.GenerateListCacheKey(
                nameof(PhotoAppService), 
                nameof(GetListAsync), 
                input);

            // Try to get from cache
            if (_memoryCache.TryGetValue(cacheKey, out PagedResultDto<PhotoDto>? cachedResult) && cachedResult != null)
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

        private async Task<PagedResultDto<PhotoDto>> GetListFromDatabaseAsync(GetPhotosInput input)
        {
            var queryable = await _photoRepository.GetQueryableAsync();
            
            var query = queryable.AsQueryable();

            // Apply filters
            if (input.AlbumId.HasValue)
            {
                query = query.Where(x => x.AlbumId == input.AlbumId.Value);
            }

            if (!string.IsNullOrEmpty(input.Location))
            {
                query = query.Where(x => x.Location != null && x.Location.Contains(input.Location));
            }

            if (input.DateTakenFrom.HasValue)
            {
                query = query.Where(x => x.DateTaken >= input.DateTakenFrom.Value);
            }

            if (input.DateTakenTo.HasValue)
            {
                query = query.Where(x => x.DateTaken <= input.DateTakenTo.Value);
            }

            if (!string.IsNullOrEmpty(input.SearchTerm))
            {
                query = query.Where(x => 
                    (x.Location != null && x.Location.Contains(input.SearchTerm)) ||
                    x.FileName.Contains(input.SearchTerm));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(input.Sorting))
            {
                query = query.OrderBy(input.Sorting);
            }
            else
            {
                query = query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.FileName);
            }

            var totalCount = await AsyncExecuter.CountAsync(query);

            var photos = await AsyncExecuter.ToListAsync(
                query.PageBy(input.SkipCount, input.MaxResultCount)
            );

            // Get album names for the photos
            var albumIds = photos.Select(p => p.AlbumId).Distinct().ToList();
            var albums = await _albumRepository.GetListAsync(a => albumIds.Contains(a.Id));
            var albumDict = albums.ToDictionary(a => a.Id, a => a.Name);

            var photoDtos = photos.Select(photo => new PhotoDto
            {
                Id = photo.Id,
                AlbumId = photo.AlbumId,
                FileName = photo.FileName,
                FilePath = photo.FilePath,
                ThumbnailPath = photo.ThumbnailPath,
                FileSize = photo.FileSize,
                Location = photo.Location,
                DisplayOrder = photo.DisplayOrder,
                Width = photo.Width,
                Height = photo.Height,
                DateTaken = photo.DateTaken,
                CameraModel = photo.CameraModel,
                Aperture = photo.Aperture,
                ShutterSpeed = photo.ShutterSpeed,
                Iso = photo.Iso,
                FocalLength = photo.FocalLength,
                CreationTime = photo.CreationTime,
                CreatorId = photo.CreatorId,
                LastModificationTime = photo.LastModificationTime,
                LastModifierId = photo.LastModifierId,
                AlbumName = albumDict.GetValueOrDefault(photo.AlbumId, "Unknown Album")
            }).ToList();

            return new PagedResultDto<PhotoDto>(totalCount, photoDtos);
        }

        /// <summary>
        /// Invalidate all photo-related cache entries
        /// </summary>
        private async Task InvalidatePhotoCacheAsync()
        {
            var cacheKeys = await _cacheKeyService.GetInvalidationKeysAsync(nameof(PhotoAppService));
            foreach (var key in cacheKeys)
            {
                _memoryCache.Remove(key);
            }
        }

        public virtual async Task<List<PhotoDto>> GetRandomPhotosAsync(GetRandomPhotosInput input)
        {
            var queryable = await _photoRepository.GetQueryableAsync();
            
            var query = queryable.AsQueryable();

            if (input.IncludeAlbumId.HasValue)
            {
                query = query.Where(x => x.AlbumId == input.IncludeAlbumId.Value);
            }

            if (input.ExcludeAlbumId.HasValue)
            {
                query = query.Where(x => x.AlbumId != input.ExcludeAlbumId.Value);
            }

            // Get random photos using GUID ordering (simple random approach)
            var photos = await AsyncExecuter.ToListAsync(
                query.OrderBy(x => Guid.NewGuid()).Take(input.Count)
            );

            // Get album names for the photos
            var albumIds = photos.Select(p => p.AlbumId).Distinct().ToList();
            var albums = await _albumRepository.GetListAsync(a => albumIds.Contains(a.Id));
            var albumDict = albums.ToDictionary(a => a.Id, a => a.Name);

            return photos.Select(photo => new PhotoDto
            {
                Id = photo.Id,
                AlbumId = photo.AlbumId,
                FileName = photo.FileName,
                FilePath = photo.FilePath,
                ThumbnailPath = photo.ThumbnailPath,
                FileSize = photo.FileSize,
                Location = photo.Location,
                DisplayOrder = photo.DisplayOrder,
                Width = photo.Width,
                Height = photo.Height,
                DateTaken = photo.DateTaken,
                CameraModel = photo.CameraModel,
                Aperture = photo.Aperture,
                ShutterSpeed = photo.ShutterSpeed,
                Iso = photo.Iso,
                FocalLength = photo.FocalLength,
                CreationTime = photo.CreationTime,
                CreatorId = photo.CreatorId,
                LastModificationTime = photo.LastModificationTime,
                LastModifierId = photo.LastModifierId,
                AlbumName = albumDict.GetValueOrDefault(photo.AlbumId, "Unknown Album")
            }).ToList();
        }

        public virtual async Task<PhotoDto> GetAsync(Guid id)
        {
            var photo = await _photoRepository.GetAsync(id);
            var album = await _albumRepository.GetAsync(photo.AlbumId);

            return new PhotoDto
            {
                Id = photo.Id,
                AlbumId = photo.AlbumId,
                FileName = photo.FileName,
                FilePath = photo.FilePath,
                ThumbnailPath = photo.ThumbnailPath,
                FileSize = photo.FileSize,
                Location = photo.Location,
                DisplayOrder = photo.DisplayOrder,
                Width = photo.Width,
                Height = photo.Height,
                DateTaken = photo.DateTaken,
                CameraModel = photo.CameraModel,
                Aperture = photo.Aperture,
                ShutterSpeed = photo.ShutterSpeed,
                Iso = photo.Iso,
                FocalLength = photo.FocalLength,
                CreationTime = photo.CreationTime,
                CreatorId = photo.CreatorId,
                LastModificationTime = photo.LastModificationTime,
                LastModifierId = photo.LastModifierId,
                AlbumName = album.Name
            };
        }

        public virtual async Task<PhotoDto> CreateAsync(CreatePhotoDto input)
        {
            // Validate album exists
            var album = await _albumRepository.GetAsync(input.AlbumId);

            // Upload file and get metadata using Cloudflare storage
            if (_photoStorageService == null)
            {
                throw new UserFriendlyException("Photo storage service is not configured. Cannot upload photos.");
            }
            
            var uploadResult = await _photoStorageService.UploadPhotoAsync(input.File, input.AlbumId);

            // Create photo entity
            var photo = new Photo(
                GuidGenerator.Create(),
                input.AlbumId,
                uploadResult.FileName,
                uploadResult.FilePath,
                uploadResult.FileSize,
                input.DisplayOrder,
                !string.IsNullOrEmpty(input.Location) ? input.Location : uploadResult.Location,
                uploadResult.ThumbnailPath,
                uploadResult.Width,
                uploadResult.Height,
                uploadResult.DateTaken,
                uploadResult.CameraModel,
                uploadResult.Aperture,
                uploadResult.ShutterSpeed,
                uploadResult.Iso,
                uploadResult.FocalLength
            );

            // Save to database
            photo = await _photoRepository.InsertAsync(photo, autoSave: true);

            // Invalidate cache after creating new photo
            await InvalidatePhotoCacheAsync();

            return new PhotoDto
            {
                Id = photo.Id,
                AlbumId = photo.AlbumId,
                FileName = photo.FileName,
                FilePath = photo.FilePath,
                ThumbnailPath = photo.ThumbnailPath,
                FileSize = photo.FileSize,
                Location = photo.Location,
                DisplayOrder = photo.DisplayOrder,
                Width = photo.Width,
                Height = photo.Height,
                DateTaken = photo.DateTaken,
                CameraModel = photo.CameraModel,
                Aperture = photo.Aperture,
                ShutterSpeed = photo.ShutterSpeed,
                Iso = photo.Iso,
                FocalLength = photo.FocalLength,
                CreationTime = photo.CreationTime,
                CreatorId = photo.CreatorId,
                LastModificationTime = photo.LastModificationTime,
                LastModifierId = photo.LastModifierId,
                AlbumName = album.Name
            };
        }

        public virtual async Task<PhotoDto> UpdateAsync(Guid id, UpdatePhotoDto input)
        {
            var photo = await _photoRepository.GetAsync(id);

            photo.UpdateLocation(input.Location);
            photo.SetDisplayOrder(input.DisplayOrder);

            photo = await _photoRepository.UpdateAsync(photo, autoSave: true);

            // Invalidate cache after updating photo
            await InvalidatePhotoCacheAsync();

            var album = await _albumRepository.GetAsync(photo.AlbumId);

            return new PhotoDto
            {
                Id = photo.Id,
                AlbumId = photo.AlbumId,
                FileName = photo.FileName,
                FilePath = photo.FilePath,
                ThumbnailPath = photo.ThumbnailPath,
                FileSize = photo.FileSize,
                Location = photo.Location,
                DisplayOrder = photo.DisplayOrder,
                Width = photo.Width,
                Height = photo.Height,
                DateTaken = photo.DateTaken,
                CreationTime = photo.CreationTime,
                CreatorId = photo.CreatorId,
                LastModificationTime = photo.LastModificationTime,
                LastModifierId = photo.LastModifierId,
                AlbumName = album.Name
            };
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            // Get photo details before deletion
            var photo = await _photoRepository.GetAsync(id);
            
            // Delete from database first
            await _photoRepository.DeleteAsync(id);
            
            // Invalidate cache after deleting photo
            await InvalidatePhotoCacheAsync();
            
            // Delete physical files from Cloudflare R2 (non-blocking - don't fail if file deletion fails)
            try
            {
                if (_photoStorageService != null)
                {
                    await _photoStorageService.DeletePhotoAsync(photo.FilePath, photo.ThumbnailPath ?? string.Empty);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to delete physical files from Cloudflare R2 for photo {PhotoId}: {FilePath}, {ThumbnailPath}", 
                    id, photo.FilePath, photo.ThumbnailPath);
            }
        }

        public virtual async Task ReorderAsync(ReorderPhotosDto input)
        {
            var photoIds = input.PhotoOrders.Select(x => x.Id).ToList();
            var photos = await _photoRepository.GetListAsync(x => photoIds.Contains(x.Id));

            foreach (var photo in photos)
            {
                var orderInfo = input.PhotoOrders.First(x => x.Id == photo.Id);
                photo.SetDisplayOrder(orderInfo.DisplayOrder);
            }

            await _photoRepository.UpdateManyAsync(photos, autoSave: true);
            
            // Invalidate cache after reordering photos
            await InvalidatePhotoCacheAsync();
        }

        public virtual async Task<PagedResultDto<PhotoDto>> GetByAlbumAsync(Guid albumId, PagedAndSortedResultRequestDto input)
        {
            var queryable = await _photoRepository.GetQueryableAsync();
            
            var query = queryable
                .Where(x => x.AlbumId == albumId)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.FileName);

            var totalCount = await AsyncExecuter.CountAsync(query);

            var photos = await AsyncExecuter.ToListAsync(
                query.PageBy(input.SkipCount, input.MaxResultCount)
            );

            var album = await _albumRepository.GetAsync(albumId);

            var photoDtos = photos.Select(photo => new PhotoDto
            {
                Id = photo.Id,
                AlbumId = photo.AlbumId,
                FileName = photo.FileName,
                FilePath = photo.FilePath,
                ThumbnailPath = photo.ThumbnailPath,
                FileSize = photo.FileSize,
                Location = photo.Location,
                DisplayOrder = photo.DisplayOrder,
                Width = photo.Width,
                Height = photo.Height,
                DateTaken = photo.DateTaken,
                CameraModel = photo.CameraModel,
                Aperture = photo.Aperture,
                ShutterSpeed = photo.ShutterSpeed,
                Iso = photo.Iso,
                FocalLength = photo.FocalLength,
                CreationTime = photo.CreationTime,
                CreatorId = photo.CreatorId,
                LastModificationTime = photo.LastModificationTime,
                LastModifierId = photo.LastModifierId,
                AlbumName = album.Name
            }).ToList();

            return new PagedResultDto<PhotoDto>(totalCount, photoDtos);
        }

        public virtual async Task<List<string>> GetLocationsAsync(Guid? albumId = null)
        {
            var queryable = await _photoRepository.GetQueryableAsync();
            
            var query = queryable.AsQueryable();

            if (albumId.HasValue)
            {
                query = query.Where(x => x.AlbumId == albumId.Value);
            }

            return await AsyncExecuter.ToListAsync(
                query
                    .Where(x => !string.IsNullOrEmpty(x.Location))
                    .Select(x => x.Location!)
                    .Distinct()
                    .OrderBy(x => x)
            );
        }
    }
}