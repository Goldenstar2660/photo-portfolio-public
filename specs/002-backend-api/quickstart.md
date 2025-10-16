# PhotoGallery Backend API - Quickstart Guide

## Overview

This guide provides step-by-step instructions for implementing the PhotoGallery Backend API using ABP Framework's Domain-Driven Design (DDD) layered architecture.

## Prerequisites

### Required Software
- **.NET 9 SDK** or later
- **Visual Studio 2022** or **JetBrains Rider** or **VS Code**
- **SQL Server** (LocalDB for development, SQL Server for production)
- **ABP CLI** (for code generation and migrations)

### Development Environment Setup
```bash
# Install ABP CLI globally
dotnet tool install -g Volo.Abp.Cli

# Verify installation
abp --version
```

### Database Requirements
- **SQL Server LocalDB** (included with Visual Studio)
- **Connection String** configured in appsettings.json
- **Migration permissions** for database schema changes

## Implementation Steps

### Step 1: Domain Layer Implementation

#### 1.1 Create Album Entity
**File**: `src/PhotoGallery.Domain/Albums/Album.cs`

```bash
# Create Albums directory
mkdir src/PhotoGallery.Domain/Albums
```

Create the Album entity with audit fields and navigation properties as specified in [data-model.md](./data-model.md).

#### 1.2 Create Photo Entity  
**File**: `src/PhotoGallery.Domain/Photos/Photo.cs`

```bash
# Create Photos directory
mkdir src/PhotoGallery.Domain/Photos
```

Create the Photo entity with all required properties and business methods.

#### 1.3 Update Data Seeder
**File**: `src/PhotoGallery.Domain/Data/PhotoGalleryDataSeederContributor.cs`

Add sample albums and photos for development and testing.

### Step 2: Application Contracts Layer

#### 2.1 Create Album DTOs and Interfaces
**Directory**: `src/PhotoGallery.Application.Contracts/Albums/`

```bash
# Create Albums contracts directory
mkdir src/PhotoGallery.Application.Contracts/Albums
```

Create the following files:
- `IAlbumAppService.cs` - Service interface
- `AlbumDto.cs` - Data transfer objects
- `CreateAlbumDto.cs`, `UpdateAlbumDto.cs` - Input DTOs

#### 2.2 Create Photo DTOs and Interfaces
**Directory**: `src/PhotoGallery.Application.Contracts/Photos/`

```bash
# Create Photos contracts directory
mkdir src/PhotoGallery.Application.Contracts/Photos
```

Create the photo-related DTOs and service interface.

#### 2.3 Add Permission Definitions
**File**: `src/PhotoGallery.Application.Contracts/Permissions/PhotoGalleryPermissions.cs`

Add constants for album and photo permissions.

### Step 3: Entity Framework Configuration

#### 3.1 Create Entity Configurations
**Directory**: `src/PhotoGallery.EntityFrameworkCore/EntityConfigurations/`

```bash
# Create EntityConfigurations directory
mkdir src/PhotoGallery.EntityFrameworkCore/EntityConfigurations
```

Create configuration files:
- `AlbumConfiguration.cs` - Album entity mapping
- `PhotoConfiguration.cs` - Photo entity mapping

#### 3.2 Update DbContext
**File**: `src/PhotoGallery.EntityFrameworkCore/PhotoGalleryDbContext.cs`

Add DbSets and apply entity configurations:

```csharp
public DbSet<Album> Albums { get; set; }
public DbSet<Photo> Photos { get; set; }

protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);
    
    builder.ApplyConfiguration(new AlbumConfiguration());
    builder.ApplyConfiguration(new PhotoConfiguration());
}
```

#### 3.3 Create Database Migration
```bash
# Navigate to DbMigrator project
cd src/PhotoGallery.DbMigrator

# Add new migration
dotnet ef migrations add AddAlbumsAndPhotos -p ../PhotoGallery.EntityFrameworkCore

# Update database
dotnet ef database update -p ../PhotoGallery.EntityFrameworkCore
```

### Step 4: Application Layer Implementation

#### 4.1 Install FluentValidation
```bash
# Navigate to Application project
cd src/PhotoGallery.Application

# Add FluentValidation package
dotnet add package FluentValidation.AspNetCore
```

#### 4.2 Create Application Services
**Directory**: `src/PhotoGallery.Application/Albums/`

Create `AlbumAppService.cs` implementing `IAlbumAppService` with all CRUD operations.

**Directory**: `src/PhotoGallery.Application/Photos/`

Create `PhotoAppService.cs` implementing `IPhotoAppService` with file upload capabilities.

#### 4.3 Create Validation Classes
Create FluentValidation validators for all DTOs in the respective directories.

#### 4.4 Add File Upload Service
**File**: `src/PhotoGallery.Application/Services/FileUploadService.cs`

