# Photos API Specification

## Base URL
```
/api/app/photo
```

## Endpoints

### GET /api/app/photo
Get paginated list of photos with filtering

**Query Parameters:**
- `skipCount`: int (default: 0) - Number of items to skip
- `maxResultCount`: int (default: 10) - Maximum number of items to return
- `sorting`: string (optional) - Sort field (e.g., "displayOrder", "dateTaken", "creationTime") 
- `albumId`: guid (optional) - Filter by album ID
- `location`: string (optional) - Filter by location
- `dateTakenFrom`: datetime (optional) - Filter photos taken after this date
- `dateTakenTo`: datetime (optional) - Filter photos taken before this date

**Response:**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "albumId": "4fb96f75-6828-5673-c4gd-3d074g77bgb7",
      "fileName": "sunset_beach.jpg",
      "filePath": "/uploads/photos/album1/3fa85f64_sunset_beach.jpg",
      "thumbnailPath": "/uploads/photos/album1/thumbnails/3fa85f64_thumb_sunset_beach.jpg",
      "fileSize": 2048576,
      "caption": "Beautiful sunset at the beach",
      "location": "Malibu, California",
      "displayOrder": 1,
      "width": 1920,
      "height": 1080,
      "dateTaken": "2023-10-01T18:30:00Z",
      "creationTime": "2023-10-14T12:00:00Z",
      "albumName": "Nature Photography"
    }
  ],
  "totalCount": 150
}
```

### GET /api/app/photo/random
Get random photos for display

**Query Parameters:**
- `count`: int (default: 6) - Number of random photos to return
- `excludeAlbumId`: guid (optional) - Exclude photos from this album

**Response:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "albumId": "4fb96f75-6828-5673-c4gd-3d074g77bgb7",
    "fileName": "sunset_beach.jpg",
    "filePath": "/uploads/photos/album1/3fa85f64_sunset_beach.jpg", 
    "thumbnailPath": "/uploads/photos/album1/thumbnails/3fa85f64_thumb_sunset_beach.jpg",
    "fileSize": 2048576,
    "caption": "Beautiful sunset at the beach",
    "location": "Malibu, California",
    "displayOrder": 1,
    "width": 1920,
    "height": 1080,
    "dateTaken": "2023-10-01T18:30:00Z",
    "creationTime": "2023-10-14T12:00:00Z",
    "albumName": "Nature Photography"
  }
]
```

### GET /api/app/photo/{id}
Get photo details by ID

**Path Parameters:**
- `id`: guid - Photo ID

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "albumId": "4fb96f75-6828-5673-c4gd-3d074g77bgb7",
  "fileName": "sunset_beach.jpg",
  "filePath": "/uploads/photos/album1/3fa85f64_sunset_beach.jpg",
  "thumbnailPath": "/uploads/photos/album1/thumbnails/3fa85f64_thumb_sunset_beach.jpg",
  "fileSize": 2048576,
  "caption": "Beautiful sunset at the beach",
  "location": "Malibu, California", 
  "displayOrder": 1,
  "width": 1920,
  "height": 1080,
  "dateTaken": "2023-10-01T18:30:00Z",
  "creationTime": "2023-10-14T12:00:00Z",
  "albumName": "Nature Photography"
}
```

### POST /api/app/photo
Upload new photo

**Content-Type:** `multipart/form-data`

**Form Data:**
- `albumId`: guid - Album ID where photo will be added
- `file`: file - Image file to upload
- `caption`: string (optional) - Photo caption
- `location`: string (optional) - Photo location
- `displayOrder`: int (default: 0) - Display order within album

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "albumId": "4fb96f75-6828-5673-c4gd-3d074g77bgb7", 
  "fileName": "sunset_beach.jpg",
  "filePath": "/uploads/photos/album1/3fa85f64_sunset_beach.jpg",
  "thumbnailPath": "/uploads/photos/album1/thumbnails/3fa85f64_thumb_sunset_beach.jpg",
  "fileSize": 2048576,
  "caption": "Beautiful sunset at the beach",
  "location": "Malibu, California",
  "displayOrder": 1,
  "width": 1920,
  "height": 1080,
  "dateTaken": "2023-10-01T18:30:00Z",
  "creationTime": "2023-10-14T12:00:00Z",
  "albumName": "Nature Photography"
}
```

### PUT /api/app/photo/{id}
Update photo metadata

**Path Parameters:**
- `id`: guid - Photo ID

**Request Body:**
```json
{
  "caption": "Stunning sunset at Malibu Beach",
  "location": "Malibu Beach, California"
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "albumId": "4fb96f75-6828-5673-c4gd-3d074g77bgb7",
  "fileName": "sunset_beach.jpg",
  "filePath": "/uploads/photos/album1/3fa85f64_sunset_beach.jpg",
  "thumbnailPath": "/uploads/photos/album1/thumbnails/3fa85f64_thumb_sunset_beach.jpg", 
  "fileSize": 2048576,
  "caption": "Stunning sunset at Malibu Beach",
  "location": "Malibu Beach, California",
  "displayOrder": 1,
  "width": 1920,
  "height": 1080,
  "dateTaken": "2023-10-01T18:30:00Z",
  "creationTime": "2023-10-14T12:00:00Z",
  "albumName": "Nature Photography"
}
```

