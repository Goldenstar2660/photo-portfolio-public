# PhotoGallery Public Web - Quickstart Guide

## Overview

This guide provides step-by-step instructions for setting up, developing, and deploying the PhotoGallery Public Web application - a Razor Pages frontend that displays photo galleries to public users.

## Prerequisites

### Required Software
- **.NET 9 SDK** or later
- **Visual Studio 2022** or **JetBrains Rider** or **VS Code**
- **Git** for version control
- **Node.js** (optional, for asset processing)

### Backend Dependencies
- **PhotoGallery Backend API** must be running
- **Database** with sample album and photo data
- **File storage** with uploaded photos accessible

### Development Environment
- **Windows 10/11**, **macOS**, or **Linux**
- **IIS Express** or **Kestrel** for local hosting
- **Chrome/Firefox** for testing (responsive design)

## Project Setup

### 1. Create the Project

```bash
# Navigate to the src directory
cd src

# Create new Razor Pages project with .NET 9
dotnet new razor -n PhotoGallery.PublicWeb --framework net9.0
cd PhotoGallery.PublicWeb

# Add required NuGet packages
dotnet add package Microsoft.Extensions.Http.Polly
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add package Microsoft.AspNetCore.Diagnostics.HealthChecks
```

### 2. Update Project File

```xml
<!-- PhotoGallery.PublicWeb.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>PhotoGallery.PublicWeb</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="13.0.1" />
    <PackageReference Include="Microsoft.AspNetCore.Diagnostics.HealthChecks" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <!-- Direct project references for optimal performance -->
    <ProjectReference Include="..\PhotoGallery.Application\PhotoGallery.Application.csproj" />
    <ProjectReference Include="..\PhotoGallery.EntityFrameworkCore\PhotoGallery.EntityFrameworkCore.csproj" />
  </ItemGroup>

</Project>
```

### 3. Add to Solution

```bash
# Navigate back to solution root
cd ../..

# Add project to solution
dotnet sln add src/PhotoGallery.PublicWeb/PhotoGallery.PublicWeb.csproj
```

## Configuration

### 1. Application Settings

```json
// appsettings.json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=PhotoGallery;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "PhotoGallery.PublicWeb": "Debug"
    }
  },
  "AllowedHosts": "*",
  "Caching": {
    "DefaultExpiration": "00:15:00",
    "NavigationExpiration": "01:00:00",
    "RandomPhotosExpiration": "00:05:00"
  },
  "Gallery": {
    "PageSize": 20,
    "MaxPageSize": 50,
    "RandomPhotoCount": 6,
    "FeaturedPhotoCount": 3,
    "SiteName": "My Photo Gallery",
    "OwnerName": "Your Name"
  }
}
```

```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  },
  "PhotoGalleryApi": {
    "BaseUrl": "https://localhost:44304/api"
  }
}
```

### 2. Program.cs Configuration

```csharp
// Program.cs for .NET 9 with Direct Service Integration
using PhotoGallery;
using PhotoGallery.PublicWeb.Services;
using PhotoGallery.PublicWeb.Models;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages
builder.Services.AddRazorPages();

// Add ABP modules for direct service access
builder.Services.AddApplication<PhotoGalleryApplicationModule>();
builder.Services.AddApplication<PhotoGalleryEntityFrameworkCoreModule>();

// Configure Entity Framework
builder.Services.AddAbpDbContext<PhotoGalleryDbContext>(options =>
{
    options.AddDefaultRepositories(includeAllEntities: true);
});

// Database connection
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddAbpDbContext<PhotoGalleryDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// Memory caching
builder.Services.AddMemoryCache();

// AutoMapper
builder.Services.AddAutoMapper(typeof(PublicWebMappingProfile));

// Application services (direct service access)
builder.Services.AddScoped<IGalleryService, GalleryService>();
builder.Services.AddScoped<IHomePageService, HomePageService>();
builder.Services.AddScoped<IAlbumPageService, AlbumPageService>();
builder.Services.AddScoped<INavigationService, NavigationService>();

// Health checks for database
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PhotoGalleryDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapHealthChecks("/health");

app.Run();

// Helper methods
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30));
}
```

## Bootstrap Template Integration

### 1. Copy Template Assets

```bash
# Create wwwroot directories
mkdir -p wwwroot/css
mkdir -p wwwroot/js
mkdir -p wwwroot/img
mkdir -p wwwroot/lib

# Copy Bootstrap template files
cp specs/003-public-site/public-site-template/css/* wwwroot/css/
cp specs/003-public-site/public-site-template/js/* wwwroot/js/
cp specs/003-public-site/public-site-template/img/* wwwroot/img/
cp -r specs/003-public-site/public-site-template/lib/* wwwroot/lib/
```

### 2. Update Template References

Update CSS and JS references in the template files to use ASP.NET Core conventions:
- Change relative paths to use `~/` prefix
- Update image sources to point to `/img/` directory
- Ensure Bootstrap and FontAwesome CDN links are current

## Development Workflow

### 1. Start Backend API

