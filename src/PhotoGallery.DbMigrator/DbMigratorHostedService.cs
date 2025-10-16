using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhotoGallery.Data;
using Serilog;
using Volo.Abp;
using Volo.Abp.Data;

namespace PhotoGallery.DbMigrator;

public class DbMigratorHostedService : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IConfiguration _configuration;

    public DbMigratorHostedService(IHostApplicationLifetime hostApplicationLifetime, IConfiguration configuration)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Diagnostic: log the resolved connection string to help troubleshoot null connection issue
        try
        {
            var conn = _configuration.GetSection("ConnectionStrings")["Default"];
            Log.Information("DbMigrator ConnectionStrings:Default = {Default}", string.IsNullOrWhiteSpace(conn) ? "<null/empty>" : conn);
        }
        catch
        {
            // ignore
        }
        using (var application = await AbpApplicationFactory.CreateAsync<PhotoGalleryDbMigratorModule>(options =>
        {
           options.Services.ReplaceConfiguration(_configuration);
           options.UseAutofac();
           options.Services.AddLogging(c => c.AddSerilog());
           options.AddDataMigrationEnvironment();
        }))
        {
            await application.InitializeAsync();

            await application
                .ServiceProvider
                .GetRequiredService<PhotoGalleryDbMigrationService>()
                .MigrateAsync();

            await application.ShutdownAsync();

            _hostApplicationLifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
