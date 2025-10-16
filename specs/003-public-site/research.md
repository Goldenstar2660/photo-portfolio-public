# Phase 0: Research & Analysis

## Technical Decisions

### ASP.NET Core Razor Pages vs MVC

**Decision**: ASP.NET Core Razor Pages  
**Rationale**: Razor Pages provides a more straightforward approach for page-focused web applications like a public gallery site. It offers better organization for simple CRUD operations and page-specific logic while maintaining SEO-friendly server-side rendering.  
**Alternatives considered**: 
- ASP.NET Core MVC: More complex setup for simple pages
- Blazor Server: Adds complexity and SignalR dependency for a public site
- React/Vue SPA: Requires authentication handling and SEO considerations

### Bootstrap Template Integration Strategy

**Decision**: Direct integration with custom Razor Pages layout  
**Rationale**: The provided Bootstrap 5 template (CHEFER chef theme) will be adapted by:
1. Converting HTML templates to Razor Pages Layout (`_Layout.cshtml`)
2. Extracting CSS/JS assets to `wwwroot` folder
3. Modifying navigation to support dynamic album dropdown
4. Adapting page structures to match template layouts  
**Alternatives considered**:
- Component-based approach: Overly complex for static template
- NuGet theme packages: Template is custom, not available as package

### API Integration Pattern

**Decision**: Hybrid approach - Direct service reference with HTTP API fallback  
**Rationale**: 
- **Primary**: Direct reference to `PhotoGallery.Application` for optimal performance
- Use dependency injection to consume `IAlbumAppService` and `IPhotoAppService` directly
- Eliminate network overhead and serialization costs
- **Secondary**: Keep HTTP API available for React/external clients
- Better error handling and type safety with direct service calls  
**Alternatives considered**:
- HTTP-only approach: Unnecessary network overhead for same-process calls
- Service-only approach: Loses API availability for external clients

### State Management

**Decision**: Server-side ViewModels with minimal client-side state  
**Rationale**: 
- ViewModels aggregate data from multiple API calls
- Server-side rendering maintains SEO benefits
- Minimal JavaScript for UI interactions (photo popups, filtering)
- Session/TempData for cross-page state when needed  
**Alternatives considered**:
- Client-side state management: Unnecessary for simple gallery site
- Database caching: Public site doesn't need persistence

## Technology Best Practices

### ASP.NET Core Configuration

**Decision**: Standard ASP.NET Core patterns with .NET 9 modern features  
**Rationale**:
- Use built-in dependency injection with primary constructors
- Configuration through `appsettings.json` for API endpoints
- Health checks for API connectivity monitoring
- Logging through built-in `ILogger` interface
- Simplified resilience patterns with `AddStandardResilienceHandler()`
- Modern C# 12 features (primary constructors, required properties, collection expressions)  

### Performance Optimization

**Decision**: Implement caching and image optimization  
**Rationale**:
- Memory caching for album lists and frequently accessed data
- HTTP response caching for static content
- Image lazy loading and responsive images
- CDN consideration for static assets  

### Error Handling

**Decision**: Graceful degradation with user-friendly error pages  
**Rationale**:
- Custom error pages matching template design
- Fallback content when API is unavailable
- Proper logging for debugging while hiding technical details
- Circuit breaker pattern for API resilience  

## API Integration Patterns

### Data Retrieval Strategy

**Decision**: Aggregate API calls in service layer  
**Rationale**:
Based on the backend API specification analysis:
- Albums API: `GET /api/albums`, `GET /api/albums/topics`, `GET /api/albums/by-topic/{topic}`
- Photos API: `GET /api/photos/by-album/{albumId}`, `GET /api/photos/random`
- Service layer will compose multiple API calls for page-specific ViewModels

### Authentication Considerations

**Decision**: Public access with no authentication required  
**Rationale**: 
- Public gallery site should be accessible without login
- API calls will use anonymous access to public endpoints
- Future enhancement could add optional user features

### Data Transformation

