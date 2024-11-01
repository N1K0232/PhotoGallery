using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;
using PhotoGallery.BusinessLayer.Services.Interfaces;
using PhotoGallery.Shared.Models;

namespace PhotoGallery.Endpoints;

public class MeEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var meApiGroup = endpoints.MapGroup("/api/me").RequireAuthorization();

        meApiGroup.MapGet("/profile", GetProfileAsync)
            .Produces<User>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithName("profile")
            .WithOpenApi();
    }

    private static async Task<IResult> GetProfileAsync(IMeService meService, HttpContext httpContext)
    {
        var result = await meService.GetUserAsync(httpContext.User);
        return httpContext.CreateResponse(result);
    }
}