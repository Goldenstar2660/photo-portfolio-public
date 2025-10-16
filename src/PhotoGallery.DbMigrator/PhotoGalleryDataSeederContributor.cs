using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using PhotoGallery.Albums;
using PhotoGallery.Photos;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Amazon.S3;
using Amazon.S3.Model;
using SixLabors.ImageSharp;

namespace PhotoGallery.DbMigrator;

public class PhotoGalleryDataSeederContributor
    : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Album, Guid> _albumRepository;
    private readonly IRepository<Photo, Guid> _photoRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PhotoGalleryDataSeederContributor> _logger;
    private readonly IAmazonS3 _s3Client;

    public PhotoGalleryDataSeederContributor(
        IRepository<Album, Guid> albumRepository,
        IRepository<Photo, Guid> photoRepository,
        IGuidGenerator guidGenerator,
        IConfiguration configuration,
        ILogger<PhotoGalleryDataSeederContributor> logger,
        IAmazonS3 s3Client)
    {
        _albumRepository = albumRepository;
        _photoRepository = photoRepository;
        _guidGenerator = guidGenerator;
        _configuration = configuration;
        _logger = logger;
        _s3Client = s3Client;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await SeedAlbumsAndPhotosAsync();
    }

    private async Task SeedAlbumsAndPhotosAsync()
    {
        // Check if album count is not exactly 6, then delete all albums
        var albumCount = await _albumRepository.GetCountAsync();
        if (albumCount != 6)
        {
            // Delete all albums (this will cascade delete photos due to foreign key)
            var existingAlbums = await _albumRepository.GetListAsync();
            await _albumRepository.DeleteManyAsync(existingAlbums);
            
            // Seed the 6 required albums
            await SeedRequiredAlbumsAsync();
        }
        else
        {
            // Albums exist, check photos
            await HandlePhotoSeeding();
        }
    }

    private async Task SeedRequiredAlbumsAsync()
    {
        _logger.LogInformation("Seeding required albums...");

        // Travel Topic Albums
        var maritimesAlbum = await _albumRepository.InsertAsync(
            new Album(
                _guidGenerator.Create(),
                "Maritimes",
                "Travel",
                "Exploring Canada's eastern coastline including Nova Scotia, New Brunswick, and Prince Edward Island. Capturing the rugged Atlantic coastline, historic fishing villages, and maritime culture.",
                1,
                "/images/albumcover/maritimes-cover.jpg"
            ),
            autoSave: true
        );

        var europeAlbum = await _albumRepository.InsertAsync(
            new Album(
                _guidGenerator.Create(),
                "Europe",
                "Travel",
                "A memorable journey through European cities and countryside. From ancient castles and cathedrals to bustling markets and scenic landscapes across multiple countries.",
                2,
                "/images/albumcover/europe-cover.jpg"
            ),
            autoSave: true
        );

        // FIRST Robotics Topic Albums
        var dcmp2025Album = await _albumRepository.InsertAsync(
            new Album(
                _guidGenerator.Create(),
                "DCMP 2025",
                "FIRST Robotics",
                "District Championship 2025 - Our team's journey through intense competition, strategic gameplay, and collaborative alliance matches at the district championship level.",
                3,
                "/images/albumcover/dcmp2025-cover.jpg"
            ),
            autoSave: true
        );

        var humber2025Album = await _albumRepository.InsertAsync(
            new Album(
                _guidGenerator.Create(),
                "Humber 2025",
                "FIRST Robotics",
                "Humber College Event 2025 - Robot battles, pit preparations, and team spirit at the Humber College competition venue.",
                4,
                "/images/albumcover/humber2025-cover.jpg"
            ),
            autoSave: true
        );

        var mcmaster2025Album = await _albumRepository.InsertAsync(
            new Album(
                _guidGenerator.Create(),
                "McMaster 2025",
                "FIRST Robotics",
                "McMaster University Event 2025 - High-stakes matches, innovative robot designs, and teamwork at McMaster's FIRST Robotics competition.",
                5,
                "/images/albumcover/mcmaster2025-cover.jpg"
            ),
            autoSave: true
        );

        var overtimeSunday2024Album = await _albumRepository.InsertAsync(
            new Album(
                _guidGenerator.Create(),
                "Overtime Sunday 2024",
                "FIRST Robotics",
                "Overtime Sunday Off-Season Event 2024 - Practice matches, skill development, and community building during the off-season competition.",
                6,
                "/images/albumcover/overtime2024-cover.jpg"
            ),
            autoSave: true
        );

        _logger.LogInformation("Albums created successfully");

        // Seed photos for all albums
        await SyncAllPhotosAsync(maritimesAlbum.Id, europeAlbum.Id, dcmp2025Album.Id, humber2025Album.Id, mcmaster2025Album.Id, overtimeSunday2024Album.Id);
    }

    private async Task HandlePhotoSeeding()
    {
        // Get all current albums
        var albums = await _albumRepository.GetListAsync();
        var albumNames = new[] { "Maritimes", "Europe", "DCMP 2025", "Humber 2025", "McMaster 2025", "Overtime Sunday 2024" };
        
        // Check if all 6 required albums exist
        var requiredAlbumsExist = albumNames.All(name => albums.Any(a => a.Name == name));
        
        if (!requiredAlbumsExist)
        {
            // Delete all albums and start fresh
            await _albumRepository.DeleteManyAsync(albums);
            await SeedRequiredAlbumsAsync();
            return;
        }

        // Get album IDs
        var maritimesAlbum = albums.First(a => a.Name == "Maritimes");
        var europeAlbum = albums.First(a => a.Name == "Europe");
        var dcmp2025Album = albums.First(a => a.Name == "DCMP 2025");
        var humber2025Album = albums.First(a => a.Name == "Humber 2025");
        var mcmaster2025Album = albums.First(a => a.Name == "McMaster 2025");
        var overtimeSunday2024Album = albums.First(a => a.Name == "Overtime Sunday 2024");
        
        var validAlbumIds = new[] { maritimesAlbum.Id, europeAlbum.Id, dcmp2025Album.Id, humber2025Album.Id, mcmaster2025Album.Id, overtimeSunday2024Album.Id };

        // Check for photos that don't belong to the required albums
        var allPhotos = await _photoRepository.GetListAsync();
        var orphanedPhotos = allPhotos.Where(p => !validAlbumIds.Any(id => id == p.AlbumId)).ToList();
        
        // Delete orphaned photos only
        if (orphanedPhotos.Any())
        {
            await _photoRepository.DeleteManyAsync(orphanedPhotos);
            _logger.LogInformation("Deleted {Count} orphaned photos", orphanedPhotos.Count);
        }
        
        // Sync photos with R2 objects using optimized approach
        await SyncAllPhotosAsync(maritimesAlbum.Id, europeAlbum.Id, dcmp2025Album.Id, humber2025Album.Id, mcmaster2025Album.Id, overtimeSunday2024Album.Id);
    }

    private async Task SyncAllPhotosAsync(Guid maritimesAlbumId, Guid europeAlbumId, Guid dcmp2025AlbumId, Guid humber2025AlbumId, Guid mcmaster2025AlbumId, Guid overtimeSunday2024AlbumId)
    {
        _logger.LogInformation("Starting optimized photo sync from Cloudflare R2...");

        // Sync photos for Travel albums
        await SyncMaritimesPhotosAsync(maritimesAlbumId);
        await SyncEuropePhotosAsync(europeAlbumId);
        
        // Sync photos for FIRST Robotics albums
        await SyncDCMP2025PhotosAsync(dcmp2025AlbumId);
        await SyncHumber2025PhotosAsync(humber2025AlbumId);
        await SyncMcMaster2025PhotosAsync(mcmaster2025AlbumId);
        await SyncOvertimeSunday2024PhotosAsync(overtimeSunday2024AlbumId);

        _logger.LogInformation("Photo sync completed");
    }

    private async Task SyncMaritimesPhotosAsync(Guid albumId)
    {
        // Locations provided (36 entries) mapped 1:1 to files maritimes_01..NN
        string[] locations = new[]
        {
            "Bouctouche, NB",
            "Bouctouche, NB",
            "Kinkora, PEI",
            "Cape Egmont, PEI",
            "Cape Egmont, PEI",
            "Cape Egmont, PEI",
            "Cape Egmont, PEI",
            "Cape Egmont, PEI",
            "Cape Bear, PEI",
            "Cape Bear, PEI",
            "Cape Bear, PEI",
            "Charlottetown, PEI",
            "Pleasant Bay, NS",
            "Pleasant Bay, NS",
            "Pleasant Bay, NS",
            "Ingonish Beach, NS",
            "Ingonish Beach, NS",
            "Ingonish Beach, NS",
            "Englishtown, NS",
            "Halifax, NS",
            "Halifax, NS",
            "Halifax, NS",
            "Peggy's Cove, NS",
            "Peggy's Cove, NS",
            "Lunenberg, NS",
            "Lunenberg, NS",
            "Lunenberg, NS",
            "Port Joli, NS",
            "Port Joli, NS",
            "Yarmouth, NS",
            "Grand Pre, NS",
            "Grand Pre, NS",
            "Alma, NB",
            "Saint John, NB",
            "Saint John, NB",
            "Hartland, NB"
        };

        await SyncAlbumPhotosAsync(albumId, "photos/maritimes/", "maritimes_", locations, "Maritimes", 30);
    }

    private async Task SyncEuropePhotosAsync(Guid albumId)
    {
        const string location = "London, United Kingdom";
        await SyncAlbumPhotosAsync(albumId, "photos/europe/", "europe_", new[] { location }, location, 60);
    }

    private async Task SyncDCMP2025PhotosAsync(Guid albumId)
    {
        const string location = "Mississauga, Ontario";
        await SyncAlbumPhotosAsync(albumId, "photos/dcmp2025/", "dcmp2025_", new[] { location }, location, 10);
    }

    private async Task SyncHumber2025PhotosAsync(Guid albumId)
    {
        const string location = "Toronto, Ontario";
        await SyncAlbumPhotosAsync(albumId, "photos/humber2025/", "humber2025_", new[] { location }, location, 25);
    }

    private async Task SyncMcMaster2025PhotosAsync(Guid albumId)
    {
        const string location = "Hamilton, Ontario";
        await SyncAlbumPhotosAsync(albumId, "photos/mcmaster2025/", "mcmaster2025_", new[] { location }, location, 40);
    }

    private async Task SyncOvertimeSunday2024PhotosAsync(Guid albumId)
    {
        const string location = "Hamilton, Ontario";
        await SyncAlbumPhotosAsync(albumId, "photos/overtime2024/", "overtime2024_", new[] { location }, location, 180);
    }

    // Main optimized sync method that checks database first
    private async Task SyncAlbumPhotosAsync(Guid albumId, string prefix, string fileNamePrefix, string[] locations, string defaultLocation, int dayOffset)
    {
        // Get existing photos for this album
        var existingPhotos = await _photoRepository.GetListAsync(p => p.AlbumId == albumId);
        var existingPhotosByFileName = existingPhotos.ToDictionary(p => p.FileName, p => p);

        // Get R2 objects
        var objects = await GetR2ObjectsAsync(prefix);
        var albumFiles = objects.Where(o => o.Key.EndsWith(".jpg") && o.Key.Contains(fileNamePrefix))
                              .OrderBy(o => o.Key)
                              .ToList();

        _logger.LogInformation("Found {Count} {Prefix} photos in R2, {ExistingCount} in database", 
            albumFiles.Count, fileNamePrefix, existingPhotos.Count);

        var cdnDomain = GetCdnDomain();
        var newPhotos = 0;
        var updatedPhotos = 0;
        var skippedPhotos = 0;

        for (int i = 0; i < albumFiles.Count; i++)
        {
            var obj = albumFiles[i];
            var fileName = Path.GetFileName(obj.Key);
            var expectedWebPath = $"{cdnDomain}/{obj.Key}";

            // Check if photo already exists in database
            if (existingPhotosByFileName.TryGetValue(fileName, out var existingPhoto))
            {
                // Photo exists - check if FilePath or ThumbnailPath needs updating
                var expectedThumbnailPath = expectedWebPath.Replace("/photos/", "/thumbnails/");
                var needsUpdate = false;
                
                if (existingPhoto.FilePath != expectedWebPath)
                {
                    existingPhoto.FilePath = expectedWebPath;
                    needsUpdate = true;
                }
                
                if (string.IsNullOrEmpty(existingPhoto.ThumbnailPath) || existingPhoto.ThumbnailPath != expectedThumbnailPath)
                {
                    existingPhoto.ThumbnailPath = expectedThumbnailPath;
                    needsUpdate = true;
                }
                
                if (needsUpdate)
                {
                    await _photoRepository.UpdateAsync(existingPhoto);
                    updatedPhotos++;
                    _logger.LogDebug("Updated FilePath and/or ThumbnailPath for {FileName}", fileName);
                }
                else
                {
                    skippedPhotos++;
                }
                continue;
            }

            // Photo doesn't exist - need to create it with R2 metadata
            var (size, width, height, exif) = await GetImageInfoFromR2Async(obj.Key);
            var location = i < locations.Length ? locations[i] : defaultLocation;
            var thumbnailPath = expectedWebPath.Replace("/photos/", "/thumbnails/");
            
            await _photoRepository.InsertAsync(
                new Photo(
                    _guidGenerator.Create(),
                    albumId,
                    fileName,
                    expectedWebPath,
                    size,
                    i + 1,
                    location,
                    thumbnailPath,
                    width,
                    height,
                    exif.DateTaken ?? DateTime.Now.AddDays(-(dayOffset + i)),
                    exif.CameraModel,
                    exif.Aperture,
                    exif.ShutterSpeed,
                    exif.Iso,
                    exif.FocalLength
                ),
                autoSave: true
            );
            newPhotos++;
        }

        // Remove photos from database that no longer exist in R2
        var r2FileNames = albumFiles.Select(o => Path.GetFileName(o.Key)).ToHashSet();
        var photosToDelete = existingPhotos.Where(p => !r2FileNames.Contains(p.FileName)).ToList();
        if (photosToDelete.Any())
        {
            await _photoRepository.DeleteManyAsync(photosToDelete);
            _logger.LogInformation("Deleted {Count} photos that no longer exist in R2 for {Prefix}", 
                photosToDelete.Count, fileNamePrefix);
        }

        _logger.LogInformation("Sync completed for {Prefix}: {New} new, {Updated} updated, {Skipped} skipped, {Deleted} deleted", 
            fileNamePrefix, newPhotos, updatedPhotos, skippedPhotos, photosToDelete.Count);
    }

    // Helper to get CDN domain from configuration
    private string GetCdnDomain()
    {
        var cdnDomain = _configuration["Cloudflare:CdnDomain"];
        if (string.IsNullOrEmpty(cdnDomain))
        {
            // Fallback to default if not configured
            return "https://pub-0660f4bac17949549be4d3bf206751b2.r2.dev";
        }
        return cdnDomain.TrimEnd('/');
    }

    // R2 API Helper methods
    private async Task<List<S3Object>> GetR2ObjectsAsync(string prefix)
    {
        try
        {
            var bucketName = _configuration["Cloudflare:Bucket"];
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = prefix,
                MaxKeys = 1000
            };

            var response = await _s3Client.ListObjectsV2Async(request);
            _logger.LogInformation("Retrieved {Count} objects from R2 with prefix '{Prefix}'", response.S3Objects.Count, prefix);
            
            return response.S3Objects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving objects from R2 with prefix '{Prefix}'", prefix);
            return new List<S3Object>();
        }
    }

    private async Task<(long size, int? width, int? height, ExifData exif)> GetImageInfoFromR2Async(string objectKey)
    {
        try
        {
            var bucketName = _configuration["Cloudflare:Bucket"];
            
            // Get object metadata first
            var metadataRequest = new GetObjectMetadataRequest
            {
                BucketName = bucketName,
                Key = objectKey
            };
            
            var metadataResponse = await _s3Client.GetObjectMetadataAsync(metadataRequest);
            var size = metadataResponse.ContentLength;

            // Download the image to extract dimensions and EXIF data
            var getRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey
            };

            using var response = await _s3Client.GetObjectAsync(getRequest);
            using var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            // Extract dimensions using ImageSharp
            int? width = null, height = null;
            try
            {
                using var image = Image.Load(memoryStream);
                width = image.Width;
                height = image.Height;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract dimensions from {ObjectKey}", objectKey);
            }

            // Extract EXIF data
            memoryStream.Position = 0;
            var exif = ReadExifFromStream(memoryStream);

            return (size, width, height, exif);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting image info from R2 for {ObjectKey}", objectKey);
            return (0L, null, null, new ExifData());
        }
    }

    private ExifData ReadExifFromStream(Stream stream)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(stream);
            DateTime? dateTaken = null;
            string? cameraModel = null;
            double? aperture = null;
            string? shutter = null;
            int? iso = null;
            double? focal = null;

            var exif0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            var exif = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            
            if (exif != null)
            {
                if (exif.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dto) || 
                    exif.TryGetDateTime(ExifDirectoryBase.TagDateTime, out dto))
                {
                    dateTaken = dto;
                }
                
                if (exif.ContainsTag(ExifDirectoryBase.TagFNumber))
                {
                    var f = exif.GetRational(ExifDirectoryBase.TagFNumber);
                    if (f.Denominator != 0) aperture = (double)f.Numerator / f.Denominator;
                }
                
                if (exif.ContainsTag(ExifDirectoryBase.TagExposureTime))
                {
                    var exp = exif.GetRational(ExifDirectoryBase.TagExposureTime);
                    if (exp.Denominator != 0)
                    {
                        var sec = (double)exp.Numerator / exp.Denominator;
                        shutter = sec >= 1 ? $"{sec:0.#} s" : $"1/{Math.Round(1/sec):0} s";
                    }
                }
                
                if (exif.ContainsTag(ExifDirectoryBase.TagIsoEquivalent))
                {
                    iso = exif.GetInt32(ExifDirectoryBase.TagIsoEquivalent);
                }
                
                if (exif.ContainsTag(ExifDirectoryBase.TagFocalLength))
                {
                    var fl = exif.GetRational(ExifDirectoryBase.TagFocalLength);
                    if (fl.Denominator != 0) focal = (double)fl.Numerator / fl.Denominator;
                }
            }
            
            if (exif0 != null && exif0.ContainsTag(ExifDirectoryBase.TagModel))
            {
                cameraModel = exif0.GetDescription(ExifDirectoryBase.TagModel);
            }
            
            return new ExifData
            {
                DateTaken = dateTaken,
                CameraModel = cameraModel,
                Aperture = aperture,
                ShutterSpeed = shutter,
                Iso = iso,
                FocalLength = focal
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract EXIF data from stream");
            return new ExifData();
        }
    }

    internal class ExifData
    {
        public DateTime? DateTaken { get; set; }
        public string? CameraModel { get; set; }
        public double? Aperture { get; set; }
        public string? ShutterSpeed { get; set; }
        public int? Iso { get; set; }
        public double? FocalLength { get; set; }
    }
}