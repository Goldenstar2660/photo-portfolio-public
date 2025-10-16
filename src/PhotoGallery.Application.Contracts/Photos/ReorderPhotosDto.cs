using System;
using System.Collections.Generic;

namespace PhotoGallery.Photos
{
    public class ReorderPhotosDto
    {
        public List<PhotoOrderDto> PhotoOrders { get; set; } = new();
    }

    public class PhotoOrderDto
    {
        public Guid Id { get; set; }
        public int DisplayOrder { get; set; }
    }
}