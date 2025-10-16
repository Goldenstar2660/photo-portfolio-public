# Data Model Design

## Overview

This document defines the data structures for the PhotoGallery Backend API, including domain entities, DTOs, and service interfaces. The design follows ABP Framework's Domain-Driven Design (DDD) patterns with clean separation between domain logic and data transfer concerns.

## Domain Entities

### Album Entity

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace PhotoGallery.Albums
{
    public class Album : AuditedAggregateRoot<Guid>
    {
        public string Name { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public string CoverImagePath { get; set; }
        
        // Navigation property
        public virtual ICollection<Photo> Photos { get; set; }
        
        protected Album()
        {
            // Required by EF Core
        }
        
        public Album(
            Guid id,
            string name,
            string topic,
            string description = null,
            int displayOrder = 0,
            string coverImagePath = null)
            : base(id)
        {
            Name = name;
            Topic = topic;
            Description = description;
            DisplayOrder = displayOrder;
            CoverImagePath = coverImagePath;
            Photos = new List<Photo>();
        }
        
        public void UpdateDetails(string name, string topic, string description = null)
        {
            Name = name;
            Topic = topic;
            Description = description;
        }
        
        public void SetCoverImage(string coverImagePath)
        {
            CoverImagePath = coverImagePath;
        }
        
        public void SetDisplayOrder(int displayOrder)
        {
            DisplayOrder = displayOrder;
        }
    }
}
```

### Photo Entity

```csharp
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace PhotoGallery.Photos
{
    public class Photo : AuditedEntity<Guid>
    {
        public Guid AlbumId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string ThumbnailPath { get; set; }
        public long FileSize { get; set; }
        public string Caption { get; set; }
        public string Location { get; set; }
        public int DisplayOrder { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public DateTime? DateTaken { get; set; }
        
        // Navigation property
        public virtual Album Album { get; set; }
        
        protected Photo()
        {
            // Required by EF Core
        }
        
        public Photo(
            Guid id,
            Guid albumId,
            string fileName,
            string filePath,
            long fileSize,
            int displayOrder = 0,
            string caption = null,
            string location = null,
            string thumbnailPath = null,
            int? width = null,
            int? height = null,
            DateTime? dateTaken = null)
            : base(id)
        {
            AlbumId = albumId;
            FileName = fileName;
            FilePath = filePath;
            FileSize = fileSize;
            DisplayOrder = displayOrder;
            Caption = caption;
            Location = location;
            ThumbnailPath = thumbnailPath;
            Width = width;
            Height = height;
            DateTaken = dateTaken;
        }
        
        public void UpdateCaption(string caption)
        {
            Caption = caption;
        }
        
        public void UpdateLocation(string location)
        {
            Location = location;
        }
        
        public void SetDisplayOrder(int displayOrder)
        {
            DisplayOrder = displayOrder;
        }
        
        public void SetThumbnail(string thumbnailPath)
        {
            ThumbnailPath = thumbnailPath;
        }
        
        public void SetDimensions(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
```

## Data Transfer Objects (DTOs)

### Album DTOs

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace PhotoGallery.Albums
{
    public class AlbumDto : AuditedEntityDto<Guid>
    {
        public string Name { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public string CoverImagePath { get; set; }
        public int PhotoCount { get; set; }
    }
    
    public class CreateAlbumDto
    {
        public string Name { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
    }
    
    public class UpdateAlbumDto
    {
        public string Name { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
    }
    
    public class ReorderAlbumsDto
    {
        public Guid[] AlbumIds { get; set; }
    }
    
    public class AlbumLookupDto : EntityDto<Guid>
    {
        public string Name { get; set; }
        public string Topic { get; set; }
    }
}
```

### Photo DTOs

```csharp
using System;
using Microsoft.AspNetCore.Http;
using Volo.Abp.Application.Dtos;

namespace PhotoGallery.Photos
{
    public class PhotoDto : EntityDto<Guid>
    {
        public Guid AlbumId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string ThumbnailPath { get; set; }
        public long FileSize { get; set; }
        public string Caption { get; set; }
        public string Location { get; set; }
        public int DisplayOrder { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public DateTime? DateTaken { get; set; }
        public DateTime CreationTime { get; set; }
        
        // Related data
        public string AlbumName { get; set; }
    }
    
    public class CreatePhotoDto
    {
        public Guid AlbumId { get; set; }
        public IFormFile File { get; set; }
        public string Caption { get; set; }
        public string Location { get; set; }
        public int DisplayOrder { get; set; }
    }
    
    public class UpdatePhotoDto
    {
        public string Caption { get; set; }
        public string Location { get; set; }
    }
    
    public class ReorderPhotosDto
    {
        public Guid[] PhotoIds { get; set; }
    }
    
    public class GetPhotosInput : PagedAndSortedResultRequestDto
    {
        public Guid? AlbumId { get; set; }
        public string Location { get; set; }
        public DateTime? DateTakenFrom { get; set; }
        public DateTime? DateTakenTo { get; set; }
    }
    
    public class GetRandomPhotosInput
    {
        public int Count { get; set; } = 6;
        public Guid? ExcludeAlbumId { get; set; }
    }
}
```

## Service Interfaces

### IAlbumAppService

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace PhotoGallery.Albums
{
    public interface IAlbumAppService : IApplicationService
    {
        Task<PagedResultDto<AlbumDto>> GetListAsync(PagedAndSortedResultRequestDto input);
        Task<AlbumDto> GetAsync(Guid id);
        Task<AlbumDto> CreateAsync(CreateAlbumDto input);
        Task<AlbumDto> UpdateAsync(Guid id, UpdateAlbumDto input);
        Task DeleteAsync(Guid id);
        Task ReorderAsync(ReorderAlbumsDto input);
        Task<List<string>> GetTopicsAsync();
        Task<PagedResultDto<AlbumDto>> GetByTopicAsync(string topic, PagedAndSortedResultRequestDto input);
        Task<List<AlbumLookupDto>> GetLookupAsync();
        Task SetCoverImageAsync(Guid id, Guid photoId);
    }
}
```

### IPhotoAppService

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace PhotoGallery.Photos
{
    public interface IPhotoAppService : IApplicationService
    {
        Task<PagedResultDto<PhotoDto>> GetListAsync(GetPhotosInput input);
        Task<List<PhotoDto>> GetRandomPhotosAsync(GetRandomPhotosInput input);
        Task<PhotoDto> GetAsync(Guid id);
        Task<PhotoDto> CreateAsync(CreatePhotoDto input);
        Task<PhotoDto> UpdateAsync(Guid id, UpdatePhotoDto input);
        Task DeleteAsync(Guid id);
        Task ReorderAsync(ReorderPhotosDto input);
        Task<PagedResultDto<PhotoDto>> GetByAlbumAsync(Guid albumId, PagedAndSortedResultRequestDto input);
        Task<List<string>> GetLocationsAsync(Guid? albumId = null);
    }
}
```

## Entity Framework Configuration

### AlbumConfiguration

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhotoGallery.Albums;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace PhotoGallery.EntityFrameworkCore.EntityConfigurations
{
    public class AlbumConfiguration : IEntityTypeConfiguration<Album>
    {
        public void Configure(EntityTypeBuilder<Album> builder)
        {
            builder.ToTable(PhotoGalleryConsts.DbTablePrefix + "Albums", PhotoGalleryConsts.DbSchema);
            
            builder.ConfigureByConvention();
            
            // Properties
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(x => x.Topic)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(x => x.Description)
                .HasMaxLength(500);
                
            builder.Property(x => x.CoverImagePath)
                .HasMaxLength(500);
            
            // Indexes
            builder.HasIndex(x => x.Name).IsUnique();
            builder.HasIndex(x => x.Topic);
            builder.HasIndex(x => x.DisplayOrder);
            
            // Relationships
            builder.HasMany(x => x.Photos)
                .WithOne(x => x.Album)
                .HasForeignKey(x => x.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
```

### PhotoConfiguration

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhotoGallery.Photos;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace PhotoGallery.EntityFrameworkCore.EntityConfigurations
{
    public class PhotoConfiguration : IEntityTypeConfiguration<Photo>
    {
        public void Configure(EntityTypeBuilder<Photo> builder)
        {
            builder.ToTable(PhotoGalleryConsts.DbTablePrefix + "Photos", PhotoGalleryConsts.DbSchema);
            
            builder.ConfigureByConvention();
            
            // Properties
            builder.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(x => x.FilePath)
                .IsRequired()
                .HasMaxLength(500);
                
            builder.Property(x => x.ThumbnailPath)
                .HasMaxLength(500);
                
            builder.Property(x => x.Caption)
                .HasMaxLength(200);
                
            builder.Property(x => x.Location)
                .HasMaxLength(100);
            
            // Indexes
            builder.HasIndex(x => x.AlbumId);
            builder.HasIndex(x => x.DisplayOrder);
            builder.HasIndex(x => x.Location);
            builder.HasIndex(x => x.DateTaken);
            builder.HasIndex(x => new { x.AlbumId, x.DisplayOrder });
        }
    }
}
```

## AutoMapper Profiles

```csharp
using AutoMapper;
using PhotoGallery.Albums;
using PhotoGallery.Photos;

namespace PhotoGallery
{
    public class PhotoGalleryApplicationAutoMapperProfile : Profile
    {
        public PhotoGalleryApplicationAutoMapperProfile()
        {
            // Album mappings
            CreateMap<Album, AlbumDto>()
                .ForMember(dest => dest.PhotoCount, opt => opt.MapFrom(src => src.Photos.Count));
            CreateMap<Album, AlbumLookupDto>();
            CreateMap<CreateAlbumDto, Album>()
                .ConstructUsing((src, context) => new Album(
                    GuidGenerator.Create(),
                    src.Name,
                    src.Topic,
                    src.Description,
                    src.DisplayOrder));
            CreateMap<UpdateAlbumDto, Album>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Photos, opt => opt.Ignore())
                .ForMember(dest => dest.DisplayOrder, opt => opt.Ignore())
                .ForMember(dest => dest.CoverImagePath, opt => opt.Ignore());
            
            // Photo mappings
            CreateMap<Photo, PhotoDto>()
                .ForMember(dest => dest.AlbumName, opt => opt.MapFrom(src => src.Album.Name));
            CreateMap<CreatePhotoDto, Photo>()
                .ConstructUsing((src, context) => new Photo(
                    GuidGenerator.Create(),
                    src.AlbumId,
                    src.File.FileName,
                    string.Empty, // Will be set after file processing
                    src.File.Length,
                    src.DisplayOrder,
                    src.Caption,
                    src.Location));
            CreateMap<UpdatePhotoDto, Photo>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AlbumId, opt => opt.Ignore())
                .ForMember(dest => dest.FileName, opt => opt.Ignore())
                .ForMember(dest => dest.FilePath, opt => opt.Ignore())
                .ForMember(dest => dest.FileSize, opt => opt.Ignore())
                .ForMember(dest => dest.DisplayOrder, opt => opt.Ignore());
        }
    }
}
```

## Validation Rules (FluentValidation)

### Album Validators

```csharp
using FluentValidation;
using PhotoGallery.Albums;

namespace PhotoGallery.Validation
{
    public class CreateAlbumDtoValidator : AbstractValidator<CreateAlbumDto>
    {
        public CreateAlbumDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Album name is required")
                .Length(1, 100).WithMessage("Album name must be between 1 and 100 characters");
                
            RuleFor(x => x.Topic)
                .NotEmpty().WithMessage("Topic is required")
                .Length(1, 50).WithMessage("Topic must be between 1 and 50 characters");
                
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
                
            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");
        }
    }
    
    public class UpdateAlbumDtoValidator : AbstractValidator<UpdateAlbumDto>
    {
        public UpdateAlbumDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Album name is required")
                .Length(1, 100).WithMessage("Album name must be between 1 and 100 characters");
                
            RuleFor(x => x.Topic)
                .NotEmpty().WithMessage("Topic is required")
                .Length(1, 50).WithMessage("Topic must be between 1 and 50 characters");
                
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        }
    }
}
```

### Photo Validators

```csharp
using FluentValidation;
using PhotoGallery.Photos;

namespace PhotoGallery.Validation
{
    public class CreatePhotoDtoValidator : AbstractValidator<CreatePhotoDto>
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
        
        public CreatePhotoDtoValidator()
        {
            RuleFor(x => x.AlbumId)
                .NotEmpty().WithMessage("Album ID is required");
                
            RuleFor(x => x.File)
                .NotNull().WithMessage("File is required")
                .Must(file => file.Length > 0).WithMessage("File cannot be empty")
                .Must(file => file.Length <= MaxFileSize).WithMessage($"File size cannot exceed {MaxFileSize / 1024 / 1024}MB")
                .Must(file => AllowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
                .WithMessage($"File must be one of the following types: {string.Join(", ", AllowedExtensions)}");
                
            RuleFor(x => x.Caption)
                .MaximumLength(200).WithMessage("Caption cannot exceed 200 characters");
                
            RuleFor(x => x.Location)
                .MaximumLength(100).WithMessage("Location cannot exceed 100 characters");
                
            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");
        }
    }
    
    public class UpdatePhotoDtoValidator : AbstractValidator<UpdatePhotoDto>
    {
        public UpdatePhotoDtoValidator()
        {
            RuleFor(x => x.Caption)
                .MaximumLength(200).WithMessage("Caption cannot exceed 200 characters");
                
            RuleFor(x => x.Location)
                .MaximumLength(100).WithMessage("Location cannot exceed 100 characters");
        }
    }
}
```

## Relationship Diagram

```
┌─────────────────┐       1:N        ┌─────────────────┐
│      Album      │◄─────────────────►│      Photo      │
├─────────────────┤                   ├─────────────────┤
│ Id (PK)         │                   │ Id (PK)         │
│ Name (unique)   │                   │ AlbumId (FK)    │
│ Topic           │                   │ FileName        │
│ Description     │                   │ FilePath        │
│ DisplayOrder    │                   │ ThumbnailPath   │
│ CoverImagePath  │                   │ FileSize        │
│ CreationTime    │                   │ Caption         │
│ LastModTime     │                   │ Location        │
└─────────────────┘                   │ DisplayOrder    │
                                      │ Width           │
                                      │ Height          │
                                      │ DateTaken       │
                                      │ CreationTime    │
                                      └─────────────────┘
```

## Business Rules

### Album Business Rules
1. Album names must be unique across the system
2. Display order is automatically managed when not specified
3. Deleting an album cascades to delete all associated photos
4. Cover image must be from a photo within the album
5. Topics are free-form text but should be consistent for filtering

### Photo Business Rules
1. Photos must belong to an existing album
2. File uploads are validated for type, size, and content
3. Display order within an album determines photo sequence
4. Thumbnail generation is automatic upon upload
5. EXIF data extraction is performed when available
6. File naming prevents conflicts and maintains security

---

**Data Model Status**: Complete  
**Entity Relationships**: Defined  
**Validation Rules**: Specified  
**Ready for Implementation**: ✅