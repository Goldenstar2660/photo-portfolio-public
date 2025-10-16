using System;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;
using PhotoGallery.Services;
using PhotoGallery.Application.Services;
using PhotoGallery.Application.Photos;
using Microsoft.Extensions.Configuration;
using PhotoGallery.Application.Configuration;
using Amazon.S3;
using Amazon.Runtime;

namespace PhotoGallery;

[DependsOn(
    typeof(PhotoGalleryDomainModule),
    typeof(PhotoGalleryApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class PhotoGalleryApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        var configuration = context.Services.GetConfiguration();

        // Configure Cloudflare R2 (S3-compatible) client
        var cloudflareConfig = configuration.GetSection("Cloudflare");
        var accessKey = cloudflareConfig["AccessKey"];
        var secretKey = cloudflareConfig["SecretKey"];
        var accountId = cloudflareConfig["AccountId"];

        if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey) && !string.IsNullOrEmpty(accountId))
        {
            services.AddSingleton<IAmazonS3>(provider =>
            {
                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                var config = new AmazonS3Config
                {
                    ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
                    ForcePathStyle = true
                };
                return new AmazonS3Client(credentials, config);
            });
        }

        // Register cache services
        services.AddTransient<ICacheKeyService, CacheKeyService>();

        // Register file upload service - only requires local file system
        services.AddTransient<IFileUploadService, FileUploadService>();
        
        // Register photo storage service - will gracefully handle missing R2 configuration
        services.AddTransient<IPhotoStorageService, PhotoStorageService>();

        // Register CacheSettings as singleton, bound from configuration
        var cacheSettingsSection = context.Services.GetConfiguration().GetSection("PhotoGallery:Cache");
        var cacheSettings = cacheSettingsSection.Get<CacheSettings>() ?? new CacheSettings();
        services.AddSingleton(cacheSettings);

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<PhotoGalleryApplicationModule>();
        });
    }
}
