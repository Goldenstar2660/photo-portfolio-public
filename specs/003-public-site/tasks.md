# Tasks: Photo Gallery Public Website

**Input**: Design documents from `/specs/003-public-site/`
**Prerequisites**: plan.md, spec.md, data-model.md, contracts/api-specification.md, quickstart.md

**Implementation Approach**: .NET 9 Razor Pages with direct ABP Application Service integration
**Template Base**: Bootstrap 5 CHEFER template adaptation
**Architecture**: Direct service references for optimal performance (no HTTP layer)

## Format: `[ID] [P?] [Story] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions
- **Projects**: `src/PhotoGallery.PublicWeb/` (new Razor Pages project)
- **Tests**: `tests/PhotoGallery.PublicWeb.Tests/`
- **Backend**: `src/PhotoGallery.Application/` (existing, reference only)

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure for the Razor Pages application

- [X] T001 Create PhotoGallery.PublicWeb project structure in src/PhotoGallery.PublicWeb/
- [X] T002 Add direct project references to PhotoGallery.Application and PhotoGallery.EntityFrameworkCore in src/PhotoGallery.PublicWeb/PhotoGallery.PublicWeb.csproj
- [X] T003 [P] Install NuGet packages (AutoMapper, IMemoryCache) in src/PhotoGallery.PublicWeb/PhotoGallery.PublicWeb.csproj
- [X] T004 [P] Copy Bootstrap 5 CHEFER template assets to src/PhotoGallery.PublicWeb/wwwroot/
- [X] T005 Create folder structure (Pages/, Models/, Services/) in src/PhotoGallery.PublicWeb/
- [X] T006 Add project to solution file PhotoGallery.sln

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T007 Configure dependency injection for ABP modules in src/PhotoGallery.PublicWeb/Program.cs
- [X] T008 Create shared layout _Layout.cshtml in src/PhotoGallery.PublicWeb/Pages/Shared/
- [X] T009 [P] Create _ViewStart.cshtml and _ViewImports.cshtml in src/PhotoGallery.PublicWeb/Pages/
- [X] T010 [P] Configure AutoMapper profiles in src/PhotoGallery.PublicWeb/MappingProfiles/
- [X] T011 Implement memory caching configuration in src/PhotoGallery.PublicWeb/Program.cs
- [X] T012 [P] Create navigation partial _NavigationPartial.cshtml in src/PhotoGallery.PublicWeb/Pages/Shared/
- [X] T013 [P] Create error pages (404, 500) in src/PhotoGallery.PublicWeb/Pages/
- [X] T014 Create base PersonalInfoViewModel in src/PhotoGallery.PublicWeb/Models/PersonalInfoViewModel.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Browse Photo Albums (Priority: P1) 🎯 MVP

**Goal**: Visitors can discover and browse through organized photo collections, view home page with featured albums, navigate to album pages, and see photos in a tiled layout

**Independent Test**: Navigate to website, view home page with featured albums and hero image, click Albums dropdown to see all albums, select an album to view photos in responsive grid layout

### Implementation for User Story 1

