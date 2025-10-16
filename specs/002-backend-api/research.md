# Research and Technical Decisions

## Overview

This document consolidates research findings and technical decisions for the PhotoGallery Backend API implementation. All technical clarifications from the planning phase have been resolved based on ABP Framework best practices and project requirements.

## Technical Decisions

### Decision 1: Validation Strategy
**Decision**: Use FluentValidation at the application layer instead of data annotation attributes on domain entities  
**Rationale**: 
- Aligns with ABP Framework recommendations for clean DDD architecture
- Keeps domain entities free of infrastructure concerns
- Provides more flexible and testable validation logic
- Supports complex business rule validation scenarios

**Alternatives Considered**:
- Data Annotations: Rejected because it couples domain entities to validation framework
- Custom validation attributes: Rejected for complexity and maintainability

**Implementation**: FluentValidation validators for all DTOs in the Application layer

### Decision 2: File Storage Approach
**Decision**: Direct file system storage with organized folder structure  
**Rationale**:
- Simple implementation for MVP requirements
- No external dependencies or cloud provider costs
- Easy to migrate to cloud storage later if needed
- Sufficient for expected scale (< 50,000 photos)

**Alternatives Considered**:
- Azure Blob Storage: Deferred for future enhancement
- AWS S3: Deferred for future enhancement  
- Database BLOB storage: Rejected for performance and storage efficiency

**Implementation**: `/wwwroot/uploads/photos/{albumId}/` structure with original and thumbnail subfolders

### Decision 3: Image Processing Library
**Decision**: Use System.Drawing.Common for thumbnail generation  
**Rationale**:
- Built into .NET framework, no additional dependencies
- Sufficient for basic thumbnail generation requirements
- Cross-platform support in .NET 9
- Well-documented and stable

**Alternatives Considered**:
- ImageSharp: More features but additional dependency and complexity
- SkiaSharp: Overkill for simple thumbnail generation
- Third-party services: Not needed for MVP scope

**Implementation**: Thumbnail generation service in Application layer

### Decision 4: EXIF Data Handling
**Decision**: Use MetadataExtractor library for EXIF data extraction  
**Rationale**:
- Mature library with comprehensive EXIF support
- Handles various image formats reliably
- Active maintenance and good documentation
- Minimal performance impact

**Alternatives Considered**:
- System.Drawing metadata: Limited EXIF support
- Custom EXIF parsing: Too complex and error-prone
- No EXIF data: Would lose valuable photo metadata

**Implementation**: Extract DateTaken, GPS location, and image dimensions during upload

### Decision 5: Database Indexing Strategy
**Decision**: Create indexes on frequently queried fields  
**Rationale**:
- DisplayOrder for album and photo ordering queries
- AlbumId for photo lookups by album
- Topic for album filtering
- CreationTime for audit and sorting

**Alternatives Considered**:
- No indexes: Would cause performance issues with scale
- Composite indexes: May be added later based on query patterns

**Implementation**: EF Core index configuration in entity configurations

### Decision 6: Authorization Model
**Decision**: Use ABP Framework's permission-based authorization  
**Rationale**:
- Consistent with ABP Framework patterns
- Supports role-based and permission-based access control
- Easy to extend for future requirements
- Built-in UI for permission management

**Alternatives Considered**:
- Custom authorization: Would duplicate ABP functionality
- Simple role-based: Less flexible than permissions

**Implementation**: Permission definitions for Albums.Create, Albums.Edit, Albums.Delete, Photos.Upload, Photos.Delete

### Decision 7: API Response Patterns
**Decision**: Follow ABP Framework's standard response patterns  
**Rationale**:
- Consistent with existing codebase
- Automatic error handling and response formatting
- Built-in support for localization and validation errors
- Standard HTTP status codes

**Alternatives Considered**:
- Custom response wrappers: Would break ABP conventions
- Minimal responses: Less informative for API consumers

**Implementation**: Use ABP's ApplicationService base class and standard DTOs

### Decision 8: Caching Strategy
**Decision**: Defer caching implementation to future iteration  
**Rationale**:
- MVP scope focuses on core functionality
- Premature optimization without performance metrics
- Can be added later based on actual usage patterns
- ABP Framework provides easy caching integration when needed

**Alternatives Considered**:
- Immediate caching implementation: Would add complexity without clear benefit

**Implementation**: Document caching opportunities for future enhancement

## Performance Considerations

### Database Query Optimization
- Use EF Core's `Include()` for loading related data efficiently
- Implement pagination for album and photo listings
- Add database indexes on frequently queried columns
- Use async methods throughout for non-blocking operations

### File Upload Performance
- Stream file uploads directly to disk to minimize memory usage
- Generate thumbnails asynchronously after upload completion
- Validate file types and sizes before processing
- Use proper disposal patterns for file streams and image objects

### Memory Management
- Dispose image processing objects properly
- Use streaming for large file operations
- Implement proper async patterns to avoid thread pool starvation
- Monitor memory usage during file operations

## Security Implementation

### Input Validation
- FluentValidation rules for all input DTOs
- File type validation using content inspection, not just extensions
- File size limits enforced at multiple levels (server config and application)
- Path traversal prevention in file naming

### File Security
- Generate unique file names to prevent conflicts and guess attacks
- Store files outside of web root where possible
- Validate uploaded file content, not just extensions
- Implement virus scanning hooks for future enhancement

### Authorization
- Permission checks before all business operations
- Audit logging for sensitive operations (create, update, delete)
- Input sanitization for text fields
- SQL injection prevention through EF Core parameterized queries

## Testing Strategy

### Unit Testing Approach
- Test domain entities for business rule validation
- Test application services with mocked dependencies
- Test AutoMapper configurations
- Test FluentValidation rules comprehensively

### Integration Testing Approach
- Test API endpoints with real database (in-memory for tests)
- Test file upload functionality end-to-end
- Test authorization and permission enforcement
- Test database migrations and seeding

### Test Data Management
- Use ABP's test framework for consistent test setup
- Create factory methods for test data generation
- Use realistic but anonymized test data
- Implement proper test cleanup and isolation

## Development Workflow

### Code Organization
- Follow ABP Framework folder structure conventions
- Group related functionality in feature folders
- Use consistent naming patterns throughout
- Implement proper separation of concerns

### Migration Strategy
- Create granular migrations for each entity
- Test migrations both up and down
- Include sample data in development seeding
- Document any manual migration steps

### Documentation Standards
- XML documentation for all public APIs
- OpenAPI/Swagger documentation generated automatically
- README files for setup and development procedures
- Architecture decision records for major technical choices

## Future Enhancement Opportunities

### Scalability Improvements
- Implement distributed caching (Redis)
- Move to cloud storage (Azure Blob Storage)
- Add CDN for image delivery
- Implement background job processing for image operations

### Feature Enhancements
- Advanced image processing (filters, effects)
- Bulk upload capabilities
- Image metadata search
- Geographic search by location data
- Social features (likes, comments, sharing)

### Performance Optimizations
- Implement lazy loading for large galleries
- Add image compression and multiple size variants
- Implement progressive image loading
- Add full-text search capabilities

---

**Research Status**: Complete  
**All Technical Clarifications**: Resolved  
**Ready for Phase 1**: ✅