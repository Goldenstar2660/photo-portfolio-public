using Xunit;

namespace PhotoGallery.EntityFrameworkCore;

[CollectionDefinition(PhotoGalleryTestConsts.CollectionDefinitionName)]
public class PhotoGalleryEntityFrameworkCoreCollection : ICollectionFixture<PhotoGalleryEntityFrameworkCoreFixture>
{

}
