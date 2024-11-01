using AutoMapper;
using PhotoGallery.Shared.Models;
using Entities = PhotoGallery.DataAccessLayer.Entities;

namespace PhotoGallery.BusinessLayer.Mapping;

public class ImageMapperProfile : Profile
{
    public ImageMapperProfile()
    {
        CreateMap<Entities.Image, Image>();
    }
}