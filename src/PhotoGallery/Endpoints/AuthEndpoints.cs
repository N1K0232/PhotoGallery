using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;
using PhotoGallery.BusinessLayer.Services.Interfaces;
using PhotoGallery.Shared.Models.Requests;
using PhotoGallery.Shared.Models.Responses;

namespace PhotoGallery.Endpoints;

public class AuthEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var authApiGroup = endpoints.MapGroup("/api/auth").AllowAnonymous();

        authApiGroup.MapGet("/qrcode", GetQRCodeAsync)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("qrcode")
            .WithOpenApi();

        authApiGroup.MapPost("/login", LoginAsync)
            .Produces<LoginResponse>()
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("login")
            .WithOpenApi();

        authApiGroup.MapPost("/register", RegisterAsync)
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("register")
            .WithOpenApi();

        authApiGroup.MapPost("/validate2fa", ValidateAsync)
            .Produces<LoginResponse>()
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("validate2fa")
            .WithOpenApi();

        authApiGroup.MapGet("/verifyemail", VerifyEmailAsync)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("verifyemail")
            .WithOpenApi();
    }

    private static async Task<IResult> GetQRCodeAsync(IIdentityService identityService, string token, HttpContext httpContext)
    {
        var result = await identityService.GetQRCodeAsync(token);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> LoginAsync(IIdentityService identityService, LoginRequest request, HttpContext httpContext)
    {
        var result = await identityService.LoginAsync(request);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> RegisterAsync(IIdentityService identityService, RegisterRequest request, HttpContext httpContext)
    {
        var result = await identityService.RegisterAsync(request);
        return httpContext.CreateResponse(result, StatusCodes.Status201Created);
    }

    private static async Task<IResult> ValidateAsync(IIdentityService identityService, TwoFactorValidationRequest request, HttpContext httpContext)
    {
        var result = await identityService.ValidateAsync(request);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> VerifyEmailAsync(IIdentityService identityService, string userId, string token, HttpContext httpContext)
    {
        var result = await identityService.VerifyEmailAsync(userId, token);
        return httpContext.CreateResponse(result);
    }
}