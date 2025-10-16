using PhotoGallery.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Amazon.S3;
using Microsoft.Extensions.Configuration;

namespace PhotoGallery.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(PhotoGalleryEntityFrameworkCoreModule),
    typeof(PhotoGalleryApplicationContractsModule)
)]
public class PhotoGalleryDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        
        // Configure AWS S3 client for Cloudflare R2
        context.Services.AddSingleton<IAmazonS3>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var accountId = config["Cloudflare:AccountId"];
            return new AmazonS3Client(
                config["Cloudflare:AccessKey"],
                config["Cloudflare:SecretKey"],
                new AmazonS3Config
                {
                    ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
                    ForcePathStyle = true
                }
            );
        });
    }
}
