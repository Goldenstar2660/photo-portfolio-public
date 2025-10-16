using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace PhotoGallery.Photos
{
    public class Photo : AuditedEntity<Guid>
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
        public double? Aperture { get; set; } // f-number
        public string? ShutterSpeed { get; set; } // e.g., 1/125
        public int? Iso { get; set; }
        public double? FocalLength { get; set; } // in mm
        
        // Navigation property
        public virtual Albums.Album? Album { get; set; }
        
        protected Photo()
        {
            // Required by EF Core
        }
        
        public Photo(
            Guid id,
            Guid albumId,
            string fileName,
            string filePath,
            long fileSize,
            int displayOrder = 0,
            string? location = null,
            string? thumbnailPath = null,
            int? width = null,
            int? height = null,
            DateTime? dateTaken = null,
            string? cameraModel = null,
            double? aperture = null,
            string? shutterSpeed = null,
            int? iso = null,
            double? focalLength = null)
            : base(id)
        {
            AlbumId = albumId;
            FileName = fileName;
            FilePath = filePath;
            FileSize = fileSize;
            DisplayOrder = displayOrder;
            Location = location;
            ThumbnailPath = thumbnailPath;
            Width = width;
            Height = height;
            DateTaken = dateTaken;
            CameraModel = cameraModel;
            Aperture = aperture;
            ShutterSpeed = shutterSpeed;
            Iso = iso;
            FocalLength = focalLength;
        }
        
        public void UpdateLocation(string? location)
        {
            Location = location;
        }
        
        public void SetDisplayOrder(int displayOrder)
        {
            DisplayOrder = displayOrder;
        }
        
        public void SetThumbnail(string? thumbnailPath)
        {
            ThumbnailPath = thumbnailPath;
        }
        
        public void SetDimensions(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}