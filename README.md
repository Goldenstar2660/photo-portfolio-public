# Derek's Photography Portfolio

A full-stack, modular .NET 9 photography portfolio website built with the ABP Framework and ASP.NET Core Razor Pages. This repository demonstrates clean DDD boundaries, AutoMapper-based DTO mapping, FluentValidation at the application layer, and a Razor Pages front end designed for fast, SEO-friendly server-side rendering.

## Overview

PhotoGallery manages Albums and Photos as first-class concepts and exposes them through application services and a REST API. A Razor Pages “PublicWeb” front end renders browsable views for albums and photos using a Bootstrap 5 template, with a focus on low latency via direct, in-process calls to application services (no HTTP hop inside the same process). The repository includes a DbMigrator for reliable database provisioning and a comprehensive specs folder that documents plans, API contracts, and checklists.

## Key Features

- Clean architecture: Modular separation of Domain, Application, API, EF Core, and Razor Pages layers.
- Full CRUD support for albums and photos with RESTful endpoints and DTO mapping.
- Server-side rendering (SSR) using Razor Pages and Bootstrap 5 for accessibility and SEO.
- Low-latency integration: In-process service calls and IMemoryCache reduce response times.
- Database automation: EF Core migrations and a containerized DbMigrator streamline setup.
- Object storage: Cloudflare R2 (S3-compatible) with CDN-aware URL generation.
- Dynamic images: On-the-fly resize/format with ImageSharp and caching.
- Comprehensive documentation: Specs folder with contracts, design plans, and success criteria.
  
## Tech Stack

- Languages: C#, SQL
- Frameworks: .NET 9, ABP Framework, ASP.NET Core Razor Pages
- Libraries: EF Core, AutoMapper, FluentValidation, IMemoryCache, ImageSharp
- Storage/CDN: Cloudflare R2 (S3-compatible via AWS SDK for .NET), optional CDN domain
- Frontend: Bootstrap 5 (SSR)
- Database: SQL Server
- DevOps: Docker, EF Core Migrations, DbMigrator, Swagger/OpenAPI

## Screenshots

### Home:
  
  <img width="1817" height="927" alt="image" src="https://github.com/user-attachments/assets/4f3b95e1-90af-45fd-b325-cce6553e2a23" />
  <img width="1817" height="748" alt="image" src="https://github.com/user-attachments/assets/46c6ea6e-5e47-4953-a8aa-7e70553aaf3f" />

### Albums Grid:

  <img width="1819" height="964" alt="image" src="https://github.com/user-attachments/assets/ece51eba-09b9-463e-a80a-ed1b3fb69f75" />
  <img width="1820" height="964" alt="image" src="https://github.com/user-attachments/assets/acce19be-696e-4370-82c2-6297d52c8bbb" />
  
### Photo Details:

  <img width="1836" height="964" alt="image" src="https://github.com/user-attachments/assets/6f5b35b7-f1f5-47ca-aaf5-ffdfbe2d9bf2" />

## Architecture Overview

- Domain Layer: Pure entities and domain logic (Albums, Photos).
- Application Layer: Orchestrates use cases, applies validation, and maps entities ↔ DTOs.
- API Layer: REST endpoints that delegate to application services.
- PublicWeb: Razor Pages front-end calling application services in-process for minimal latency.
- Persistence: EF Core for ORM, repositories, and migrations.
- DbMigrator: Applies migrations and seeds data automatically (supports Dockerized runs).

Design Patterns: Repository/UoW, DTO mapping via AutoMapper, FluentValidation, async I/O, and caching for heavy read paths.

## Project Structure

