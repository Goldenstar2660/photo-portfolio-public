using System;
using Volo.Abp.Application.Dtos;

namespace PhotoGallery.Photos
{
    public class PhotoDto : AuditedEntityDto<Guid>
    {
        public Guid AlbumId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? ThumbnailPath { get; set; }
        public long FileSize { get; set; }
        public string? Location { get; set; }
        public int DisplayOrder { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public DateTime? DateTaken { get; set; }
        public string? CameraModel { get; set; }
        public double? Aperture { get; set; }
        public string? ShutterSpeed { get; set; }
        public int? Iso { get; set; }
        public double? FocalLength { get; set; }
        
        // Related data
        public string AlbumName { get; set; } = string.Empty;
    }
}