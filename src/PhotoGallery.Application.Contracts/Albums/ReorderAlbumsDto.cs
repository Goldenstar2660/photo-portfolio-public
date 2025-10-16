using System;
using System.Collections.Generic;

namespace PhotoGallery.Albums
{
    public class ReorderAlbumsDto
    {
        public List<AlbumOrderDto> AlbumOrders { get; set; } = new();
    }

    public class AlbumOrderDto
    {
        public Guid Id { get; set; }
        public int DisplayOrder { get; set; }
    }
}