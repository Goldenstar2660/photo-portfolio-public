using Volo.Abp.Modularity;

namespace PhotoGallery;

/* Inherit from this class for your domain layer tests. */
public abstract class PhotoGalleryDomainTestBase<TStartupModule> : PhotoGalleryTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
