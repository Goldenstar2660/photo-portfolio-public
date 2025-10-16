using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PhotoGallery.Data;
using Volo.Abp.DependencyInjection;

namespace PhotoGallery.EntityFrameworkCore;

public class EntityFrameworkCorePhotoGalleryDbSchemaMigrator
    : IPhotoGalleryDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCorePhotoGalleryDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the PhotoGalleryDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<PhotoGalleryDbContext>()
            .Database
            .MigrateAsync();
    }
}
