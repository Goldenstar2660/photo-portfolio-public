using Volo.Abp.Modularity;

namespace PhotoGallery
{
    [DependsOn(
        typeof(PhotoGalleryApplicationTestModule),
        typeof(PhotoGalleryHttpApiModule)
    )]
    public class PhotoGalleryHttpApiTestModule : AbpModule
    {
    }
}