### DELETE /api/app/photo/{id}
Delete photo and associated files

**Path Parameters:**
- `id`: guid - Photo ID

**Response:**
```
204 No Content
```

### PUT /api/app/photo/reorder
Reorder photos within an album

**Request Body:**
```json
{
  "photoIds": [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "4fb96f75-6828-5673-c4gd-3d074g77bgb7",
    "5gc07g86-7939-6784-d5he-4e185h88chc8"
  ]
}
```

**Response:**
```
200 OK
```

### GET /api/app/photo/by-album/{albumId}
Get photos by album with pagination

**Path Parameters:**
- `albumId`: guid - Album ID

**Query Parameters:**
- `skipCount`: int (default: 0)
- `maxResultCount`: int (default: 10) 
- `sorting`: string (optional)

**Response:**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "albumId": "4fb96f75-6828-5673-c4gd-3d074g77bgb7",
      "fileName": "sunset_beach.jpg",
      "filePath": "/uploads/photos/album1/3fa85f64_sunset_beach.jpg",
      "thumbnailPath": "/uploads/photos/album1/thumbnails/3fa85f64_thumb_sunset_beach.jpg",
      "fileSize": 2048576,
      "caption": "Beautiful sunset at the beach",
      "location": "Malibu, California",
      "displayOrder": 1,
      "width": 1920,
      "height": 1080,
      "dateTaken": "2023-10-01T18:30:00Z",
      "creationTime": "2023-10-14T12:00:00Z",
      "albumName": "Nature Photography"
    }
  ],
  "totalCount": 25
}
```

### GET /api/app/photo/locations
Get list of unique locations

**Query Parameters:**
- `albumId`: guid (optional) - Filter locations by album

**Response:**
```json
[
  "Malibu, California",
  "Yosemite National Park",
  "New York City", 
  "Paris, France",
  "Tokyo, Japan"
]
```

## File Upload Requirements

### Supported File Types
- `.jpg`, `.jpeg` - JPEG images
- `.png` - PNG images  
- `.gif` - GIF images

### File Size Limits
- Maximum file size: 10MB per upload
- Recommended size: Under 5MB for optimal performance

### File Processing
- **Automatic thumbnail generation**: 300x300px thumbnails created
- **EXIF data extraction**: Date taken, GPS coordinates, camera settings
- **File naming**: UUID-based naming to prevent conflicts
- **Storage structure**: `/uploads/photos/{albumId}/original/` and `/uploads/photos/{albumId}/thumbnails/`

### Security Measures
- File content validation (not just extension checking)
- Path traversal prevention
- Virus scanning hooks (configurable)
- File size enforcement at multiple levels

## Error Responses

### 400 Bad Request - Invalid File
```json
{
  "error": {
    "code": "Volo.Abp.Validation.AbpValidationException",
    "message": "File validation failed",
    "details": "File size cannot exceed 10MB",
    "validationErrors": [
      {
        "message": "File size cannot exceed 10MB",
        "members": ["file"]
      }
    ]
  }
}
```

### 400 Bad Request - Unsupported File Type
```json
{
  "error": {
    "code": "Volo.Abp.Validation.AbpValidationException", 
    "message": "File validation failed",
    "details": "File must be one of the following types: .jpg, .jpeg, .png, .gif",
    "validationErrors": [
      {
        "message": "File must be one of the following types: .jpg, .jpeg, .png, .gif",
        "members": ["file"]
      }
    ]
  }
}
```

### 404 Not Found
```json
{
  "error": {
    "code": "Volo.Abp.Domain.Entities.EntityNotFoundException",
    "message": "Photo not found",
    "details": "Photo with ID 3fa85f64-5717-4562-b3fc-2c963f66afa6 was not found"
  }
}
```

### 413 Payload Too Large
```json
{
  "error": {
    "code": "Microsoft.AspNetCore.Http.BadHttpRequestException",
    "message": "Request body too large",
    "details": "The request body is larger than the configured maximum size"
  }
}
```

### 422 Unprocessable Entity - File Processing Error
```json
{
  "error": {
    "code": "PhotoGallery.Photos.PhotoProcessingException",
    "message": "Photo processing failed", 
    "details": "Unable to generate thumbnail for uploaded image"
  }
}
```

## HTTP Status Codes

- `200 OK` - Successful GET, PUT operations
- `201 Created` - Successful POST operations (photo upload)
- `204 No Content` - Successful DELETE operations
- `400 Bad Request` - Validation errors, malformed requests
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Insufficient permissions  
- `404 Not Found` - Resource not found
- `413 Payload Too Large` - File size exceeds server limits
- `422 Unprocessable Entity` - File processing errors
- `500 Internal Server Error` - Server errors

## Performance Considerations

### Caching Headers
- Static photo files should include appropriate cache headers
- Thumbnail responses should be cached for optimal performance
- ETag support for conditional requests

### Async Processing
- Large file uploads are processed asynchronously
- Thumbnail generation happens in background
- Progress tracking for long-running operations

### Pagination
- Default page size: 10 items
- Maximum page size: 100 items
- Cursor-based pagination for large datasets