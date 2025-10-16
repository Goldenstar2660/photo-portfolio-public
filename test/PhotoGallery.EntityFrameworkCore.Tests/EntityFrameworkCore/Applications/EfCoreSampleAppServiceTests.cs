using PhotoGallery.Samples;
using Xunit;

namespace PhotoGallery.EntityFrameworkCore.Applications;

[Collection(PhotoGalleryTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<PhotoGalleryEntityFrameworkCoreTestModule>
{

}
