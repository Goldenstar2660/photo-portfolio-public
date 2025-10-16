# Albums API Specification

## Base URL
```
/api/app/album
```

## Endpoints

### GET /api/app/album
Get paginated list of albums

**Query Parameters:**
- `skipCount`: int (default: 0) - Number of items to skip
- `maxResultCount`: int (default: 10) - Maximum number of items to return  
- `sorting`: string (optional) - Sort field (e.g., "name", "displayOrder", "creationTime")

**Response:**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Nature Photography",
      "topic": "Nature", 
      "description": "Beautiful landscapes and wildlife",
      "displayOrder": 1,
      "coverImagePath": "/uploads/photos/album1/cover.jpg",
      "photoCount": 25,
      "creationTime": "2023-10-14T12:00:00Z",
      "lastModificationTime": "2023-10-14T12:00:00Z"
    }
  ],
  "totalCount": 100
}
```

### GET /api/app/album/{id}
Get album details by ID

**Path Parameters:**
- `id`: guid - Album ID

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Nature Photography",
  "topic": "Nature",
  "description": "Beautiful landscapes and wildlife", 
  "displayOrder": 1,
  "coverImagePath": "/uploads/photos/album1/cover.jpg",
  "photoCount": 25,
  "creationTime": "2023-10-14T12:00:00Z",
  "lastModificationTime": "2023-10-14T12:00:00Z"
}
```

### POST /api/app/album
Create new album

**Request Body:**
```json
{
  "name": "Urban Architecture",
  "topic": "Architecture",
  "description": "Modern buildings and structures",
  "displayOrder": 5
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Urban Architecture",
  "topic": "Architecture", 
  "description": "Modern buildings and structures",
  "displayOrder": 5,
  "coverImagePath": null,
  "photoCount": 0,
  "creationTime": "2023-10-14T12:00:00Z",
  "lastModificationTime": null
}
```

### PUT /api/app/album/{id}
Update existing album

**Path Parameters:**
- `id`: guid - Album ID

**Request Body:**
```json
{
  "name": "Urban Architecture Updated",
  "topic": "Modern Architecture",
  "description": "Contemporary buildings and innovative structures"
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Urban Architecture Updated",
  "topic": "Modern Architecture",
  "description": "Contemporary buildings and innovative structures",
  "displayOrder": 5,
  "coverImagePath": null,
  "photoCount": 0,
  "creationTime": "2023-10-14T12:00:00Z",
  "lastModificationTime": "2023-10-14T12:30:00Z"
}
```

### DELETE /api/app/album/{id}
Delete album and all associated photos

**Path Parameters:**
- `id`: guid - Album ID

**Response:**
```
204 No Content
```

### PUT /api/app/album/reorder
Reorder albums by display order

**Request Body:**
```json
{
  "albumIds": [
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

### GET /api/app/album/topics
Get list of unique topics

**Response:**
```json
[
  "Nature",
  "Architecture", 
  "Portrait",
  "Street Photography",
  "Wildlife"
]
```

### GET /api/app/album/by-topic/{topic}
Get albums filtered by topic

**Path Parameters:**
- `topic`: string - Topic name

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
      "name": "City Landscapes",
      "topic": "Architecture",
      "description": "Urban photography collection",
      "displayOrder": 2,
      "coverImagePath": "/uploads/photos/album2/cover.jpg", 
      "photoCount": 18,
      "creationTime": "2023-10-14T12:00:00Z",
      "lastModificationTime": "2023-10-14T12:00:00Z"
    }
  ],
  "totalCount": 5
}
```

### GET /api/app/album/lookup
Get simplified album list for dropdowns

**Response:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Nature Photography",
    "topic": "Nature"
  },
  {
    "id": "4fb96f75-6828-5673-c4gd-3d074g77bgb7", 
    "name": "Urban Architecture",
    "topic": "Architecture"
  }
]
```

### PUT /api/app/album/{id}/cover-image/{photoId}
Set album cover image

**Path Parameters:**
- `id`: guid - Album ID
- `photoId`: guid - Photo ID to use as cover

**Response:**
```
200 OK
```

## Error Responses

### 400 Bad Request
```json
{
  "error": {
    "code": "Volo.Abp.Validation.AbpValidationException",
    "message": "Validation failed",
    "details": "Album name is required",
    "validationErrors": [
      {
        "message": "Album name is required",
        "members": ["name"]
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
    "message": "Album not found",
    "details": "Album with ID 3fa85f64-5717-4562-b3fc-2c963f66afa6 was not found"
  }
}
```

### 409 Conflict
```json
{
  "error": {
    "code": "Volo.Abp.Domain.Entities.DuplicateEntityException",
    "message": "Duplicate album name",
    "details": "An album with the name 'Nature Photography' already exists"
  }
}
```

## HTTP Status Codes

- `200 OK` - Successful GET, PUT operations
- `201 Created` - Successful POST operations  
- `204 No Content` - Successful DELETE operations
- `400 Bad Request` - Validation errors, malformed requests
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Business rule violations (e.g., duplicate names)
- `500 Internal Server Error` - Server errors