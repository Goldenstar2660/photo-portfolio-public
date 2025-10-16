using Volo.Abp.Modularity;

namespace PhotoGallery;

[DependsOn(
    typeof(PhotoGalleryApplicationModule),
    typeof(PhotoGalleryDomainTestModule)
)]
public class PhotoGalleryApplicationTestModule : AbpModule
{

}
