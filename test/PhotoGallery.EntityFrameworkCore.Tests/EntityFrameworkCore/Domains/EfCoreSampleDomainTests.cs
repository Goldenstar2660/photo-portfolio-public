using PhotoGallery.Samples;
using Xunit;

namespace PhotoGallery.EntityFrameworkCore.Domains;

[Collection(PhotoGalleryTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<PhotoGalleryEntityFrameworkCoreTestModule>
{

}