- [X] T015 [P] [US1] Create HomeViewModel in src/PhotoGallery.PublicWeb/Models/HomeViewModel.cs
- [X] T016 [P] [US1] Create FeaturedAlbumViewModel in src/PhotoGallery.PublicWeb/Models/FeaturedAlbumViewModel.cs
- [X] T017 [P] [US1] Create RandomPhotoViewModel in src/PhotoGallery.PublicWeb/Models/RandomPhotoViewModel.cs
- [X] T018 [US1] Create IHomePageService interface in src/PhotoGallery.PublicWeb/Services/IHomePageService.cs
- [X] T019 [US1] Implement HomePageService with album/photo aggregation in src/PhotoGallery.PublicWeb/Services/HomePageService.cs
- [X] T020 [US1] Create Index.cshtml Razor Page in src/PhotoGallery.PublicWeb/Pages/Index.cshtml
- [X] T021 [US1] Implement IndexModel page model in src/PhotoGallery.PublicWeb/Pages/Index.cshtml.cs
- [X] T022 [US1] Style home page with hero section and featured albums in src/PhotoGallery.PublicWeb/wwwroot/css/site.css
- [X] T023 [P] [US1] Create AlbumViewModel in src/PhotoGallery.PublicWeb/Models/AlbumViewModel.cs
- [X] T024 [P] [US1] Create PhotoViewModel in src/PhotoGallery.PublicWeb/Models/PhotoViewModel.cs
- [X] T025 [US1] Create IAlbumPageService interface in src/PhotoGallery.PublicWeb/Services/IAlbumPageService.cs
- [X] T026 [US1] Implement AlbumPageService with photo retrieval in src/PhotoGallery.PublicWeb/Services/AlbumPageService.cs
- [X] T027 [US1] Create Album.cshtml Razor Page in src/PhotoGallery.PublicWeb/Pages/Album.cshtml
- [X] T028 [US1] Implement AlbumModel page model with album ID parameter in src/PhotoGallery.PublicWeb/Pages/Album.cshtml.cs
- [X] T029 [US1] Implement responsive photo grid layout in src/PhotoGallery.PublicWeb/Pages/Album.cshtml
- [X] T030 [US1] Add lazy loading for photos in src/PhotoGallery.PublicWeb/wwwroot/js/photo-grid.js
- [X] T031 [US1] Style photo grid with responsive columns in src/PhotoGallery.PublicWeb/wwwroot/css/album.css
- [X] T032 [US1] Implement Albums dropdown in navigation in src/PhotoGallery.PublicWeb/Pages/Shared/_NavigationPartial.cshtml
- [X] T033 [US1] Create NavigationViewModel with album topics in src/PhotoGallery.PublicWeb/Models/NavigationViewModel.cs
- [X] T034 [US1] Add caching for album and photo data (15min for albums, 5min for photos) in src/PhotoGallery.PublicWeb/Services/AlbumPageService.cs
- [X] T035 [US1] Handle empty album state gracefully in src/PhotoGallery.PublicWeb/Pages/Album.cshtml
- [X] T036 [US1] Implement responsive mobile menu (hamburger) in src/PhotoGallery.PublicWeb/wwwroot/js/navigation.js

**Checkpoint**: At this point, User Story 1 should be fully functional - visitors can browse home page, navigate albums dropdown, and view album photos in grid layout

---

## Phase 4: User Story 4 - Navigate Site Efficiently (Priority: P2)

**Goal**: Visitors can easily navigate between sections using a consistent navigation menu with Home, About, and Albums dropdown

**Independent Test**: Click through navigation menu from any page, verify menu remains accessible, test mobile hamburger menu, verify current page highlighting

**Note**: US4 comes before US2/US3 because navigation is foundational for all user journeys

### Implementation for User Story 4

- [X] T037 [US4] Enhance _NavigationPartial.cshtml with current page highlighting in src/PhotoGallery.PublicWeb/Pages/Shared/_NavigationPartial.cshtml
- [X] T038 [US4] Implement mobile responsive navigation styles in src/PhotoGallery.PublicWeb/wwwroot/css/navigation.css
- [X] T039 [US4] Add JavaScript for mobile menu toggle in src/PhotoGallery.PublicWeb/wwwroot/js/navigation.js
- [X] T040 [US4] Create navigation service with caching (30min) in src/PhotoGallery.PublicWeb/Services/NavigationService.cs
- [X] T041 [US4] Add breadcrumb navigation component in src/PhotoGallery.PublicWeb/Pages/Shared/_BreadcrumbPartial.cshtml
- [X] T042 [US4] Style navigation for mobile devices (<768px) in src/PhotoGallery.PublicWeb/wwwroot/css/navigation.css
- [X] T043 [US4] Test navigation accessibility (keyboard navigation) in src/PhotoGallery.PublicWeb/Pages/Shared/_NavigationPartial.cshtml

**Checkpoint**: Navigation system complete - visitors can efficiently navigate all sections on desktop and mobile

---

## Phase 5: User Story 2 - View Photo Details (Priority: P2)

**Goal**: Visitors can interact with photos to see them enlarged in lightbox with metadata (date, location, caption), navigate between photos, and use keyboard controls

**Independent Test**: Open any album page, hover over photo to see metadata overlay, click photo to open lightbox, navigate with keyboard arrows, close with Esc or click outside

### Implementation for User Story 2

