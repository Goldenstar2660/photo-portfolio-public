using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhotoGallery.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace PhotoGallery;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpAutoMapperModule),
    typeof(PhotoGalleryApplicationModule),
    typeof(PhotoGalleryEntityFrameworkCoreModule)
)]
public class PhotoGalleryPublicWebModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        var configuration = context.Services.GetConfiguration();

        // Configure AutoMapper
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<PhotoGalleryPublicWebModule>();
        });

        // Add Razor Pages
        services.AddRazorPages();

        // Add Memory Cache
        services.AddMemoryCache();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        // Configure the HTTP request pipeline
        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();

        app.UseConfiguredEndpoints(endpoints =>
        {
            endpoints.MapRazorPages();
        });
    }
}
