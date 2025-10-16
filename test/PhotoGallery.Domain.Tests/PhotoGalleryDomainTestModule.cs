using Volo.Abp.Modularity;

namespace PhotoGallery;

[DependsOn(
    typeof(PhotoGalleryDomainModule),
    typeof(PhotoGalleryTestBaseModule)
)]
public class PhotoGalleryDomainTestModule : AbpModule
{

}