- [x] T044 [P] [US2] Create PhotoDetailViewModel in src/PhotoGallery.PublicWeb/Models/PhotoDetailViewModel.cs
- [x] T045 [US2] Add photo metadata overlay on hover in src/PhotoGallery.PublicWeb/Pages/Album.cshtml
- [x] T046 [US2] Style metadata overlay in src/PhotoGallery.PublicWeb/wwwroot/css/photo-overlay.css
- [x] T047 [US2] Create lightbox modal component in src/PhotoGallery.PublicWeb/Pages/Shared/_PhotoLightboxPartial.cshtml
- [x] T048 [US2] Implement lightbox JavaScript with navigation in src/PhotoGallery.PublicWeb/wwwroot/js/lightbox.js
- [x] T049 [US2] Add keyboard navigation support (arrows, Esc) in src/PhotoGallery.PublicWeb/wwwroot/js/lightbox.js
- [x] T050 [US2] Display photo caption, date, and location in lightbox in src/PhotoGallery.PublicWeb/Pages/Shared/_PhotoLightboxPartial.cshtml
- [x] T051 [US2] Style lightbox modal with responsive design in src/PhotoGallery.PublicWeb/wwwroot/css/lightbox.css
- [x] T052 [US2] Implement next/previous photo navigation in lightbox in src/PhotoGallery.PublicWeb/wwwroot/js/lightbox.js
- [x] T053 [US2] Handle missing metadata gracefully (hide empty fields) in src/PhotoGallery.PublicWeb/Pages/Shared/_PhotoLightboxPartial.cshtml
- [x] T054 [US2] Add loading indicator for full-resolution images in src/PhotoGallery.PublicWeb/wwwroot/js/lightbox.js
- [x] T055 [US2] Optimize lightbox for mobile touch gestures in src/PhotoGallery.PublicWeb/wwwroot/js/lightbox.js

**Checkpoint**: Photo detail viewing complete - visitors can see enlarged photos with metadata and navigate between photos

---

## Phase 6: User Story 3 - Learn About the Photographer (Priority: P3)

**Goal**: Visitors can access About page to learn about photographer's background, hobbies (biking, photography), and personal story

**Independent Test**: Click About link in navigation, view page with photographer's profile photo, name, and biography about biking and photography hobbies

### Implementation for User Story 3

- [x] T056 [P] [US3] Create AboutViewModel in src/PhotoGallery.PublicWeb/Models/AboutViewModel.cs
- [x] T057 [P] [US3] Create HobbyViewModel in src/PhotoGallery.PublicWeb/Models/HobbyViewModel.cs
- [x] T058 [US3] Create About.cshtml Razor Page in src/PhotoGallery.PublicWeb/Pages/About.cshtml
- [x] T059 [US3] Implement AboutModel page model in src/PhotoGallery.PublicWeb/Pages/About.cshtml.cs
- [x] T060 [US3] Design About page layout with profile section in src/PhotoGallery.PublicWeb/Pages/About.cshtml
- [x] T061 [US3] Add biography content configuration in src/PhotoGallery.PublicWeb/appsettings.json
- [x] T062 [US3] Style About page to match site aesthetic in src/PhotoGallery.PublicWeb/wwwroot/css/about.css
- [x] T063 [US3] Add hobbies section (biking, photography) in src/PhotoGallery.PublicWeb/Pages/About.cshtml
- [x] T064 [US3] Make About content editable via configuration in src/PhotoGallery.PublicWeb/appsettings.json

**Checkpoint**: About page complete - visitors can learn about photographer's background and interests

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and overall site quality

### Performance Optimization

- [ ] T065 [P] Implement image optimization for thumbnails in src/PhotoGallery.PublicWeb/Services/ImageOptimizationService.cs
- [ ] T066 [P] Add browser caching headers for static assets in src/PhotoGallery.PublicWeb/Program.cs
- [ ] T067 Configure pagination for large albums (50+ photos) in src/PhotoGallery.PublicWeb/Services/AlbumPageService.cs
- [ ] T068 Optimize memory cache eviction policies in src/PhotoGallery.PublicWeb/Program.cs
- [ ] T069 [P] Add health checks for application monitoring in src/PhotoGallery.PublicWeb/Program.cs

### SEO & Accessibility

