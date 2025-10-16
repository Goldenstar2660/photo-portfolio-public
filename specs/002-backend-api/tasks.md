# Tasks: Backend API

**Input**: Design documents from `/specs/002-backend-api/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Derived User Stories

Based on the technical specification, the following user stories are derived:

**US1 (P1)**: Album Management - Administrators can create, read, update, delete, and reorder albums
**US2 (P2)**: Photo Management - Administrators can upload, view, update metadata, delete, and reorder photos within albums  
**US3 (P3)**: Photo File Operations - System can handle file uploads, generate thumbnails, extract EXIF data, and manage file storage
**US4 (P4)**: API Integration - External systems can access albums and photos via REST API with proper validation and error handling
**US5 (P2)**: Memory Caching - GetList operations for Albums and Photos are cached in memory for improved performance

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic ABP Framework structure

- [X] T001 Create Album and Photo domain entities in src/PhotoGallery.Domain/Albums/ and src/PhotoGallery.Domain/Photos/
- [X] T002 [P] Install FluentValidation package in src/PhotoGallery.Application/
- [X] T003 [P] Create permission definitions in src/PhotoGallery.Application.Contracts/Permissions/PhotoGalleryPermissions.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Create Entity Framework configurations in src/PhotoGallery.EntityFrameworkCore/EntityConfigurations/AlbumConfiguration.cs
- [X] T005 [P] Create Entity Framework configurations in src/PhotoGallery.EntityFrameworkCore/EntityConfigurations/PhotoConfiguration.cs
- [X] T006 Update PhotoGalleryDbContext with Album and Photo DbSets in src/PhotoGallery.EntityFrameworkCore/PhotoGalleryDbContext.cs
- [X] T007 Create and run database migration for Albums and Photos tables
- [X] T008 [P] Update AutoMapper profile in src/PhotoGallery.Application/PhotoGalleryApplicationAutoMapperProfile.cs
- [X] T009 [P] Create upload directory structure at src/PhotoGallery.HttpApi.Host/wwwroot/uploads/photos/
- [X] T010 Configure file upload settings in src/PhotoGallery.HttpApi.Host/appsettings.json

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Album Management (Priority: P1) 🎯 MVP

**Goal**: Administrators can perform CRUD operations on photo albums with proper validation and business rules

**Independent Test**: Create, list, update, delete, and reorder albums via API endpoints

### Implementation for User Story 1

- [X] T011 [P] [US1] Create AlbumDto in src/PhotoGallery.Application.Contracts/Albums/AlbumDto.cs
- [X] T012 [P] [US1] Create CreateAlbumDto in src/PhotoGallery.Application.Contracts/Albums/CreateAlbumDto.cs
- [X] T013 [P] [US1] Create UpdateAlbumDto in src/PhotoGallery.Application.Contracts/Albums/UpdateAlbumDto.cs
- [X] T014 [P] [US1] Create ReorderAlbumsDto in src/PhotoGallery.Application.Contracts/Albums/ReorderAlbumsDto.cs
- [X] T015 [P] [US1] Create IAlbumAppService interface in src/PhotoGallery.Application.Contracts/Albums/IAlbumAppService.cs
- [X] T016 [P] [US1] Create AlbumDtoValidator in src/PhotoGallery.Application/Albums/AlbumDtoValidator.cs
- [X] T017 [US1] Implement AlbumAppService with CRUD operations in src/PhotoGallery.Application/Albums/AlbumAppService.cs
- [X] T018 [US1] Create AlbumController with REST endpoints in src/PhotoGallery.HttpApi/Controllers/AlbumController.cs
- [X] T019 [US1] Add authorization attributes to album operations
- [X] T020 [US1] Register album services in module configuration

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Photo Management (Priority: P2)

**Goal**: Administrators can manage photo metadata, view photos, delete photos, and reorder photos within albums

**Independent Test**: Upload photo, update caption/location, reorder photos, delete photo via API endpoints

### Implementation for User Story 2

- [X] T021 [P] [US2] Create PhotoDto in src/PhotoGallery.Application.Contracts/Photos/PhotoDto.cs
- [X] T022 [P] [US2] Create UpdatePhotoDto in src/PhotoGallery.Application.Contracts/Photos/UpdatePhotoDto.cs
- [X] T023 [P] [US2] Create ReorderPhotosDto in src/PhotoGallery.Application.Contracts/Photos/ReorderPhotosDto.cs
- [X] T024 [P] [US2] Create GetPhotosInput in src/PhotoGallery.Application.Contracts/Photos/GetPhotosInput.cs
- [X] T025 [P] [US2] Create IPhotoAppService interface in src/PhotoGallery.Application.Contracts/Photos/IPhotoAppService.cs
- [X] T026 [P] [US2] Create PhotoDtoValidator in src/PhotoGallery.Application/Photos/PhotoDtoValidator.cs
- [X] T027 [US2] Implement PhotoAppService with metadata operations in src/PhotoGallery.Application/Photos/PhotoAppService.cs
- [X] T028 [US2] Create PhotoController with REST endpoints in src/PhotoGallery.HttpApi/Controllers/PhotoController.cs
- [X] T029 [US2] Add authorization attributes to photo operations
- [X] T030 [US2] Implement photo filtering and pagination logic

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Photo File Operations (Priority: P3)

**Goal**: System can handle file uploads, generate thumbnails, extract EXIF data, and manage secure file storage

**Independent Test**: Upload photo file, verify thumbnail generation, verify EXIF extraction, verify secure file naming

### Implementation for User Story 3

- [X] T031 [P] [US3] Create CreatePhotoDto with file upload in src/PhotoGallery.Application.Contracts/Photos/CreatePhotoDto.cs
- [X] T032 [P] [US3] Create IFileUploadService interface in src/PhotoGallery.Application.Contracts/Services/IFileUploadService.cs
- [X] T033 [P] [US3] Install image processing dependencies (System.Drawing.Common, MetadataExtractor)
- [X] T034 [US3] Implement FileUploadService with thumbnail generation in src/PhotoGallery.Application/Services/FileUploadService.cs
- [X] T035 [US3] Add EXIF data extraction functionality to FileUploadService
- [X] T036 [US3] Implement secure file naming and path generation
- [X] T037 [US3] Add file upload validation (size, type, content) to PhotoDtoValidator
- [X] T038 [US3] Update PhotoAppService to handle file uploads and integrate FileUploadService
- [X] T039 [US3] Add file upload endpoint to PhotoController with multipart/form-data support
- [X] T040 [US3] Implement file deletion logic for photo removal

**Checkpoint**: All core functionality should now be implemented and testable

---

## Phase 6: User Story 4 - API Integration & Validation (Priority: P4)

**Goal**: External systems can reliably access albums and photos with comprehensive validation, error handling, and API documentation

**Independent Test**: Access all API endpoints with various inputs, verify validation errors, verify OpenAPI documentation

### Implementation for User Story 4

- [X] T041 [P] [US4] Add comprehensive validation rules to all validators
- [X] T042 [P] [US4] Implement custom exception handling for business rules
- [X] T043 [P] [US4] Add OpenAPI documentation attributes to all controllers
- [X] T044 [US4] Implement GetTopicsAsync functionality in AlbumAppService
- [X] T045 [US4] Implement GetByTopicAsync functionality in AlbumAppService
- [X] T046 [US4] Implement GetRandomPhotosAsync functionality in PhotoAppService
- [X] T047 [US4] Implement GetLocationsAsync functionality in PhotoAppService
- [X] T048 [US4] Add proper HTTP status codes and error responses
- [X] T049 [US4] Configure CORS policy if needed for external access
- [X] T050 [US4] Update API documentation with all endpoints and examples

**Checkpoint**: All user stories should now be independently functional with full API integration

---

## Phase 6.5: User Story 5 - Memory Caching (Priority: P2)

**Goal**: GetList operations for Albums and Photos are cached in memory for improved performance with automatic cache invalidation

**Independent Test**: Measure response times for repeated GetList requests, verify cache hit/miss behavior, test cache invalidation on data changes

### Implementation for User Story 5

- [X] T061 [P] [US5] Create ICacheKeyService interface in src/PhotoGallery.Application.Contracts/Services/ICacheKeyService.cs
- [X] T062 [P] [US5] Implement CacheKeyService in src/PhotoGallery.Application/Services/CacheKeyService.cs
- [X] T063 [P] [US5] Add cache configuration to src/PhotoGallery.HttpApi.Host/appsettings.json
- [X] T064 [US5] Update AlbumAppService GetListAsync to implement memory caching in src/PhotoGallery.Application/Albums/AlbumAppService.cs
- [X] T065 [US5] Update PhotoAppService GetListAsync to implement memory caching in src/PhotoGallery.Application/Photos/PhotoAppService.cs  
- [X] T066 [US5] Add cache invalidation to Album Create/Update/Delete operations in AlbumAppService
- [X] T067 [US5] Add cache invalidation to Photo Create/Update/Delete operations in PhotoAppService
- [X] T068 [P] [US5] Register IMemoryCache and cache services in PhotoGalleryApplicationModule
- [X] T069 [P] [US5] Add cache configuration validation and error handling

**Checkpoint**: Memory caching should be functional with measurable performance improvements and proper cache invalidation

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and production readiness

- [X] T051 [P] Create comprehensive unit tests in test/PhotoGallery.Application.Tests/Albums/AlbumAppServiceTests.cs
- [X] T052 [P] Create comprehensive unit tests in test/PhotoGallery.Application.Tests/Photos/PhotoAppServiceTests.cs
- [X] T053 [P] Create API integration tests in test/PhotoGallery.HttpApi.Tests/Controllers/AlbumControllerTests.cs
- [X] T054 [P] Create API integration tests in test/PhotoGallery.HttpApi.Tests/Controllers/PhotoControllerTests.cs
- [X] T055 [P] Create file upload integration tests in test/PhotoGallery.HttpApi.Tests/Integration/FileUploadTests.cs
- [X] T056 [P] Update sample data seeder in src/PhotoGallery.Domain/Data/PhotoGalleryDataSeederContributor.cs
- [ ] T057 Performance optimization for large album and photo queries
- [ ] T058 [P] Security review and hardening of file upload functionality
- [ ] T059 [P] Add logging for all critical operations
- [ ] T060 [P] Run quickstart.md validation and update if needed
- [X] T070 [P] [US5] Create cache unit tests in test/PhotoGallery.Application.Tests/Services/CacheKeyServiceTests.cs
- [X] T071 [P] [US5] Create cache integration tests in test/PhotoGallery.Application.Tests/Albums/AlbumCacheTests.cs
- [X] T072 [P] [US5] Create cache integration tests in test/PhotoGallery.Application.Tests/Photos/PhotoCacheTests.cs
- [X] T073 [P] [US5] Create cache performance tests to verify response time improvements

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3 → P4)
- **Polish (Phase 7)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - May integrate with US1 but should be independently testable
- **User Story 3 (P3)**: Depends on User Story 2 for photo entities and basic operations
- **User Story 4 (P4)**: Can enhance User Stories 1-3 but should be independently testable
- **User Story 5 (P2)**: Depends on User Stories 1 and 2 being implemented for caching their GetList operations

### Within Each User Story

- DTOs and interfaces before implementations
- Validators before service implementations  
- Services before controllers
- Core implementation before authorization and advanced features
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, User Stories 1 and 2 can start in parallel
- All DTOs and interfaces within a user story marked [P] can run in parallel
- Testing tasks in Polish phase marked [P] can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all DTOs for User Story 1 together:
Task: "Create AlbumDto in src/PhotoGallery.Application.Contracts/Albums/AlbumDto.cs"
Task: "Create CreateAlbumDto in src/PhotoGallery.Application.Contracts/Albums/CreateAlbumDto.cs"
Task: "Create UpdateAlbumDto in src/PhotoGallery.Application.Contracts/Albums/UpdateAlbumDto.cs"
Task: "Create ReorderAlbumsDto in src/PhotoGallery.Application.Contracts/Albums/ReorderAlbumsDto.cs"
Task: "Create IAlbumAppService interface in src/PhotoGallery.Application.Contracts/Albums/IAlbumAppService.cs"
Task: "Create AlbumDtoValidator in src/PhotoGallery.Application/Albums/AlbumDtoValidator.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (Album Management)
4. **STOP and VALIDATE**: Test album CRUD operations independently
5. Deploy/demo basic album management functionality

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP - Album Management!)
3. Add User Story 2 → Test independently → Deploy/Demo (Photo Metadata Management)
4. Add User Story 3 → Test independently → Deploy/Demo (File Upload & Processing)
5. Add User Story 4 → Test independently → Deploy/Demo (Full API Integration)
6. Add User Story 5 → Test independently → Deploy/Demo (Performance with Memory Caching)
7. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Album Management)
   - Developer B: User Story 2 (Photo Management)
   - Developer C: User Story 3 (File Operations) - starts after US2 entities are ready
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Follow ABP Framework conventions throughout implementation
- Use FluentValidation instead of data annotations on domain entities
- Implement async/await patterns for all I/O operations
- Verify file paths match project structure from plan.md
- Stop at any checkpoint to validate story independently
- Album management (US1) provides the minimal viable backend API
- File operations (US3) depend on photo entity structure from US2
- Authorization should be implemented consistently across all user stories
- Memory caching (US5) enhances performance of existing GetList operations without changing API contracts
- Cache invalidation should be automatic and transparent to API consumers