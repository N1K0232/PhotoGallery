using AutoMapper;
using PhotoGallery.Authentication.Entities;
using PhotoGallery.Shared.Models;
using PhotoGallery.Shared.Models.Requests;

namespace PhotoGallery.BusinessLayer.Mapping;

public class UserMapperProfile : Profile
{
    public UserMapperProfile()
    {
        CreateMap<RegisterRequest, ApplicationUser>();
        CreateMap<ApplicationUser, User>();
    }
}