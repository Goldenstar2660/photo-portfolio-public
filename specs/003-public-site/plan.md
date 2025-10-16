# Implementation Plan: Public Photo Gallery Site

**Branch**: `003-public-site` | **Date**: October 14, 2025 | **Spec**: [003-public-site/spec.md](./spec.md)
**Input**: Feature specification from `/specs/003-public-site/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Create a public-facing Razor Pages website that allows visitors to view photo galleries organized by albums. The site uses Bootstrap 5 templates and integrates directly with backend application services for optimal performance. Key features include:
- **Home Page**: Hero image, personal introduction, and 2-3 featured albums
- **Album Browsing**: Responsive tiled grid layout with photos organized by topic
- **Photo Details**: Interactive lightbox with metadata (date, location, caption) and keyboard navigation
- **About Page**: Photographer profile, biography highlighting hobbies (biking, photography)
- **Navigation**: Persistent navigation bar with Albums dropdown, responsive mobile hamburger menu

The site delivers four core user stories: browse photo albums (P1), view photo details (P2), navigate efficiently (P2), and learn about the photographer (P3).

## Technical Context

**Language/Version**: C# / .NET 9 (ASP.NET Core Razor Pages)  
**Primary Dependencies**: ASP.NET Core, Bootstrap 5, Entity Framework Core, Direct ABP Application Services  
**Storage**: Uses existing backend services for data retrieval (direct service calls)  
**Testing**: xUnit, ASP.NET Core Test Host, Selenium for E2E testing  
**Target Platform**: Web browsers (responsive design for desktop and mobile)  
**Project Type**: Web application - frontend consuming existing backend API  
**Performance Goals**: Page load < 3s on 3G, API response integration < 200ms  
**Constraints**: Must use provided Bootstrap 5 template, responsive design mandatory  
**Scale/Scope**: Public gallery site with 4 user stories - browse albums (P1), view photo details with lightbox (P2), efficient navigation (P2), about page (P3)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Code Quality Standards ✓
- Single Razor Pages project maintains single responsibility
- Clean separation between presentation (Razor Pages) and data access (API services)
- SOLID principles applied through service injection and interface abstractions

### Architecture Standards ✓  
- Clear separation of concerns: Razor Pages for presentation, service layer for API integration
- Dependency injection for API client services
- No direct database access - follows existing API boundaries

### Security Standards ⚠️
- Input validation required for any user inputs (search, filtering)
- XSS prevention through Razor Pages built-in encoding
- No authentication required for public site (as specified)

### Testing Standards ✓
- Unit tests for service classes and page models
- Integration tests for API integration
- E2E tests for critical user journeys

### Performance Requirements ✓
- Bootstrap 5 templates optimized for performance
- Image optimization and lazy loading for photo galleries
- Caching strategy for API responses

**GATE STATUS**: ✅ PASS - No constitution violations. Architecture follows established patterns.

## Project Structure

### Documentation (this feature)

```
specs/003-public-site/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```
src/
├── PhotoGallery.PublicWeb/          # New Razor Pages project
│   ├── Pages/                       # Razor Pages
│   │   ├── Index.cshtml            # Home page
│   │   ├── About.cshtml            # About page
│   │   ├── Albums/                 # Album-related pages
│   │   │   ├── Index.cshtml        # Album listing
│   │   │   └── Details.cshtml      # Individual album view
│   │   ├── Shared/                 # Shared layout and components
│   │   │   ├── _Layout.cshtml      # Main layout using Bootstrap template
│   │   │   ├── _ViewStart.cshtml   # View start
│   │   │   └── _ViewImports.cshtml # View imports
│   │   └── Error.cshtml            # Error page
│   ├── Services/                   # API integration services
│   │   ├── IPhotoGalleryApiService.cs
│   │   └── PhotoGalleryApiService.cs
│   ├── Models/                     # View models and DTOs
│   │   ├── AlbumViewModel.cs
│   │   ├── PhotoViewModel.cs
│   │   └── HomeViewModel.cs
│   ├── wwwroot/                    # Static assets from Bootstrap template
│   │   ├── css/                    # Adapted Bootstrap styles
│   │   ├── js/                     # JavaScript from template
│   │   ├── img/                    # Template images
│   │   └── lib/                    # Third-party libraries
│   ├── Program.cs                  # Application startup
│   ├── appsettings.json           # Configuration
│   └── PhotoGallery.PublicWeb.csproj
├── PhotoGallery.Application/       # Existing backend (no changes)
├── PhotoGallery.HttpApi.Host/      # Existing API (no changes)
└── [other existing projects...]

tests/
├── PhotoGallery.PublicWeb.Tests/   # New test project
│   ├── Unit/                       # Unit tests
│   │   ├── Services/              # Service tests
│   │   └── Pages/                 # Page model tests
│   ├── Integration/               # Integration tests
│   │   └── ApiIntegrationTests.cs
│   └── E2E/                       # End-to-end tests
│       └── UserJourneyTests.cs
└── [other existing test projects...]
```

**Structure Decision**: Web application structure selected because this is a frontend-only project that consumes the existing backend API. The Razor Pages pattern aligns with the requirement for server-side rendering and SEO optimization for a public gallery site.

## Complexity Tracking

*No constitution violations requiring justification.*
