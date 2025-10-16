using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhotoGallery.Photos;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace PhotoGallery.EntityFrameworkCore.EntityConfigurations
{
    public class PhotoConfiguration : IEntityTypeConfiguration<Photo>
    {
        public void Configure(EntityTypeBuilder<Photo> builder)
        {
            builder.ToTable(PhotoGalleryConsts.DbTablePrefix + "Photos", PhotoGalleryConsts.DbSchema);
            
            builder.ConfigureByConvention();
            
            // Properties
            builder.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(x => x.FilePath)
                .IsRequired()
                .HasMaxLength(500);
                
            builder.Property(x => x.ThumbnailPath)
                .HasMaxLength(500);
                
            builder.Property(x => x.Location)
                .HasMaxLength(100);
            builder.Property(x => x.CameraModel)
                .HasMaxLength(100);
            builder.Property(x => x.ShutterSpeed)
                .HasMaxLength(50);
            // Numeric fields Aperture, Iso, FocalLength keep defaults
            
            // Indexes
            builder.HasIndex(x => x.AlbumId);
            builder.HasIndex(x => x.DisplayOrder);
            builder.HasIndex(x => x.Location);
            builder.HasIndex(x => x.DateTaken);
            builder.HasIndex(x => new { x.AlbumId, x.DisplayOrder });
        }
    }
}