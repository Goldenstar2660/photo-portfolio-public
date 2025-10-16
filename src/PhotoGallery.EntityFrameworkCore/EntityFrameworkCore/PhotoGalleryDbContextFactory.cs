using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PhotoGallery.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class PhotoGalleryDbContextFactory : IDesignTimeDbContextFactory<PhotoGalleryDbContext>
{
    public PhotoGalleryDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        PhotoGalleryEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<PhotoGalleryDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new PhotoGalleryDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../PhotoGallery.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
