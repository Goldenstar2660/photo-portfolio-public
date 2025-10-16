using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using Volo.Abp;
using Volo.Abp.Testing;

namespace PhotoGallery
{
    public abstract class PhotoGalleryHttpApiTestBase : AbpIntegratedTest<PhotoGalleryHttpApiTestModule>
    {
        protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
        {
            options.UseAutofac();
        }

        protected WebApplicationFactory<Program> GetFactory()
        {
            return new PhotoGalleryWebApplicationFactory();
        }
    }
}