**Decision**: DTO mapping and ViewModel composition  
**Rationale**:
- API DTOs: `AlbumDto`, `PhotoDto` (from backend contracts)
- ViewModels: `HomeViewModel`, `AlbumViewModel`, `PhotoViewModel`, `PhotoDetailViewModel`, `AboutViewModel`
- AutoMapper for DTO to ViewModel transformations
- Composition patterns for complex page data
- Dedicated lightbox ViewModel (`PhotoDetailViewModel`) with navigation metadata

### About Page Content Management

**Decision**: Configuration-based content for easy updates without code deployment  
**Rationale**:
- **Content source**: Store biography and personal info in `appsettings.json` or database (FR-012)
- **Profile photo**: Static image in wwwroot or configurable path
- **Hobbies**: Specifically highlight biking and photography interests (FR-011)
- **Editable**: Allow content updates without requiring code changes
- **Template integration**: Match CHEFER template's about page aesthetic

**Implementation approach**:
- Configuration section for personal information (name, bio, profile image path)
- Hobby collection with icons (using FontAwesome for biking and camera icons)
- Optional: Admin interface integration for content editing (future enhancement)
- Static content versioning for cache invalidation

## UI/UX Integration Patterns

### Bootstrap Template Adaptation

**Decision**: Maintain template visual identity with functional adaptations  
**Rationale**:
- Keep CHEFER template's color scheme and typography
- Adapt navigation structure for photo gallery context
- Modify page layouts to support dynamic content
- Ensure responsive design principles are maintained

### Photo Display Strategies

**Decision**: Progressive enhancement with lightbox for image interactions  
**Rationale**:
- **Base functionality**: Grid view works without JavaScript (graceful degradation)
- **Enhanced features**: Lightbox/modal for full-size viewing with metadata overlay
- **Keyboard navigation**: Arrow keys for next/previous, Esc to close lightbox (FR-024)
- **Hover metadata**: Display date and location on photo hover (FR-019)
- **Lazy loading**: Intersection Observer API for below-the-fold photos (FR-018)
- **Thumbnail optimization**: Serve thumbnails in grid, full-resolution in lightbox (FR-030)
- **Responsive images**: Picture element with srcset for different screen sizes

**Implementation approach**:
- Lightbox library: Consider PhotoSwipe or custom lightweight solution
- Metadata overlay: CSS transitions with graceful fallback
- Progressive loading: Thumbnail first, then full-resolution on demand

### Navigation Enhancement

**Decision**: Persistent navigation with dynamic albums dropdown and mobile responsiveness  
**Rationale**:
- **Persistent menu**: Navigation bar on all pages (FR-001)
- **Albums dropdown**: Dynamically populated, organized by topic (FR-002)
- **Mobile menu**: Hamburger menu for screens <768px (FR-003)
- **Current page highlighting**: Visual indicator for active page (FR-004)
- **Caching strategy**: 1-hour cache for navigation data to reduce API calls
- **Fallback handling**: Static menu structure if API unavailable
- **Accessibility**: Keyboard navigation and ARIA labels for screen readers

**Implementation approach**:
- Bootstrap 5 navbar component with custom dropdown
- Server-side rendering for initial menu state
- Client-side JavaScript for mobile toggle and dropdown interactions
- Topic grouping: Organize albums by topic within dropdown

## Development Workflow

### Project Structure

**Decision**: Separate project within existing solution  
**Rationale**:
- `PhotoGallery.PublicWeb` as independent Razor Pages project
- Shared infrastructure (HttpApi.Client) for API communication
- Independent deployment capability
- Clear separation from admin functionality

### Testing Strategy

**Decision**: Comprehensive testing at multiple levels  
**Rationale**:
- Unit tests for service classes and ViewModels
- Integration tests for API communication
- UI tests for critical user journeys
- Performance testing for image loading

### Deployment Considerations

**Decision**: Standard ASP.NET Core deployment patterns  
**Rationale**:
- IIS hosting support for Windows environments
- Docker containerization for cloud deployment
- Static file handling for template assets
- CDN integration for production optimization