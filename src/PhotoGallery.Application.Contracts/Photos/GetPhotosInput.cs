using System;
using Volo.Abp.Application.Dtos;

namespace PhotoGallery.Photos
{
    public class GetPhotosInput : PagedAndSortedResultRequestDto
    {
        public Guid? AlbumId { get; set; }
        public string? Location { get; set; }
        public DateTime? DateTakenFrom { get; set; }
        public DateTime? DateTakenTo { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class GetRandomPhotosInput
    {
        public int Count { get; set; } = 6;
        public Guid? ExcludeAlbumId { get; set; }
        public Guid? IncludeAlbumId { get; set; }
    }
}