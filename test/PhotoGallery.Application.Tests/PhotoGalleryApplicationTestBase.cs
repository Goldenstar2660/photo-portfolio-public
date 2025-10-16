using Volo.Abp.Modularity;

namespace PhotoGallery;

public abstract class PhotoGalleryApplicationTestBase<TStartupModule> : PhotoGalleryTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
