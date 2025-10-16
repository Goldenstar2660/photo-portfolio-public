using System;
using Microsoft.AspNetCore.Http;

namespace PhotoGallery.Photos
{
    public class CreatePhotoDto
    {
        public Guid AlbumId { get; set; }
        public IFormFile File { get; set; } = default!;
        public string Location { get; set; } = "";
        public int DisplayOrder { get; set; }
    }
}