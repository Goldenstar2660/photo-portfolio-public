using System;
using Volo.Abp.Application.Dtos;

namespace PhotoGallery.Albums
{
    public class AlbumDto : AuditedEntityDto<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public string? CoverImagePath { get; set; }
        public int PhotoCount { get; set; }
    }
}