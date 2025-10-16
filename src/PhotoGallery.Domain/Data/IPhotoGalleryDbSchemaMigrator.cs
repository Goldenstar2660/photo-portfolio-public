using System.Threading.Tasks;

namespace PhotoGallery.Data;

public interface IPhotoGalleryDbSchemaMigrator
{
    Task MigrateAsync();
}
