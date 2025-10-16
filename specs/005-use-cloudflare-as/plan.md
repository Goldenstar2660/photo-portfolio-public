# Implementation Plan: Use Cloudflare as storage for photos and thumbnails

**Branch**: `005-use-cloudflare-as` | **Date**: 2025-10-15 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/005-use-cloudflare-as/spec.md`

## Summary

Replace existing photo and thumbnail management (currently using website folder storage) with Cloudflare R2 or Cloudflare Images as the storage backend. All uploads, retrievals, and thumbnail generation will use Cloudflare. Credentials are provided via environment variables. No dual storage; migration is complete.

## Technical Context

**Language/Version**: C# / .NET 9
**Primary Dependencies**: ABP Framework, Entity Framework Core, AutoMapper, FluentValidation, Cloudflare R2/.NET SDK or S3-compatible SDK
**Storage**: Cloudflare R2 (object storage), no local file storage
**Testing**: xUnit, integration tests for storage, E2E for upload/view flows
**Target Platform**: Azure App Service, somee.com, or compatible .NET hosting
**Project Type**: Web application (ASP.NET Core Razor Pages + ABP backend)
**Performance Goals**: 99% images available within 10s of upload; 95% gallery pages load thumbnails in <1.5s
**Constraints**: No local file storage; credentials via environment; must support CDN caching and error handling
**Scale/Scope**: Up to 100k photos, 10k users, scalable to more

## Constitution Check

All gates pass:
- Clean code, SOLID, DDD, error handling, input validation, encryption, audit logging, test coverage, accessibility, performance, monitoring, documentation, code review, CI/CD enforced
- No local file storage; all storage via Cloudflare
- No dual storage; migration is complete

## Project Structure

### Documentation (this feature)
```
specs/005-use-cloudflare-as/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output
```

### Source Code (repository root)
```
src/
├── PhotoGallery.Domain/Photos/Photo.cs
├── PhotoGallery.Application/Photos/PhotoAppService.cs
├── PhotoGallery.Application/Photos/PhotoStorageService.cs
├── PhotoGallery.PublicWeb/Services/PhotoStorageClient.cs
├── PhotoGallery.PublicWeb/Pages/Photos/
└── ...

tests/
├── PhotoGallery.Application.Tests/Photos/
├── PhotoGallery.PublicWeb.Tests/Integration/
└── ...
```

**Structure Decision**: Extend existing domain/application/public web structure. All photo storage logic moves to Cloudflare integration services. No local file storage remains.

## Complexity Tracking

No constitution violations. No rejected alternatives required.
