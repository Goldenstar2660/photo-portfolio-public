using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace PhotoGallery.Photos
{
    public interface IPhotoAppService : IApplicationService
    {
        Task<PagedResultDto<PhotoDto>> GetListAsync(GetPhotosInput input);
        Task<List<PhotoDto>> GetRandomPhotosAsync(GetRandomPhotosInput input);
        Task<PhotoDto> GetAsync(Guid id);
        Task<PhotoDto> CreateAsync(CreatePhotoDto input);
        Task<PhotoDto> UpdateAsync(Guid id, UpdatePhotoDto input);
        Task DeleteAsync(Guid id);
        Task ReorderAsync(ReorderPhotosDto input);
        Task<PagedResultDto<PhotoDto>> GetByAlbumAsync(Guid albumId, PagedAndSortedResultRequestDto input);
        Task<List<string>> GetLocationsAsync(Guid? albumId = null);
    }
}