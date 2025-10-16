using Microsoft.Extensions.Localization;
using PhotoGallery.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace PhotoGallery;

[Dependency(ReplaceServices = true)]
public class PhotoGalleryBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<PhotoGalleryResource> _localizer;

    public PhotoGalleryBrandingProvider(IStringLocalizer<PhotoGalleryResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
