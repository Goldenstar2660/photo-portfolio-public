using AutoMapper;
using PhotoGallery.Albums;
using PhotoGallery.Photos;
using System;

namespace PhotoGallery;

public class PhotoGalleryApplicationAutoMapperProfile : Profile
{
    public PhotoGalleryApplicationAutoMapperProfile()
    {
        // Album mappings
        CreateMap<Album, AlbumDto>()
            .ForMember(dest => dest.PhotoCount, opt => opt.MapFrom(src => src.Photos != null ? src.Photos.Count : 0));
        CreateMap<CreateAlbumDto, Album>()
            .ConstructUsing((dto, context) => new Album(
                context.Mapper.Map<Guid>(Guid.NewGuid()),
                dto.Name,
                dto.Topic,
                dto.Description,
                dto.DisplayOrder,
                dto.CoverImagePath));
        CreateMap<UpdateAlbumDto, Album>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Photos, opt => opt.Ignore());
        
        // Photo mappings
        CreateMap<Photo, PhotoDto>()
            .ForMember(dest => dest.AlbumName, opt => opt.Ignore()); // Will be set manually in service
        CreateMap<UpdatePhotoDto, Photo>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AlbumId, opt => opt.Ignore())
            .ForMember(dest => dest.FileName, opt => opt.Ignore())
            .ForMember(dest => dest.FilePath, opt => opt.Ignore())
            .ForMember(dest => dest.FileSize, opt => opt.Ignore());
        
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
    }
}
