# Feature Specification: Use Cloudflare as storage for photos and thumbnails

**Feature Branch**: `007-use-cloudflare-as`  
**Created**: 2025-10-15  
**Status**: Draft  
**Input**: User description: "Add feature to store photos and generated thumbnails in Cloudflare R2 (or Cloudflare Images) as the storage backend; update upload, retrieval, and thumbnail generation flows to use Cloudflare; ensure caching, signed URLs, and configurable CDN settings are supported."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Upload photo (Priority: P1)

As an authenticated user, I want to upload a photo so that it is stored securely and served quickly to viewers.

**Why this priority**: Uploads are core to the product; without storage the gallery cannot function.

**Independent Test**: Upload a sample image via the existing upload UI or API and verify the image appears in the gallery and is retrievable via its public URL.

**Acceptance Scenarios**:

1. **Given** a valid authenticated user and a supported image file, **When** the user uploads the image, **Then** the original image is persisted to Cloudflare storage and a success response is returned.
2. **Given** a completed upload, **When** the user views the gallery or photo page, **Then** the image is served via Cloudflare CDN URL and renders correctly.

---

### User Story 2 - View thumbnail (Priority: P1)

As any site visitor, I want to see thumbnails for photos so pages load fast and users can browse galleries.

**Why this priority**: Browsing performance is critical for UX and business metrics (engagement, retention).

**Independent Test**: Request a gallery page and verify that thumbnails are requested from the Cloudflare-hosted URL and the images are cached by the CDN.

**Acceptance Scenarios**:

1. **Given** photos exist with generated thumbnails, **When** a gallery page is requested, **Then** thumbnails are delivered via Cloudflare CDN URLs and render within the page.

---

### User Story 3 - Generate thumbnails on upload (Priority: P2)

As a system operator, I want thumbnails generated when photos are uploaded so viewers get optimized images without extra processing steps.

**Why this priority**: Improves performance and reduces client-side bandwidth.

**Independent Test**: Upload an image and check that thumbnail variants (e.g., small, medium) are present in Cloudflare storage and accessible.

**Acceptance Scenarios**:

1. **Given** a successful photo upload, **When** the upload process completes, **Then** thumbnail variants are generated and saved to Cloudflare storage and their URLs are referenced by the application.

---

### Edge Cases

- Uploading very large files (e.g., >100 MB): ensure upload is either rejected with a clear error or accepted via a resumable/streaming path. [ASSUMPTION: initial limit 100 MB]
- Network interruption during upload: upload should fail cleanly and be retryable by client.
- Duplicate uploads (same file hash): system may deduplicate or store as separate objects depending on configuration. [ASSUMPTION: deduplication not implemented in v1]
- Loss of Cloudflare service availability: fall back to error page and queue uploads for retry if possible.
- Permission mismatch: private images requested without valid signed URL should return 403/unauthorized.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST store original uploaded photos in Cloudflare storage (R2 or Cloudflare Images) and return a persistent URL or object identifier.
- **FR-002**: System MUST generate and store thumbnail variants (small, medium, large) for each uploaded photo and expose their URLs.
- **FR-003**: System MUST serve images and thumbnails via Cloudflare CDN URLs to leverage caching and global delivery.
- **FR-005**: System MUST provide configuration for CDN caching behavior (TTL, cache-control headers) and for signed URL expiry.
- **FR-006**: System MUST handle upload errors gracefully and surface meaningful error messages to the user (e.g., file too large, unsupported format, network error).
- **FR-007**: System MUST log storage events (upload, delete, generate thumbnail) for auditing and troubleshooting.
- **FR-008**: System MUST provide configuration to set Cloudflare credentials (access key, secret) and bucket/container names via environment variables. Credentials will be securely injected at runtime by the hosting platform (e.g., Azure App Service, somee.com) and not stored in code or database. Changes require redeployment or platform configuration update.
- **FR-009**: System SHOULD provide optional deduplication by comparing file hashes to avoid storing duplicate objects. (Optional, P3)

### Key Entities *(include if feature involves data)*

- **Photo**: Represents an uploaded image. Key attributes: id, filename, uploadTimestamp, uploaderId, storageObjectKey, publicUrl, isPrivate, thumbnails (list of objects with size and url), metadata (width, height, mimeType, fileSize).
- **Thumbnail**: Represents a generated variant. Key attributes: photoId, sizeLabel (small/medium/large), storageObjectKey, url, width, height.
- **StorageConfig**: Represents Cloudflare storage configuration. Key attributes: providerType (R2|Images), bucketName, accountId, accessKeyRef, region, cdnSettings (defaultTTL, cacheControlHeaders), signedUrlSettings (defaultExpirySeconds).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 99% of uploaded images are available via their Cloudflare-hosted URLs within 10 seconds of upload completion.
- **SC-002**: 95% of gallery pages load thumbnails (first meaningful paint for images) within 1.5 seconds on a typical broadband connection.
- **SC-004**: Storage and thumbnail generation errors are surfaced to users with actionable messages in at least 95% of failure scenarios.
- **SC-005**: Configuration changes to CDN TTL or signed URL expiry take effect within the next applied deployment or configuration reload.

## Assumptions

- Cloudflare R2 or Cloudflare Images will be used as the storage provider; initial rollout can target R2 for object storage and optionally Cloudflare Images for on-the-fly transformations.
- Existing upload UI and background processing pipeline can be extended to integrate with Cloudflare storage.
- Credentials for Cloudflare will be provisioned by platform administrators.

## Open Questions / Clarifications

- Credentials will be provided via environment variables set in the hosting platform (e.g., Azure Portal, somee.com dashboard). This approach avoids storing secrets in code or database and leverages platform security features. Updates require configuration changes and redeployment if needed.

## Clarifications
### Session 2025-10-15
- Q: Does this enhancement replace the existing photo management that uses the website folder as storage? → A: Yes, this enhancement replaces the current website folder storage with Cloudflare storage for all photo and thumbnail management.
- Add explicit out-of-scope declaration: This feature does not support dual storage (website folder + Cloudflare); all photo and thumbnail management will be migrated to Cloudflare storage only.
