using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace PhotoGallery.Albums
{
    public interface IAlbumAppService : IApplicationService
    {
        Task<PagedResultDto<AlbumDto>> GetListAsync(PagedAndSortedResultRequestDto input);
        Task<AlbumDto> GetAsync(Guid id);
        Task<AlbumDto> CreateAsync(CreateAlbumDto input);
        Task<AlbumDto> UpdateAsync(Guid id, UpdateAlbumDto input);
        Task DeleteAsync(Guid id);
        Task ReorderAsync(ReorderAlbumsDto input);
        Task<List<string>> GetTopicsAsync();
        Task<PagedResultDto<AlbumDto>> GetByTopicAsync(string topic, PagedAndSortedResultRequestDto input);
    }
}