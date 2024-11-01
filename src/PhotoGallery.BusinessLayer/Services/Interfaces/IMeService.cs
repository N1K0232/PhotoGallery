using System.Security.Claims;
using OperationResults;
using PhotoGallery.Shared.Models;

namespace PhotoGallery.BusinessLayer.Services.Interfaces;

public interface IMeService
{
    Task<Result<User>> GetUserAsync(ClaimsPrincipal principal);
}