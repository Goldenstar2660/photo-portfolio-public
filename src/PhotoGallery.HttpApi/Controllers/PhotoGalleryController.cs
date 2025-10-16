using PhotoGallery.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace PhotoGallery.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class PhotoGalleryController : AbpControllerBase
{
    protected PhotoGalleryController()
    {
        LocalizationResource = typeof(PhotoGalleryResource);
    }
}
