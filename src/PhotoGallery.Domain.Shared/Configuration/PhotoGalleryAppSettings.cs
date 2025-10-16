namespace PhotoGallery.Configuration
{
    public class FileUploadSettings
    {
        public string BasePath { get; set; } = "uploads";
        public string PhotosPath { get; set; } = "uploads/photos";
        public string ThumbnailsPath { get; set; } = "uploads/thumbnails";
        public long MaxFileSize { get; set; } = 10485760; // 10MB
        public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        public int ThumbnailWidth { get; set; } = 300;
        public int ThumbnailHeight { get; set; } = 300;
        public int ThumbnailQuality { get; set; } = 85;
    }

    public class PhotoGalleryAppSettings
    {
        public FileUploadSettings FileUpload { get; set; } = new();
    }
}