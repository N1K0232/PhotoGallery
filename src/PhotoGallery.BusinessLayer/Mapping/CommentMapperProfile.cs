using AutoMapper;
using PhotoGallery.Shared.Models;
using PhotoGallery.Shared.Models.Requests;
using Entities = PhotoGallery.DataAccessLayer.Entities;

namespace PhotoGallery.BusinessLayer.Mapping;

public class CommentMapperProfile : Profile
{
    public CommentMapperProfile()
    {
        CreateMap<Entities.Comment, Comment>();
        CreateMap<NewCommentRequest, Entities.Comment>()
            .ForMember(c => c.SentimentScore, options => options.MapFrom(c => Convert.ToSingle(c.Score)));
    }
}