Implement file processing, thumbnail generation, and EXIF data extraction.

#### 4.5 Update AutoMapper Profile
**File**: `src/PhotoGallery.Application/PhotoGalleryApplicationAutoMapperProfile.cs`

Add mappings between entities and DTOs as specified in the data model.

#### 4.6 Register Services in Module
**File**: `src/PhotoGallery.Application/PhotoGalleryApplicationModule.cs`

```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    Configure<AbpAutoMapperOptions>(options =>
    {
        options.AddMaps<PhotoGalleryApplicationModule>();
    });
    
    // Register FluentValidation
    context.Services.AddValidatorsFromAssembly(typeof(PhotoGalleryApplicationModule).Assembly);
    
    // Register file upload service
    context.Services.AddTransient<IFileUploadService, FileUploadService>();
}
```

### Step 5: HTTP API Layer

#### 5.1 Create API Controllers
**Directory**: `src/PhotoGallery.HttpApi/Controllers/`

Create REST controllers:
- `AlbumController.cs` - Album CRUD endpoints
- `PhotoController.cs` - Photo CRUD and upload endpoints

#### 5.2 Configure File Upload Settings
**File**: `src/PhotoGallery.HttpApi.Host/appsettings.json`

```json
{
  "FileUpload": {
    "MaxFileSize": 10485760,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif"],
    "UploadPath": "wwwroot/uploads/photos"
  }
}
```

#### 5.3 Create Upload Directory Structure
```bash
# Create upload directories
mkdir -p src/PhotoGallery.HttpApi.Host/wwwroot/uploads/photos
```

### Step 6: Configure Permissions

#### 6.1 Update Permission Provider
**File**: `src/PhotoGallery.Application.Contracts/Permissions/PhotoGalleryPermissionDefinitionProvider.cs`

```csharp
public override void Define(IPermissionDefinitionContext context)
{
    var photoGalleryGroup = context.AddGroup(PhotoGalleryPermissions.GroupName);
    
    var albums = photoGalleryGroup.AddPermission(PhotoGalleryPermissions.Albums.Default);
    albums.AddChild(PhotoGalleryPermissions.Albums.Create);
    albums.AddChild(PhotoGalleryPermissions.Albums.Edit);
    albums.AddChild(PhotoGalleryPermissions.Albums.Delete);
    
    var photos = photoGalleryGroup.AddPermission(PhotoGalleryPermissions.Photos.Default);
    photos.AddChild(PhotoGalleryPermissions.Photos.Create);
    photos.AddChild(PhotoGalleryPermissions.Photos.Edit);
    photos.AddChild(PhotoGalleryPermissions.Photos.Delete);
}
```

### Step 7: Testing Setup

#### 7.1 Create Unit Test Project Structure
```bash
# Navigate to test directory
cd test

# Create test directories if they don't exist
mkdir -p PhotoGallery.Application.Tests/Albums
mkdir -p PhotoGallery.Application.Tests/Photos
mkdir -p PhotoGallery.HttpApi.Tests/Controllers
```

#### 7.2 Write Application Service Tests
Create comprehensive unit tests for both `AlbumAppService` and `PhotoAppService`.

#### 7.3 Write API Controller Tests
Create integration tests for all API endpoints.

### Step 8: Final Configuration

#### 8.1 Update Module Dependencies
Ensure all modules properly reference the new entities and services.

#### 8.2 Configure CORS (if needed)
**File**: `src/PhotoGallery.HttpApi.Host/Program.cs`

```csharp
app.UseCors(options =>
{
    options.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
});
```

#### 8.3 Add Swagger Documentation
ABP automatically configures Swagger. Enhance with additional documentation:

```csharp
services.AddAbpSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "PhotoGallery API", 
        Version = "v1" 
    });
});
```

## Running the Application

### Development Environment

#### 1. Start SQL Server LocalDB
```bash
# Start LocalDB instance
sqllocaldb start MSSQLLocalDB
```

#### 2. Run Database Migration
```bash
cd src/PhotoGallery.DbMigrator
dotnet run
```

#### 3. Start the API
```bash
cd src/PhotoGallery.HttpApi.Host
dotnet run
```

#### 4. Access Swagger UI
Navigate to: `https://localhost:44367/swagger` (port may vary)

### Testing the API

#### Sample API Calls

**Create Album:**
```bash
curl -X POST "https://localhost:44367/api/app/album" \
     -H "Content-Type: application/json" \
     -d '{
       "name": "Nature Photography",
       "topic": "Nature", 
       "description": "Beautiful landscapes and wildlife"
     }'
```

