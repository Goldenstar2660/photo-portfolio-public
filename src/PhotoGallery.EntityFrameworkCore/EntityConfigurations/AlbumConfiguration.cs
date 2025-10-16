using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhotoGallery.Albums;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace PhotoGallery.EntityFrameworkCore.EntityConfigurations
{
    public class AlbumConfiguration : IEntityTypeConfiguration<Album>
    {
        public void Configure(EntityTypeBuilder<Album> builder)
        {
            builder.ToTable(PhotoGalleryConsts.DbTablePrefix + "Albums", PhotoGalleryConsts.DbSchema);
            
            builder.ConfigureByConvention();
            
            // Properties
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(x => x.Topic)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(x => x.Description)
                .HasMaxLength(500);
                
            builder.Property(x => x.CoverImagePath)
                .HasMaxLength(500);
            
            // Indexes
            builder.HasIndex(x => x.Name).IsUnique();
            builder.HasIndex(x => x.Topic);
            builder.HasIndex(x => x.DisplayOrder);
            
            // Relationships
            builder.HasMany(x => x.Photos)
                .WithOne(x => x.Album)
                .HasForeignKey(x => x.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}