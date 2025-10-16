using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace PhotoGallery.Albums
{
    public class Album : AuditedAggregateRoot<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public string? CoverImagePath { get; set; }
        
        // Navigation property
        public virtual ICollection<Photos.Photo> Photos { get; set; } = new List<Photos.Photo>();
        
        protected Album()
        {
            // Required by EF Core
        }
        
        public Album(
            Guid id,
            string name,
            string topic,
            string? description = null,
            int displayOrder = 0,
            string? coverImagePath = null)
            : base(id)
        {
            Name = name;
            Topic = topic;
            Description = description;
            DisplayOrder = displayOrder;
            CoverImagePath = coverImagePath;
            Photos = new List<Photos.Photo>();
        }
        
        public void UpdateDetails(string name, string topic, string? description = null)
        {
            Name = name;
            Topic = topic;
            Description = description;
        }
        
        public void SetCoverImage(string? coverImagePath)
        {
            CoverImagePath = coverImagePath;
        }
        
        public void SetDisplayOrder(int displayOrder)
        {
            DisplayOrder = displayOrder;
        }
    }
}