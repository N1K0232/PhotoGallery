using System.Linq.Dynamic.Core;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MimeMapping;
using OperationResults;
using PhotoGallery.BusinessLayer.Internal;
using PhotoGallery.Contracts;
using PhotoGallery.DataAccessLayer;
using PhotoGallery.Shared.Models;
using PhotoGallery.StorageProviders;
using TinyHelpers.Extensions;
using Entities = PhotoGallery.DataAccessLayer.Entities;

namespace PhotoGallery.BusinessLayer.Services;

public class ImageService : IImageService
{
    private readonly IApplicationDbContext dbContext;
    private readonly IStorageProvider storageProvider;
    private readonly IUserService userService;
    private readonly IMapper mapper;

    public ImageService(IApplicationDbContext dbContext,
        IStorageProvider storageProvider,
        IUserService userService,
        IMapper mapper)
    {
        this.dbContext = dbContext;
        this.storageProvider = storageProvider;
        this.userService = userService;
        this.mapper = mapper;
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var image = await dbContext.GetAsync<Entities.Image>(id);
        if (image is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound);
        }

        await dbContext.DeleteAsync(image);
        await dbContext.SaveAsync();

        await storageProvider.DeleteAsync(image.Path);
        return Result.Ok();
    }

    public async Task<Result<Image>> GetAsync(Guid id)
    {
        var dbImage = await dbContext.GetAsync<Entities.Image>(id);
        if (dbImage is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound);
        }

        var image = mapper.Map<Image>(dbImage);
        return image;
    }

    public async Task<Result<PaginatedList<Image>>> GetListAsync(string title, string description, int pageIndex, int itemsPerPage, string orderBy)
    {
        var query = dbContext.GetData<Entities.Image>()
            .WhereIf(title.HasValue(), i => i.Title.Contains(title))
            .WhereIf(description.HasValue(), i => i.Description.Contains(description));

        var totalCount = await query.CountAsync();
        var dbImages = await query.OrderBy(orderBy)
            .Skip(pageIndex * itemsPerPage).Take(itemsPerPage + 1)
            .ToListAsync();

        var images = mapper.Map<IEnumerable<Image>>(dbImages).Take(itemsPerPage);
        var hasNextPage = dbImages.Count > itemsPerPage;

        return new PaginatedList<Image>(images, totalCount, hasNextPage);
    }

    public async Task<Result<StreamFileContent>> GetStreamAsync(Guid id)
    {
        var image = await dbContext.GetAsync<Entities.Image>(id);
        if (image is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound);
        }

        var stream = await storageProvider.ReadAsync(image.Path);
        if (stream is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound);
        }

        return new StreamFileContent(stream, MimeUtility.GetMimeMapping(image.Path));
    }

    public async Task<Result<Image>> UploadAsync(Stream stream, string fileName, string title, string description)
    {
        var path = PathGenerator.CreatePath(fileName);
        await storageProvider.SaveAsync(stream, path);

        var userId = await userService.GetIdAsync();
        var dbImage = new Entities.Image
        {
            UserId = userId,
            Name = fileName,
            Path = path,
            Title = title,
            Description = description
        };

        await dbContext.InsertAsync(dbImage);
        await dbContext.SaveAsync();

        var image = mapper.Map<Image>(dbImage);
        return image;
    }
}