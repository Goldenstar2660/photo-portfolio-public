using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace PhotoGallery.Data;

/* This is used if database provider does't define
 * IPhotoGalleryDbSchemaMigrator implementation.
 */
public class NullPhotoGalleryDbSchemaMigrator : IPhotoGalleryDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