- [ ] T070 [P] Add dynamic page titles and meta descriptions in src/PhotoGallery.PublicWeb/Pages/Shared/_Layout.cshtml
- [ ] T071 [P] Implement Open Graph tags for social sharing in src/PhotoGallery.PublicWeb/Pages/Shared/_Layout.cshtml
- [ ] T072 [P] Add JSON-LD structured data for albums in src/PhotoGallery.PublicWeb/Pages/Album.cshtml
- [ ] T073 [P] Ensure all images have alt text in src/PhotoGallery.PublicWeb/Pages/
- [ ] T074 [P] Add ARIA labels for interactive elements in src/PhotoGallery.PublicWeb/Pages/Shared/
- [ ] T075 [P] Verify WCAG 2.1 AA contrast ratios in src/PhotoGallery.PublicWeb/wwwroot/css/
- [ ] T076 Generate XML sitemap for search engines in src/PhotoGallery.PublicWeb/Services/SitemapService.cs

### Error Handling & Logging

- [ ] T077 Implement global exception handling in src/PhotoGallery.PublicWeb/Program.cs
- [ ] T078 [P] Create user-friendly error messages in src/PhotoGallery.PublicWeb/Pages/Error.cshtml
- [ ] T079 [P] Add structured logging for service calls in src/PhotoGallery.PublicWeb/Services/
- [ ] T080 [P] Configure logging levels and sinks in src/PhotoGallery.PublicWeb/appsettings.json
- [ ] T081 Add telemetry for performance monitoring in src/PhotoGallery.PublicWeb/Program.cs

### Security

- [ ] T082 [P] Enforce HTTPS in production in src/PhotoGallery.PublicWeb/Program.cs
- [ ] T083 [P] Add security headers (CSP, X-Frame-Options) in src/PhotoGallery.PublicWeb/Program.cs
- [ ] T084 [P] Implement input validation for query parameters in src/PhotoGallery.PublicWeb/Pages/
- [ ] T085 Configure rate limiting for API calls in src/PhotoGallery.PublicWeb/Program.cs

### Testing & Quality

- [ ] T086 Create PhotoGallery.PublicWeb.Tests project in tests/PhotoGallery.PublicWeb.Tests/
- [ ] T087 [P] Write unit tests for HomePageService in tests/PhotoGallery.PublicWeb.Tests/Unit/Services/HomePageServiceTests.cs
- [ ] T088 [P] Write unit tests for AlbumPageService in tests/PhotoGallery.PublicWeb.Tests/Unit/Services/AlbumPageServiceTests.cs
- [ ] T089 [P] Write integration tests for Index page in tests/PhotoGallery.PublicWeb.Tests/Integration/IndexPageTests.cs
- [ ] T090 [P] Write integration tests for Album page in tests/PhotoGallery.PublicWeb.Tests/Integration/AlbumPageTests.cs
- [ ] T091 [P] Write E2E tests for home page to album navigation in tests/PhotoGallery.PublicWeb.Tests/E2E/NavigationTests.cs
- [ ] T092 [P] Write E2E tests for photo viewing journey in tests/PhotoGallery.PublicWeb.Tests/E2E/PhotoViewingTests.cs
- [ ] T093 Validate page load time performance (<3s) in tests/PhotoGallery.PublicWeb.Tests/Performance/PageLoadTests.cs

### Documentation & Deployment

- [ ] T094 [P] Update README.md with setup instructions in src/PhotoGallery.PublicWeb/README.md
- [ ] T095 [P] Create deployment guide in specs/003-public-site/deployment.md
- [ ] T096 [P] Document configuration options in specs/003-public-site/configuration.md
- [ ] T097 Create Docker configuration in src/PhotoGallery.PublicWeb/Dockerfile
- [ ] T098 Run quickstart.md validation in specs/003-public-site/quickstart.md

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phases 3-6)**: All depend on Foundational phase completion
  - User Story 1 (P1): Can start after Foundational - No dependencies on other stories
  - User Story 4 (P2): Can start after Foundational - Enhances navigation from US1
  - User Story 2 (P2): Can start after US1 complete - Builds on album display
  - User Story 3 (P3): Can start after Foundational - Independent of other stories
