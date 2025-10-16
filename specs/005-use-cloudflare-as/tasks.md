# Tasks: Use Cloudflare as storage for photos and thumbnails

## Phase 1: Setup
- [X] T001 Create Cloudflare storage configuration in src/PhotoGallery.Application/Photos/PhotoStorageService.cs
- [X] T002 Update environment variable documentation in specs/005-use-cloudflare-as/quickstart.md
- [X] T003 Remove local file storage logic from src/PhotoGallery.Application/Photos/PhotoAppService.cs

## Phase 2: Foundational
- [ ] T004 Implement Cloudflare R2 integration service in src/PhotoGallery.Application/Photos/PhotoStorageService.cs
- [ ] T005 Add CDN caching configuration support in src/PhotoGallery.Application/Photos/PhotoStorageService.cs
- [ ] T006 Add error handling and logging for storage operations in src/PhotoGallery.Application/Photos/PhotoStorageService.cs

## Phase 3: User Story 1 - Upload photo (P1)
- [ ] T007 [US1] Update upload logic to use Cloudflare storage in src/PhotoGallery.Application/Photos/PhotoAppService.cs
- [ ] T008 [US1] Update upload UI to show success/failure in src/PhotoGallery.PublicWeb/Pages/Photos/Upload.cshtml
- [ ] T009 [US1] Add integration test for upload in tests/PhotoGallery.Application.Tests/Photos/PhotoAppServiceTests.cs

## Phase 4: User Story 2 - View thumbnail (P1)
- [ ] T010 [US2] Update gallery page to load thumbnails from Cloudflare CDN in src/PhotoGallery.PublicWeb/Pages/Photos/Gallery.cshtml
- [ ] T011 [US2] Add CDN URL mapping logic in src/PhotoGallery.PublicWeb/Services/PhotoStorageClient.cs
- [ ] T012 [US2] Add integration test for thumbnail loading in tests/PhotoGallery.PublicWeb.Tests/Integration/ThumbnailTests.cs

## Phase 5: User Story 3 - Generate thumbnails on upload (P2)
- [ ] T013 [US3] Implement thumbnail generation and storage in src/PhotoGallery.Application/Photos/PhotoStorageService.cs
- [ ] T014 [US3] Update upload flow to reference thumbnail URLs in src/PhotoGallery.Application/Photos/PhotoAppService.cs
- [ ] T015 [US3] Add integration test for thumbnail generation in tests/PhotoGallery.Application.Tests/Photos/ThumbnailTests.cs

## Final Phase: Polish & Cross-Cutting Concerns
- [ ] T016 Add documentation for migration steps in specs/005-use-cloudflare-as/quickstart.md
- [ ] T017 Add admin instructions for environment variable setup in specs/005-use-cloudflare-as/quickstart.md
- [ ] T018 Review and refactor code for SOLID and DDD compliance in src/PhotoGallery.Application/Photos/
- [ ] T019 Update audit logging for all storage events in src/PhotoGallery.Application/Photos/PhotoStorageService.cs

## Dependencies
- User Story 1 (Upload) must be completed before User Story 2 (View thumbnail)
- User Story 1 must be completed before User Story 3 (Generate thumbnails)
- Foundational tasks must be completed before any user story phases

## Parallel Execution Examples
- T009 [US1] Add integration test for upload and T012 [US2] Add integration test for thumbnail loading can be executed in parallel
- T016 Add documentation for migration steps and T017 Add admin instructions for environment variable setup can be executed in parallel

## Implementation Strategy
- MVP scope: Complete Phase 1, Phase 2, and Phase 3 (User Story 1 - Upload photo)
- Incremental delivery: Each user story phase is independently testable and can be delivered separately
