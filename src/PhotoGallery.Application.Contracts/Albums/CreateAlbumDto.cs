namespace PhotoGallery.Albums
{
    public class CreateAlbumDto
    {
        public string Name { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public string? CoverImagePath { get; set; }
    }
}