# PhotoGallery Public API Contract

## Base Configuration

**API Base URL**: `https://localhost:44304/api` (development)  
**Content-Type**: `application/json`  
**Authentication**: None (public endpoints)  
**API Version**: v1

## Album Endpoints

### GET /api/albums
Get paginated list of all albums

**Request:**
```http
GET /api/albums?page=1&pageSize=20&sorting=displayOrder
```

**Query Parameters:**
- `page` (optional): Page number, default 1
- `pageSize` (optional): Items per page, default 20, max 100  
- `sorting` (optional): Sort field and direction, default "displayOrder"

**Response:**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Mountain Adventures",
      "topic": "Travel",
      "description": "Beautiful mountain landscapes and hiking trails",
      "displayOrder": 1,
      "coverImagePath": "/uploads/photos/thumb_mountain_cover.jpg",
      "creationTime": "2024-01-15T10:30:00Z",
      "lastModificationTime": "2024-01-20T14:25:00Z"
    }
  ],
  "totalCount": 15,
  "page": 1,
  "pageSize": 20
}
```

### GET /api/albums/{id}
Get specific album details

**Request:**
```http
GET /api/albums/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Mountain Adventures",
  "topic": "Travel",
  "description": "Beautiful mountain landscapes and hiking trails",
  "displayOrder": 1,
  "coverImagePath": "/uploads/photos/thumb_mountain_cover.jpg",
  "creationTime": "2024-01-15T10:30:00Z",
  "lastModificationTime": "2024-01-20T14:25:00Z"
}
```

### GET /api/albums/topics
Get list of unique album topics

**Request:**
```http
GET /api/albums/topics
```

**Response:**
```json
[
  "Travel",
  "Nature",
  "Urban",
  "Portrait",
  "Drone Photography"
]
```

### GET /api/albums/by-topic/{topic}
Get albums filtered by topic

**Request:**
```http
GET /api/albums/by-topic/Travel?page=1&pageSize=10
```

**Response:**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Mountain Adventures",
      "topic": "Travel",
      "description": "Beautiful mountain landscapes and hiking trails",
      "displayOrder": 1,
      "coverImagePath": "/uploads/photos/thumb_mountain_cover.jpg",
      "creationTime": "2024-01-15T10:30:00Z",
      "lastModificationTime": "2024-01-20T14:25:00Z"
    }
  ],
  "totalCount": 5,
  "page": 1,
  "pageSize": 10
}
```

## Photo Endpoints

### GET /api/photos/by-album/{albumId}
Get photos for specific album with pagination

**Request:**
```http
GET /api/photos/by-album/3fa85f64-5717-4562-b3fc-2c963f66afa6?page=1&pageSize=20&location=Denver
```

**Query Parameters:**
- `page` (optional): Page number, default 1
- `pageSize` (optional): Items per page, default 20, max 50
- `location` (optional): Filter by photo location

**Response:**
```json
{
  "items": [
    {
      "id": "7fb85f64-5717-4562-b3fc-2c963f66afa8",
      "albumId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "fileName": "mountain_sunrise.jpg",
      "filePath": "/uploads/photos/mountain_sunrise.jpg",
      "thumbnailPath": "/uploads/photos/thumb_mountain_sunrise.jpg",
      "fileSize": 2048576,
      "caption": "Beautiful sunrise over the Rocky Mountains",
      "location": "Denver, Colorado",
      "displayOrder": 1,
      "dateTaken": "2024-01-15T06:30:00Z",
      "creationTime": "2024-01-15T10:30:00Z"
    }
  ],
  "totalCount": 35,
  "page": 1,
  "pageSize": 20
}
```

### GET /api/photos/random
Get random photos for homepage display

**Request:**
```http
GET /api/photos/random?count=6
```

**Query Parameters:**
- `count` (optional): Number of random photos, default 6, max 20

**Response:**
```json
[
  {
    "id": "7fb85f64-5717-4562-b3fc-2c963f66afa8",
    "albumId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fileName": "mountain_sunrise.jpg",
    "filePath": "/uploads/photos/mountain_sunrise.jpg",
    "thumbnailPath": "/uploads/photos/thumb_mountain_sunrise.jpg",
    "fileSize": 2048576,
    "caption": "Beautiful sunrise over the Rocky Mountains",
    "location": "Denver, Colorado",
    "displayOrder": 1,
    "dateTaken": "2024-01-15T06:30:00Z",
    "creationTime": "2024-01-15T10:30:00Z"
  }
]
```

### GET /api/photos/{id}
Get specific photo details

**Request:**
```http
GET /api/photos/7fb85f64-5717-4562-b3fc-2c963f66afa8
```

**Response:**
```json
{
  "id": "7fb85f64-5717-4562-b3fc-2c963f66afa8",
  "albumId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "mountain_sunrise.jpg",
  "filePath": "/uploads/photos/mountain_sunrise.jpg",
  "thumbnailPath": "/uploads/photos/thumb_mountain_sunrise.jpg",
  "fileSize": 2048576,
  "caption": "Beautiful sunrise over the Rocky Mountains",
  "location": "Denver, Colorado",
  "displayOrder": 1,
  "dateTaken": "2024-01-15T06:30:00Z",
  "creationTime": "2024-01-15T10:30:00Z"
}
```

## Error Response Format

All endpoints return errors in consistent format:

```json
{
  "error": {
    "code": "NotFound",
    "message": "Album with ID 3fa85f64-5717-4562-b3fc-2c963f66afa6 was not found",
    "details": null,
    "validationErrors": [],
    "correlationId": "8b7a9c4d-1e2f-4a5b-9c8d-7e6f5a4b3c2d"
  }
}
```

**Common HTTP Status Codes:**
- `200 OK` - Successful request
- `400 Bad Request` - Invalid parameters
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

## Rate Limiting

**Rate Limits:**
- 1000 requests per hour per IP
- 100 requests per minute per IP
- Headers included in response:
  - `X-RateLimit-Limit`: Request limit
  - `X-RateLimit-Remaining`: Remaining requests
  - `X-RateLimit-Reset`: Reset timestamp

## Caching Headers

**API Response Caching:**
- Albums list: `Cache-Control: public, max-age=900` (15 minutes)
- Album details: `Cache-Control: public, max-age=1800` (30 minutes)
- Photo data: `Cache-Control: public, max-age=3600` (1 hour)
- Random photos: `Cache-Control: public, max-age=300` (5 minutes)

## CORS Configuration

**Allowed Origins:** 
- `https://localhost:*` (development)
- `https://gallery.yourdomain.com` (production)

**Allowed Methods:** `GET, OPTIONS`  
**Allowed Headers:** `Content-Type, Authorization`