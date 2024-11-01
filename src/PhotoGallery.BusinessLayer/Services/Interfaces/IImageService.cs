using OperationResults;
using PhotoGallery.Shared.Models;

namespace PhotoGallery.BusinessLayer.Services;

public interface IImageService
{
    Task<Result> DeleteAsync(Guid id);

    Task<Result<Image>> GetAsync(Guid id);

    Task<Result<PaginatedList<Image>>> GetListAsync(string title, string description, int pageIndex, int itemsPerPage, string orderBy);

    Task<Result<StreamFileContent>> GetStreamAsync(Guid id);

    Task<Result<Image>> UploadAsync(Stream stream, string fileName, string title, string description);
}