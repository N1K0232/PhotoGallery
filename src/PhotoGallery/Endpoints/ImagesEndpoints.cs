using MinimalHelpers.Routing;
using OperationResults;
using OperationResults.AspNetCore.Http;
using PhotoGallery.BusinessLayer.Services;
using PhotoGallery.Shared.Models;

namespace PhotoGallery.Endpoints;

public class ImagesEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var imagesApiGroup = endpoints.MapGroup("/api/images").RequireAuthorization();

        imagesApiGroup.MapDelete("{id:guid}", DeleteAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("Delete")
            .WithOpenApi();

        imagesApiGroup.MapGet("{id:guid}", GetAsync)
            .Produces<Image>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetImage")
            .WithOpenApi();

        imagesApiGroup.MapGet(string.Empty, GetListAsync)
            .Produces<PaginatedList<Image>>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("GetImages")
            .WithOpenApi();

        imagesApiGroup.MapGet("{id:guid}/stream", GetStreamAsync)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetImageStream")
            .WithOpenApi();

        imagesApiGroup.MapPost(string.Empty, UploadAsync)
            .Produces<Image>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status400BadRequest)
            .DisableAntiforgery()
            .WithName("Upload")
            .WithOpenApi();
    }

    private static async Task<IResult> DeleteAsync(IImageService imageService, Guid id, HttpContext httpContext)
    {
        var result = await imageService.DeleteAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetAsync(IImageService imageService, Guid id, HttpContext httpContext)
    {
        var result = await imageService.GetAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetListAsync(IImageService imageService, HttpContext httpContext, string title = null, string description = null, int pageIndex = 0, int itemsPerPage = 10, string orderBy = "Name")
    {
        var result = await imageService.GetListAsync(title, description, pageIndex, itemsPerPage, orderBy);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetStreamAsync(IImageService imageService, Guid id, HttpContext httpContext)
    {
        var result = await imageService.GetStreamAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> UploadAsync(IImageService imageService, IFormFile file, string title, string description, HttpContext httpContext)
    {
        var result = await imageService.UploadAsync(file.OpenReadStream(), file.FileName, title, description);
        return httpContext.CreateResponse(result, "GetImage", new { id = result.Content?.Id });
    }
}