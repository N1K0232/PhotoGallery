using OperationResults;
using PhotoGallery.Shared.Models;
using PhotoGallery.Shared.Models.Requests;

namespace PhotoGallery.BusinessLayer.Services.Interfaces;

public interface ICommentService
{
    Task<Result> DeleteAsync(Guid imageId, Guid commentId);

    Task<Result<Comment>> GetAsync(Guid imageId, Guid commentId);

    Task<Result<IEnumerable<Comment>>> GetListAsync(Guid imageId);

    Task<Result<Comment>> InsertAsync(Guid imageId, NewCommentRequest request);
}