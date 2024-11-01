using PhotoGallery.Authentication.Extensions;
using PhotoGallery.Contracts;

namespace PhotoGallery.Services;

public class HttpUserService(IHttpContextAccessor httpContextAccessor) : IUserService
{
    public Task<Guid> GetIdAsync()
    {
        var userId = httpContextAccessor.HttpContext.User.GetId();
        return Task.FromResult(userId);
    }

    public Task<string> GetUserNameAsync()
    {
        var userName = httpContextAccessor.HttpContext.User.Identity.Name;
        return Task.FromResult(userName);
    }
}