```bash
# Start the backend API first
cd src/PhotoGallery.HttpApi.Host
dotnet run
```

Verify API is running at `https://localhost:44304/swagger`

### 2. Start Public Web

```bash
# In a new terminal, start the public web project
cd src/PhotoGallery.PublicWeb
dotnet run
```

The application will be available at `https://localhost:5001`

### 3. Development Tools

**Visual Studio:**
- Use multiple startup projects (HttpApi.Host + PublicWeb)
- Enable hot reload for faster development
- Use Browser Link for CSS changes

**VS Code:**
- Install C# Dev Kit extension
- Use `dotnet watch run` for hot reload
- Configure launch.json for debugging

**Rider:**
- Create compound run configuration
- Use hot reload and edit-and-continue
- Enable CSS/JS file watching

## Testing Strategy

### 1. Unit Tests

```bash
# Create test project
cd test
dotnet new xunit -n PhotoGallery.PublicWeb.Tests
cd PhotoGallery.PublicWeb.Tests

# Add test packages
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Moq
dotnet add package AutoFixture

# Add project reference
dotnet add reference ../../src/PhotoGallery.PublicWeb/PhotoGallery.PublicWeb.csproj
```

### 2. Integration Tests

Create tests for:
- API service integration
- Page rendering with real data
- Error handling scenarios
- Caching behavior

### 3. End-to-End Tests

```bash
# Add Selenium for E2E testing
dotnet add package Selenium.WebDriver
dotnet add package Selenium.WebDriver.ChromeDriver
```

Test critical user journeys:
- Homepage loads with random photos
- Album navigation works
- Photo viewing and filtering
- Responsive design on mobile

## Deployment

### 1. Production Configuration

```json
// appsettings.Production.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "PhotoGallery.PublicWeb": "Information"
    }
  },
  "PhotoGalleryApi": {
    "BaseUrl": "https://api.yourdomain.com/api"
  },
  "Gallery": {
    "SiteName": "Professional Photo Gallery",
    "OwnerName": "Your Professional Name"
  }
}
```

### 2. IIS Deployment

```bash
# Publish the application
dotnet publish -c Release -o ./publish

# Copy publish folder to IIS wwwroot
# Configure IIS application pool for .NET 9
# Set environment variables in web.config
```

### 3. Docker Deployment

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/PhotoGallery.PublicWeb/PhotoGallery.PublicWeb.csproj", "src/PhotoGallery.PublicWeb/"]
COPY ["src/PhotoGallery.Application.Contracts/PhotoGallery.Application.Contracts.csproj", "src/PhotoGallery.Application.Contracts/"]
RUN dotnet restore "src/PhotoGallery.PublicWeb/PhotoGallery.PublicWeb.csproj"
COPY . .
WORKDIR "/src/src/PhotoGallery.PublicWeb"
RUN dotnet build "PhotoGallery.PublicWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PhotoGallery.PublicWeb.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PhotoGallery.PublicWeb.dll"]
```

```bash
# Build and run Docker container
docker build -t photogallery-publicweb .
docker run -p 8080:80 photogallery-publicweb
```

## Monitoring and Debugging

### 1. Health Checks

Monitor application health at `/health` endpoint:
- API connectivity status
- Memory usage
- Response times

### 2. Logging

Configure structured logging:
- Request/response logging
- API call tracking
- Error reporting
- Performance metrics

### 3. Application Insights (Optional)

```bash
# Add Application Insights
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

Track:
- Page views and user interactions
- API response times
- Error rates and exceptions
- Custom events for photo views

## Troubleshooting

### Common Issues

**API Connection Errors:**
- Verify backend API is running
- Check firewall and network connectivity
- Review CORS configuration
- Validate API endpoint URLs

**Template Assets Not Loading:**
- Verify wwwroot file structure
- Check static file middleware configuration
- Validate CSS/JS file paths
- Clear browser cache

**Performance Issues:**
- Review caching configuration
- Check image optimization
- Monitor API response times
- Validate pagination settings

**Responsive Design Problems:**
- Test on multiple devices/browsers
- Validate Bootstrap grid usage
- Check viewport meta tag
- Review CSS media queries

### Debug Commands

```bash
# Check project dependencies
dotnet list package

# Verify project builds
dotnet build --verbosity detailed

# Run with detailed logging
dotnet run --environment Development

# Check health status
curl https://localhost:5001/health
```

## Next Steps

1. **Customize Template**: Adapt the Bootstrap template to match your branding
2. **Add Features**: Implement search, filtering, and social sharing
3. **Optimize Performance**: Add CDN, image optimization, and caching
4. **Security**: Implement rate limiting and security headers
5. **Analytics**: Add user tracking and photo view statistics
6. **SEO**: Implement structured data and meta tags
7. **Accessibility**: Ensure WCAG 2.1 AA compliance

## Support and Resources

- **Documentation**: See `/specs/003-public-site/` for detailed specifications
- **API Reference**: Backend API documentation at `/swagger`
- **Template License**: Check Bootstrap template license terms
- **Community**: ASP.NET Core documentation and community forums