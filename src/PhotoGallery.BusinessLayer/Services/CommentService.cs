using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using PhotoGallery.BusinessLayer.Services.Interfaces;
using PhotoGallery.Contracts;
using PhotoGallery.DataAccessLayer;
using PhotoGallery.Shared.Models;
using PhotoGallery.Shared.Models.Requests;
using Entities = PhotoGallery.DataAccessLayer.Entities;

namespace PhotoGallery.BusinessLayer.Services;

public class CommentService : ICommentService
{
    private readonly IApplicationDbContext dbContext;
    private readonly IUserService userService;
    private readonly IMapper mapper;

    public CommentService(IApplicationDbContext dbContext,
        IUserService userService,
        IMapper mapper)
    {
        this.dbContext = dbContext;
        this.userService = userService;
        this.mapper = mapper;
    }

    public async Task<Result> DeleteAsync(Guid imageId, Guid commentId)
    {
        var exists = await dbContext.GetData<Entities.Image>().AnyAsync(i => i.Id == imageId);
        if (!exists)
        {
            return Result.Fail(FailureReasons.ItemNotFound);
        }

        var query = dbContext.GetData<Entities.Comment>();
        var comment = await query.FirstOrDefaultAsync(c => c.ImageId == imageId && c.Id == commentId);

        if (comment is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound);
        }

        await dbContext.DeleteAsync(comment);
        await dbContext.SaveAsync();

        return Result.Ok();
    }

    public async Task<Result<IEnumerable<Comment>>> GetListAsync(Guid imageId)
    {
        var exists = await dbContext.GetData<Entities.Image>().AnyAsync(i => i.Id == imageId);
        if (!exists)
        {
            return Result.Fail(FailureReasons.ItemNotFound);
        }

        var query = dbContext.GetData<Entities.Comment>().Where(c => c.ImageId == imageId);
        var comments = await query.ProjectTo<Comment>(mapper.ConfigurationProvider).ToListAsync();

        return comments;
    }

    public async Task<Result<Comment>> GetAsync(Guid imageId, Guid commentId)
    {
        var query = dbContext.GetData<Entities.Comment>();
        var dbComment = await query.FirstOrDefaultAsync(c => c.ImageId == imageId && c.Id == commentId);

        if (dbComment is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound);
        }

        var comment = mapper.Map<Comment>(dbComment);
        return comment;
    }

    public async Task<Result<Comment>> InsertAsync(Guid imageId, NewCommentRequest request)
    {
        var exists = await dbContext.GetData<Entities.Image>().AnyAsync(i => i.Id == imageId);
        if (!exists)
        {
            return Result.Fail(FailureReasons.ItemNotFound);
        }

        var dbComment = mapper.Map<Entities.Comment>(request);
        dbComment.UserId = await userService.GetIdAsync();
        dbComment.ImageId = imageId;

        await dbContext.InsertAsync(dbComment);
        await dbContext.SaveAsync();

        var comment = mapper.Map<Comment>(dbComment);
        return comment;
    }
}