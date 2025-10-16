# PhotoGallery Development Guidelines

Auto-generated from all feature plans. Last updated: 2025-10-14

## Active Technologies
- C# / .NET 9 + ABP Framework, Entity Framework Core, FluentValidation, AutoMapper (002-backend-api)
- C# / .NET 9 + ASP.NET Core Razor Pages, Bootstrap 5, Entity Framework Core, Direct ABP Application Services (003-public-site)

## Project Structure
```
src/
├── PhotoGallery.Domain/             # Backend domain entities
│   ├── Albums/
│   │   └── Album.cs                 # Album aggregate root
│   └── Photos/
│       └── Photo.cs                 # Photo entity
├── PhotoGallery.Application/        # Backend application services
│   ├── Albums/
│   │   ├── AlbumAppService.cs       # Album business logic
│   │   └── AlbumDtoValidator.cs     # FluentValidation rules
│   └── Photos/
│       ├── PhotoAppService.cs       # Photo business logic
│       └── PhotoDtoValidator.cs     # FluentValidation rules
├── PhotoGallery.HttpApi/            # Backend REST controllers
│   └── Controllers/
│       ├── AlbumController.cs       # Album REST endpoints
│       └── PhotoController.cs       # Photo REST endpoints
├── PhotoGallery.PublicWeb/          # New Razor Pages project
│   ├── Pages/                       # Razor Pages
│   ├── Services/                    # API integration services
│   ├── Models/                      # View models and DTOs
│   ├── wwwroot/                     # Static assets from Bootstrap template
│   └── Program.cs                   # Application startup
├── PhotoGallery.HttpApi.Host/       # Existing API host
└── [other existing projects...]

tests/
├── PhotoGallery.Application.Tests/  # Backend API tests
│   ├── Albums/
│   │   └── AlbumAppServiceTests.cs  # Album service tests
│   └── Photos/
│       └── PhotoAppServiceTests.cs  # Photo service tests
├── PhotoGallery.PublicWeb.Tests/    # New test project
│   ├── Unit/                        # Unit tests
│   ├── Integration/                 # Integration tests
│   └── E2E/                         # End-to-end tests
└── [other existing test projects...]
```

## Commands
dotnet build; dotnet test; dotnet run

## Code Style
C# / .NET 9: Follow standard conventions
- Use ABP Framework DDD patterns and layered architecture
- Apply FluentValidation at application layer (no data annotations on domain entities)
- Implement async/await patterns for all I/O operations
- Follow SOLID principles consistently
- Use AutoMapper for entity-DTO transformations

C# / .NET 9: Follow standard conventions
- Use ASP.NET Core Razor Pages patterns
- Implement dependency injection for services
- Apply SOLID principles consistently
- Use async/await patterns for API calls
- Follow ABP Framework conventions for existing projects

## Recent Changes
- 002-backend-api: Added C# / .NET 9 + ABP Framework, Entity Framework Core, FluentValidation, AutoMapper
- 003-public-site: Added C# / .NET 9 + ASP.NET Core Razor Pages, Bootstrap 5, Entity Framework Core, HttpClient for API calls

<!-- MANUAL ADDITIONS START -->
## Project-Specific Guidelines

### PhotoGallery Public Web Application
- **Architecture**: ASP.NET Core Razor Pages with direct ABP Application Service integration
- **UI Framework**: Bootstrap 5 (CHEFER template adaptation)
- **Service Integration**: Direct service references for optimal performance (no HTTP layer)
- **Caching**: IMemoryCache for data responses
- **Error Handling**: Direct exception handling with graceful fallbacks
- **Performance**: Elimination of HTTP serialization overhead, same-process transactions

### Key Patterns
- **ViewModels**: Aggregate data from multiple API calls
- **Services**: Typed HttpClient services with error handling
- **Mapping**: AutoMapper for DTO to ViewModel transformations
- **Validation**: Client-side and API-level validation
- **SEO**: Server-side rendering for public accessibility

### Testing Strategy
- **Unit Tests**: Service classes and ViewModels with mocked dependencies
- **Integration Tests**: API communication and HTTP client behavior
- **E2E Tests**: Critical user journeys (home page, album navigation, photo viewing)

### Security Considerations
- **Public Access**: No authentication required for gallery viewing
- **Input Validation**: Validate all user inputs (pagination, filtering)
- **XSS Prevention**: Use Razor Pages built-in encoding
- **HTTPS**: Enforce secure connections in production

### Constitution Compliance
- **Architecture**: Clean separation between presentation and data layers
- **Performance**: < 3s page load, < 200ms API integration
- **Testing**: 80%+ coverage for business logic
- **Code Quality**: SOLID principles, clean code standards
- **Error Handling**: User-friendly error pages with proper logging
<!-- MANUAL ADDITIONS END -->