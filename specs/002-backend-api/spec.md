## 2. Technical Architecture

Follow ABP framework's coding convension to implement backend API, though:
*  don't add validation annotion to domain class
*  use FluentValidation at application layer to validate input
*  If not required, skip the domain manager unless must have

### 2.1 Domain Layer

#### 2.1.1 Entities

**Album Entity:**
```csharp
- Id: Guid (Primary Key)
- Name: string (required, max 100 chars, unique)
- Topic: string (required, max 50 chars)
- Description: string (optional, max 500 chars)
- DisplayOrder: int (auto-assigned)
- CoverImagePath: string (optional, path to cover image)
- Photos: List<Photo> (navigation property)
- CreationTime: DateTime (audit field)
- LastModificationTime: DateTime (audit field)
```

**Photo Entity:**
```csharp
- Id: Guid (Primary Key)
- AlbumId: Guid (Foreign Key)
- FileName: string (required, original filename)
- FilePath: string (required, server path)
- ThumbnailPath: string (optional, thumbnail path)
- FileSize: long (file size in bytes)
- Caption: string (optional, max 200 chars)
- Location: string (optional, max 100 chars)
- DisplayOrder: int (position within album)
- Width: int (optional, image width)
- Height: int (optional, image height)
- DateTaken: DateTime (optional, EXIF data)
- CreationTime: DateTime (audit field)
```
### 2.2 Application Layer

#### 2.2.1 Application Services

**IAlbumAppService:**
- GetListAsync(): Paginated album list with optional sorting (cached in memory)
- GetAsync(): Single album details
- CreateAsync(): Create new album (invalidates cache)
- UpdateAsync(): Update existing album (invalidates cache)
- DeleteAsync(): Delete album and all photos (invalidates cache)
- ReorderAsync(): Update album display order (invalidates cache)
- GetTopicsAsync(): Get list of all unique topics
- GetByTopicAsync(): Get albums filtered by topic

**IPhotoAppService:**
- GetListByAlbumAsync(): Paginated photos for specific album (cached in memory)
- PickRandomAsync(): random pick n photos with all photo information
- GetAsync(): Single photo details
- DeleteAsync(): Delete photo (invalidates cache)
- ReorderAsync(): Update photo display order (invalidates cache)
- UpdateCaptionAsync(): Update photo caption (invalidates cache)


#### 2.2.2 DTOs (Data Transfer Objects)

**AlbumDto, CreateAlbumDto, UpdateAlbumDto:**
- Data transfer objects for album operations
- Input validation attributes
- AutoMapper profiles for entity mapping

**PhotoDto, ReorderPhotosDto:**
- Data transfer objects for photo operations
- File upload handling for new photos

#### 2.2.3 Memory Caching

**Cache Strategy:**
- **IMemoryCache:** In-memory caching for frequently accessed GetList operations
- **Cache Keys:** Composite keys including service name, method parameters, and pagination
- **Expiration:** Configurable TTL (default 5 minutes) with sliding expiration
- **Invalidation:** Automatic cache clearing on Create/Update/Delete operations
- **Cache Size:** Configurable memory limits to prevent excessive usage

**Cached Operations:**
- Album GetListAsync() results with pagination and sorting parameters
- Photo GetListAsync() results filtered by album and search criteria  

**Cache Key Format:**
```
{ServiceName}:GetList:{ParameterHash}:{UserId}
Examples:
- AlbumAppService:GetList:abc123:null (public access)
- PhotoAppService:GetList:def456:user789 (user-specific)
```

### 2.3 Infrastructure Layer

#### 2.3.1 Database Configuration
- **Entity Framework Core** with SQL Server
- **Migrations:** Code-first database migrations
- **Relationships:** One-to-many (Album -> Photos) with cascade delete
- **Indexing:** Optimized queries for album and photo retrieval

#### 2.3.2 File Storage
- **Upload Directory:** `/wwwroot/uploads/photos/{albumId}/`
- **Thumbnail Directory:** `/wwwroot/uploads/photos/{albumId}/thumbnails/`
- **File Naming:** Unique filename generation to prevent conflicts
- **Size Optimization:** Automatic thumbnail generation for performance