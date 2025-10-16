using PhotoGallery.Localization;
using Volo.Abp.Application.Services;

namespace PhotoGallery;

/* Inherit your application services from this class.
 */
public abstract class PhotoGalleryAppService : ApplicationService
{
    protected PhotoGalleryAppService()
    {
        LocalizationResource = typeof(PhotoGalleryResource);
    }
}
