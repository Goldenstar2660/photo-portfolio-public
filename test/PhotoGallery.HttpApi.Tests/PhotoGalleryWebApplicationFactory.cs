using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.AspNetCore.TestBase;

namespace PhotoGallery
{
    public class PhotoGalleryWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var context = services.GetRequiredService<IAbpApplicationWithExternalServiceProvider>();
                context.ServiceProvider = services.BuildServiceProvider();
            });
        }
    }
}