````text
src/
├─ PhotoGallery.Domain/               # Domain entities, domain module (Albums, Photos)
├─ PhotoGallery.Application/          # Application services, validators, AutoMapper profile
├─ PhotoGallery.Application.Contracts/# DTOs, contracts shared across layers
├─ PhotoGallery.EntityFrameworkCore/  # DbContext, entity configs, migrations
├─ PhotoGallery.HttpApi/              # API module (controllers live here in ABP style)
├─ PhotoGallery.HttpApi.Host/         # API host (web server entry point)
├─ PhotoGallery.PublicWeb/            # Razor Pages public site (SSR, Bootstrap 5)
└─ PhotoGallery.DbMigrator/           # Migration/seed runner for reliable setup

specs/
├─ 002-backend-api/                   # Data model, contracts (albums, photos), plan, tasks
├─ 003-public-site/                   # Public site plan, template, http-client config (legacy)
├─ 004-admin-site/                    # Admin site plan (React, future)
└─ complete-project-plan.md           # End-to-end plans
````

## Getting Started (Windows)

Prerequisites:
- .NET 9 SDK
- SQL Server (LocalDB or full), or adjust EF Core provider/connection string as needed

1) Restore, build, and run tests:
````powershell
dotnet restore
dotnet build
dotnet test
````

2) Configure database connection:
- Update the Default connection string in appsettings.json for:
  - PhotoGallery.DbMigrator
  - PhotoGallery.HttpApi.Host
  - PhotoGallery.PublicWeb (if it maintains its own connection/app settings)

3) Apply migrations and seed:
````powershell
dotnet run --project src/PhotoGallery.DbMigrator/PhotoGallery.DbMigrator.csproj
````

4) Run the API host:
````powershell
dotnet run --project src/PhotoGallery.HttpApi.Host/PhotoGallery.HttpApi.Host.csproj
````

5) Run the Public site (Razor Pages):
- Preferred: direct in-process service integration in the same solution.
````powershell
dotnet run --project src/PhotoGallery.PublicWeb/PhotoGallery.PublicWeb.csproj
````

Notes:
- For single-process hosting, PublicWeb can reference Application and EF Core modules directly (per Project-Specific Guidelines).
- For multi-process hosting, PublicWeb can be adapted to call HttpApi via HttpClient; specs include an http-client-config, but the chosen architecture favors direct service calls for performance.

## Development Guidelines

- ABP DDD layering and module conventions.
- FluentValidation at the application layer.
- AutoMapper for DTO mapping.
- Async/await for all I/O.
- Razor Pages with DI for services and SSR for SEO.
- IMemoryCache for public read models.
- Error handling with graceful fallbacks and user-friendly pages.

See:
- specs/002-backend-api/contracts for Albums and Photos API definitions.
- specs/003-public-site/public-site-template for the Bootstrap 5 theme used by PublicWeb.

## Testing

Test strategy per specs:
- Unit tests for application services and Razor Page models with mocked repositories/cache.
- Integration tests for API behavior through HttpApi.
- E2E smoke tests for public user journeys (home → album → photo).

Run all tests:
````powershell
dotnet test
````

Coverage targets (goal): 80%+ for core business logic.

## Deployment

- Use DbMigrator to apply migrations as a pre-deploy step.
- Host API and PublicWeb together (single process) for lowest latency, or separately for independent scaling.
- Enforce HTTPS and configure logging/monitoring. ABP integrates well with Serilog and Health Checks.

Containerization:
- DbMigrator includes Dockerfiles for containerized migration execution.
- Additional Dockerfiles for API/PublicWeb can be added as the project stabilizes.

## Roadmap

- PublicWeb
  - Complete album/photo pages and caching policies.
  - Pagination, filtering, and SEO metadata.
  - Accessibility pass and Lighthouse optimization.

- Admin Site (future, React)
  - React + TypeScript (Vite), role-based admin for album/photo management.
  - Integrate with ABP Identity/OpenIddict for authentication.
  - Reuse REST endpoints from HttpApi.
  - Add CI/CD workflow and E2E tests (Playwright).

- Observability and Ops
  - Health checks, structured logging, basic dashboards.
  - Optional swap to distributed cache for scale-out.