- **Polish (Phase 7)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - Core browsing functionality, no dependencies on other stories
- **User Story 4 (P2)**: Can start after Foundational (Phase 2) - Navigation enhancement, works with US1 but independently testable
- **User Story 2 (P2)**: Recommended after US1 - Builds on album pages with lightbox, independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Completely independent static page

### Within Each User Story

- ViewModels before services (models are independent)
- Services before page models (services provide data)
- Page models before UI implementation
- CSS/JS can be developed in parallel with implementation
- Tests (if added) should be written alongside implementation

### Parallel Opportunities

**Phase 1 (Setup):**
- T003 (NuGet packages) || T004 (Template assets) || T005 (Folder structure)

**Phase 2 (Foundational):**
- T009 (ViewStart/Imports) || T010 (AutoMapper) || T012 (Navigation partial) || T013 (Error pages) || T014 (PersonalInfoViewModel)

**Phase 3 (User Story 1):**
- T015 (HomeViewModel) || T016 (FeaturedAlbumViewModel) || T017 (RandomPhotoViewModel)
- T023 (AlbumViewModel) || T024 (PhotoViewModel)

**Phase 4 (User Story 4):**
- T038 (Navigation CSS) can run with T037 (Navigation partial enhancement)

**Phase 5 (User Story 2):**
- T044 (PhotoDetailViewModel) can start immediately
- T045-T046 (Metadata overlay) || T047 (Lightbox component)

**Phase 6 (User Story 3):**
- T056 (AboutViewModel) || T057 (HobbyViewModel)

**Phase 7 (Polish):**
- All T065-T085 can run in parallel (different files, no dependencies)
- All T086-T093 (Tests) can run in parallel
- All T094-T098 (Documentation) can run in parallel

---

## Parallel Execution Examples

### User Story 1 - Parallel Tasks

```bash
# Start together - different view models:
T015: Create HomeViewModel
T016: Create FeaturedAlbumViewModel  
T017: Create RandomPhotoViewModel

# Start together - different entities:
T023: Create AlbumViewModel
T024: Create PhotoViewModel
```

### User Story 2 - Parallel Tasks

```bash
# Start together - different components:
T045: Add photo metadata overlay (Album.cshtml)
T047: Create lightbox modal component (_PhotoLightboxPartial.cshtml)
T046: Style metadata overlay (photo-overlay.css)
```

### Polish Phase - Parallel Tasks