**Upload Photo:**
```bash
curl -X POST "https://localhost:44367/api/app/photo" \
     -H "Content-Type: multipart/form-data" \
     -F "albumId=ALBUM_ID_HERE" \
     -F "file=@/path/to/image.jpg" \
     -F "caption=Beautiful sunset"
```

## Project Structure After Implementation

```
src/PhotoGallery.Domain/
├── Albums/
│   └── Album.cs
├── Photos/
│   └── Photo.cs
└── Data/
    └── PhotoGalleryDataSeederContributor.cs

src/PhotoGallery.Application.Contracts/
├── Albums/
│   ├── IAlbumAppService.cs
│   ├── AlbumDto.cs
│   ├── CreateAlbumDto.cs
│   └── UpdateAlbumDto.cs
├── Photos/
│   ├── IPhotoAppService.cs
│   ├── PhotoDto.cs
│   └── CreatePhotoDto.cs
└── Permissions/
    ├── PhotoGalleryPermissions.cs
    └── PhotoGalleryPermissionDefinitionProvider.cs

src/PhotoGallery.Application/
├── Albums/
│   ├── AlbumAppService.cs
│   └── AlbumDtoValidator.cs
├── Photos/
│   ├── PhotoAppService.cs
│   └── PhotoDtoValidator.cs
├── Services/
│   └── FileUploadService.cs
└── PhotoGalleryApplicationAutoMapperProfile.cs

src/PhotoGallery.EntityFrameworkCore/
├── EntityConfigurations/
│   ├── AlbumConfiguration.cs
│   └── PhotoConfiguration.cs
└── Migrations/
    └── [Timestamp]_AddAlbumsAndPhotos.cs

src/PhotoGallery.HttpApi/
└── Controllers/
    ├── AlbumController.cs
    └── PhotoController.cs

src/PhotoGallery.HttpApi.Host/
└── wwwroot/uploads/photos/
    └── [Dynamic album directories]

tests/
├── PhotoGallery.Application.Tests/
│   ├── Albums/
│   │   └── AlbumAppServiceTests.cs
│   └── Photos/
│       └── PhotoAppServiceTests.cs
└── PhotoGallery.HttpApi.Tests/
    └── Controllers/
        ├── AlbumControllerTests.cs
        └── PhotoControllerTests.cs
```

## Configuration Files

### appsettings.json
```json
{
  "ConnectionStrings": {
    "Default": "Server=(LocalDb)\\MSSQLLocalDB;Database=PhotoGallery;Trusted_Connection=true"
  },
  "FileUpload": {
    "MaxFileSize": 10485760,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif"],
    "UploadPath": "wwwroot/uploads/photos",
    "ThumbnailSize": 300
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

## Performance Optimizations

### Database Indexing
- Indexes are automatically created based on entity configurations
- Additional indexes can be added via migrations if needed

### File Handling
- Implement async file operations throughout
- Use proper disposal patterns for streams
- Consider implementing file compression for large uploads

### Caching (Future Enhancement)
- ABP provides built-in caching support
- Consider caching frequently accessed album lists
- Implement cache invalidation strategies

## Security Considerations

### File Upload Security
- Validate file content, not just extensions
- Implement path traversal prevention
- Consider virus scanning integration
- Limit file sizes and types

### Authorization
- Use ABP's permission system consistently
- Implement proper authorization checks in all service methods
- Consider role-based access control for different user types

### Data Validation
- Use FluentValidation for comprehensive input validation
- Implement business rule validation in domain entities
- Sanitize user inputs to prevent XSS attacks

## Deployment Considerations

### Production Database
- Update connection strings for production SQL Server
- Ensure proper database permissions
- Plan for database backup and recovery

### File Storage
- Consider cloud storage (Azure Blob Storage) for production
- Implement CDN for static file delivery
- Plan for file backup and disaster recovery

### Environment Configuration
- Use different configuration files for different environments
- Implement proper logging for production monitoring
- Configure appropriate CORS policies

## Troubleshooting

### Common Issues

**Migration Errors:**
- Ensure SQL Server is running
- Check connection string configuration
- Verify EF Core packages are up to date

**File Upload Failures:**
- Check file size limits in both application and server configuration
- Verify upload directory permissions
- Ensure supported file types are correctly configured

**Permission Errors:**
- Verify permission definitions are properly configured
- Check that users have appropriate roles assigned
- Ensure authorization attributes are applied to service methods

### Debugging Tips

1. **Enable detailed logging** in development
2. **Use ABP's built-in logging** for service operations
3. **Test API endpoints individually** using Swagger UI
4. **Monitor database queries** using EF Core logging
5. **Verify file system permissions** for upload directories

---

**Implementation Status**: Ready for Development  
**Estimated Duration**: 8-10 days  
**Dependencies**: ABP Framework, Entity Framework Core, FluentValidation  
**Next Steps**: Begin with Domain Layer implementation