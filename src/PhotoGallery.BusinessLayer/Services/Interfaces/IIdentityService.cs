using OperationResults;
using PhotoGallery.Shared.Models.Requests;
using PhotoGallery.Shared.Models.Responses;

namespace PhotoGallery.BusinessLayer.Services.Interfaces;

public interface IIdentityService
{
    Task<Result<ByteArrayFileContent>> GetQRCodeAsync(string token);

    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);

    Task<Result> RegisterAsync(RegisterRequest request);

    Task<Result<LoginResponse>> ValidateAsync(TwoFactorValidationRequest request);

    Task<Result> VerifyEmailAsync(string userId, string token);
}