```bash
# All performance tasks can run in parallel:
T065: Implement image optimization
T066: Add browser caching headers
T069: Add health checks

# All SEO tasks can run in parallel:
T070: Add dynamic page titles
T071: Implement Open Graph tags
T072: Add JSON-LD structured data
T073: Ensure alt text
T074: Add ARIA labels
T075: Verify contrast ratios

# All test tasks can run in parallel:
T087: Unit tests for HomePageService
T088: Unit tests for AlbumPageService
T089: Integration tests for Index page
T090: Integration tests for Album page
T091-T093: E2E and performance tests
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. ✅ Complete Phase 1: Setup (T001-T006)
2. ✅ Complete Phase 2: Foundational (T007-T014) - CRITICAL BLOCKER
3. ✅ Complete Phase 3: User Story 1 (T015-T036) - Browse Photo Albums
4. **STOP and VALIDATE**: 
   - Test home page with featured albums
   - Test Albums dropdown navigation
   - Test album page with photo grid
   - Test responsive layout on mobile
5. Deploy/demo MVP if ready

**MVP Delivered**: Visitors can browse home page, navigate to albums, and view photos in grid layout

### Incremental Delivery

1. **Foundation** (Setup + Foundational) → T001-T014 complete
2. **MVP** (+ User Story 1) → T015-T036 complete
   - Deploy/Demo: Core browsing functionality working
3. **Enhanced Navigation** (+ User Story 4) → T037-T043 complete
   - Deploy/Demo: Professional navigation with mobile support
4. **Rich Photo Viewing** (+ User Story 2) → T044-T055 complete
   - Deploy/Demo: Lightbox viewing with metadata
5. **Personal Touch** (+ User Story 3) → T056-T064 complete
   - Deploy/Demo: Complete public site with About page
6. **Production Ready** (+ Polish) → T065-T098 complete
   - Deploy/Demo: Optimized, tested, production-ready site

**Each increment adds value without breaking previous features**

### Parallel Team Strategy

With multiple developers after Foundational phase complete:

**Team Structure A** (3 developers):
- Developer A: User Story 1 (T015-T036) - Core browsing
- Developer B: User Story 3 (T056-T064) - About page (independent)
- Developer C: User Story 4 (T037-T043) - Navigation enhancement

**Team Structure B** (2 developers):
- Developer A: User Story 1 (T015-T036), then User Story 2 (T044-T055)
- Developer B: User Story 3 (T056-T064), then User Story 4 (T037-T043)

**Integration Point**: After all user stories complete, both teams merge and tackle Phase 7 (Polish) together

---

## Success Criteria

### Technical Requirements

- [x] **Architecture**: Direct ABP service integration specified
- [ ] **Performance**: Page load < 3s (SC-002, SC-003)
- [ ] **Responsive**: Mobile-first design 320px-2560px (FR-029, SC-005)
- [ ] **Caching**: 15min albums, 5min photos (FR-031)
- [ ] **Lazy Loading**: Photos below fold (FR-018)
- [ ] **Testing**: Unit + Integration + E2E coverage

### User Experience Requirements

- [ ] **Navigation**: Within 3 clicks to any album (SC-001)
- [ ] **Lightbox**: Opens in < 1s (SC-004)
- [ ] **Metadata**: Visible on hover without clicks (SC-012)
- [ ] **Mobile**: No pinch-zooming required (SC-013)
- [ ] **Accessibility**: WCAG 2.1 AA compliance (SC-014, FR-028)
- [ ] **Visual Feedback**: < 100ms for interactions (SC-010)

### User Story Completion

- [ ] **US1 - Browse Albums**: Home page, featured albums, album grid, responsive layout
- [ ] **US2 - View Details**: Hover metadata, lightbox, keyboard navigation, next/prev photos
- [ ] **US3 - About Photographer**: Profile, biography, hobbies (biking, photography)
- [ ] **US4 - Navigate Efficiently**: Persistent menu, Albums dropdown, mobile hamburger, breadcrumbs

### Edge Cases Handled

- [ ] Empty albums display message (Edge Case 1)
- [ ] Missing metadata gracefully hidden (Edge Case 2)
- [ ] Photo load failures show placeholder (Edge Case 3)
- [ ] Large albums (100+) use pagination/lazy loading (Edge Case 4)
- [ ] No featured albums fallback to recent (Edge Case 5)
- [ ] Slow connections show thumbnails first (Edge Case 6)
- [ ] Non-existent albums show 404 page (Edge Case 7)

---

## Task Summary

**Total Tasks**: 98 tasks (T001-T098)

**By Phase**:
- Phase 1 (Setup): 6 tasks
- Phase 2 (Foundational): 8 tasks
- Phase 3 (User Story 1): 22 tasks
- Phase 4 (User Story 4): 7 tasks
- Phase 5 (User Story 2): 12 tasks
- Phase 6 (User Story 3): 9 tasks
- Phase 7 (Polish): 34 tasks

**By User Story**:
- US1 (Browse Albums): 22 tasks
- US2 (View Details): 12 tasks
- US3 (About Photographer): 9 tasks
- US4 (Navigate Efficiently): 7 tasks
- Infrastructure: 14 tasks (Setup + Foundational)
- Cross-cutting: 34 tasks (Polish)

**Parallel Opportunities**: 48 tasks marked [P] can run in parallel with others

**Format Validation**: ✅ All tasks follow checklist format with ID, [P] marker (where applicable), [Story] label (for user stories), and file paths

---

## Notes

- **[P] tasks**: Different files, no dependencies - can run in parallel
- **[Story] labels**: Maps tasks to user stories for traceability
- **Checkpoints**: Stop and validate after each user story phase
- **MVP**: User Story 1 alone is a viable MVP
- **Tests**: Include T086-T093 for comprehensive test coverage
- **Documentation**: T094-T098 provide deployment and configuration guides

**REMEMBER**: Each user story should be independently completable and testable. Commit frequently. Stop at checkpoints to validate before proceeding.

---

**Document Status**: Ready for Implementation
**Last Updated**: October 14, 2025
**Based On**: specs/003-public-site/spec.md (updated October 14, 2025)
**Next Step**: Begin Phase 1 (Setup) - T001