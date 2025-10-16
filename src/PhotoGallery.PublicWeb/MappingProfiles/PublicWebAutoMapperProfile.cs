using AutoMapper;
using PhotoGallery.Albums;
using PhotoGallery.Photos;
using PhotoGallery.PublicWeb.Models;

namespace PhotoGallery.PublicWeb.MappingProfiles;

public class PublicWebAutoMapperProfile : Profile
{
    public PublicWebAutoMapperProfile()
    {
        // Album mappings
        CreateMap<AlbumDto, AlbumViewModel>()
            .ForMember(dest => dest.TotalPhotoCount, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentPage, opt => opt.Ignore())
            .ForMember(dest => dest.PageSize, opt => opt.Ignore())
            .ForMember(dest => dest.Photos, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreationTime));

        CreateMap<AlbumDto, FeaturedAlbumViewModel>()
            .ForMember(dest => dest.CoverPhotoUrl, opt => opt.Ignore())
            .ForMember(dest => dest.PhotoCount, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreationTime));

        CreateMap<AlbumDto, AlbumCardViewModel>()
            .ForMember(dest => dest.CoverPhotoUrl, opt => opt.Ignore())
            .ForMember(dest => dest.PhotoCount, opt => opt.Ignore());

        CreateMap<AlbumDto, AlbumSummaryViewModel>()
            .ForMember(dest => dest.PhotoCount, opt => opt.Ignore());

        // Photo mappings
        CreateMap<PhotoDto, PhotoViewModel>()
            .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.ThumbnailPath))
            .ForMember(dest => dest.FullSizeUrl, opt => opt.MapFrom(src => src.FilePath))
            .ForMember(dest => dest.AlbumName, opt => opt.Ignore())
            .ForMember(dest => dest.FileSizeFormatted, opt => opt.MapFrom(src => FormatFileSize(src.FileSize)))
            .ForMember(dest => dest.FormattedFileSize, opt => opt.MapFrom(src => FormatFileSize(src.FileSize)))
            .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.Width))
            .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height))
            .ForMember(dest => dest.CameraModel, opt => opt.MapFrom(src => src.CameraModel))
            .ForMember(dest => dest.Aperture, opt => opt.MapFrom(src => src.Aperture))
            .ForMember(dest => dest.ShutterSpeed, opt => opt.MapFrom(src => src.ShutterSpeed))
            .ForMember(dest => dest.Iso, opt => opt.MapFrom(src => src.Iso))
            .ForMember(dest => dest.FocalLength, opt => opt.MapFrom(src => src.FocalLength));

        CreateMap<PhotoDto, RandomPhotoViewModel>()
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.FilePath))
            .ForMember(dest => dest.AlbumName, opt => opt.Ignore());
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
