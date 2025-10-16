# Implementation Plan: Backend API

**Branch**: `002-backend-api` | **Date**: 2025-10-14 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-backend-api/spec.md`

## Summary

Implement a complete backend REST API for the PhotoGallery application using ABP Framework's Domain-Driven Design (DDD) layered architecture. The API will provide full CRUD operations for Albums and Photos, file upload capabilities with thumbnail generation, comprehensive validation using FluentValidation at the application layer, and memory caching for improved performance on GetList operations.

**Technical Context**

**Language/Version**: C# / .NET 9 with ABP Framework 9.0+  
**Primary Dependencies**: ABP Framework, Entity Framework Core, FluentValidation, AutoMapper, IMemoryCache  
**Storage**: SQL Server with Entity Framework Core, File system for photo storage  
**Testing**: xUnit, ABP Testing Framework, Shouldly  
**Target Platform**: ASP.NET Core Web API (Linux/Windows server)  
**Project Type**: Web API backend following ABP Framework layered architecture  
**Performance Goals**: < 50ms cached response time, < 200ms uncached API response time, handle file uploads up to 10MB  
**Constraints**: Follow ABP conventions, use FluentValidation instead of data annotations, avoid domain managers unless required, implement memory caching for GetList operations  
**Scale/Scope**: Photo gallery management for single photographer, expected < 1000 albums, < 50,000 photos, 70%+ cache hit rate

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

✅ **Code Quality Principles**
- Follows ABP Framework DDD patterns and SOLID principles
- Single responsibility maintained in each layer
- Consistent with existing codebase formatting and standards

✅ **Architecture Standards**  
- Domain-Driven Design with clear layer separation
- Dependencies flow inward toward domain
- Infrastructure concerns isolated from business logic
- No circular dependencies

✅ **Security Standards**
- Input validation at application service boundaries using FluentValidation
- File upload validation for size, type, and security
- Authorization checks before business logic execution
- Audit logging through ABP audit system

✅ **Testing Standards**
- Unit tests for domain logic and application services
- Integration tests for API endpoints and database operations
- Test coverage target > 80% for business logic

✅ **Performance Requirements**
- Cached API response time < 50ms for GetList operations
- Uncached API response time < 200ms for standard operations
- Cache hit rate > 70% for frequently accessed data
- Async operations for file uploads and database queries
- Proper database indexing and query optimization
- Memory management for image processing and cache storage

✅ **Documentation Requirements**
- OpenAPI/Swagger documentation for all endpoints
- Code documentation following ABP conventions
- Setup and deployment procedures

## Project Structure

### Documentation (this feature)

```
specs/002-backend-api/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output  
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (API specifications)
└── tasks.md             # Phase 2 output (NOT created by this command)
```

### Source Code (repository root)

```
src/
├── PhotoGallery.Domain/
│   ├── Albums/
│   │   └── Album.cs                    # Album entity
│   ├── Photos/
│   │   └── Photo.cs                    # Photo entity  
│   └── Data/
│       └── PhotoGalleryDataSeeder.cs   # Sample data seeding
├── PhotoGallery.Application.Contracts/
│   ├── Albums/
│   │   ├── IAlbumAppService.cs         # Album service interface
│   │   ├── AlbumDto.cs                 # Album DTOs
│   │   ├── CreateAlbumDto.cs
│   │   └── UpdateAlbumDto.cs
│   ├── Photos/
│   │   ├── IPhotoAppService.cs         # Photo service interface
│   │   ├── PhotoDto.cs                 # Photo DTOs
│   │   └── ReorderPhotosDto.cs
│   └── Permissions/
│       └── PhotoGalleryPermissions.cs  # Permission definitions
├── PhotoGallery.Application/
│   ├── Albums/
│   │   ├── AlbumAppService.cs          # Album business logic with caching
│   │   └── AlbumDtoValidator.cs        # FluentValidation rules
│   ├── Photos/
│   │   ├── PhotoAppService.cs          # Photo business logic with caching
│   │   └── PhotoDtoValidator.cs        # FluentValidation rules
│   └── Services/
│       ├── FileUploadService.cs        # File handling service
│       └── ICacheKeyService.cs         # Cache key generation service
├── PhotoGallery.HttpApi/
│   └── Controllers/
│       ├── AlbumController.cs          # Album REST endpoints
│       └── PhotoController.cs          # Photo REST endpoints
├── PhotoGallery.EntityFrameworkCore/
│   ├── EntityConfigurations/
│   │   ├── AlbumConfiguration.cs       # EF configuration
│   │   └── PhotoConfiguration.cs       # EF configuration
│   └── Migrations/                     # Database migrations
└── PhotoGallery.HttpApi.Host/
    └── wwwroot/uploads/photos/          # File storage location

tests/
├── PhotoGallery.Application.Tests/
│   ├── Albums/
│   │   └── AlbumAppServiceTests.cs     # Album service tests
│   └── Photos/
│       └── PhotoAppServiceTests.cs     # Photo service tests
└── PhotoGallery.HttpApi.Tests/
    ├── Controllers/
    │   ├── AlbumControllerTests.cs      # Album API tests
    │   └── PhotoControllerTests.cs      # Photo API tests
    └── Integration/
        └── FileUploadTests.cs           # File upload integration tests
```

**Structure Decision**: Follows ABP Framework's standard DDD layered architecture with Domain, Application, HttpApi, and EntityFrameworkCore layers. Each feature (Albums, Photos) has its own folder within each layer to maintain organization and support future scaling.

## Complexity Tracking

*No constitution violations requiring justification*

| Aspect | Complexity Level | Justification |
|--------|------------------|---------------|
| Layer Count | Standard (4 layers) | Follows ABP Framework standard DDD architecture |
| Dependencies | Minimal | Only necessary ABP packages plus FluentValidation |
| File Storage | Simple | Direct file system storage, no cloud complexity yet |