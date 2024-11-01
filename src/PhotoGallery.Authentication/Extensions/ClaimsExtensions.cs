using System.Security.Claims;
using System.Security.Principal;

namespace PhotoGallery.Authentication.Extensions;

public static class ClaimsExtensions
{
    public static Guid GetId(this IPrincipal user)
    {
        var value = GetClaimValueInternal(user, ClaimTypes.NameIdentifier);

        if (Guid.TryParse(value, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }

    public static string GetClaimValue(this IPrincipal user, string claimType)
        => GetClaimValueInternal(user, claimType);

    internal static string GetClaimValueInternal(this IPrincipal user, string claimType)
        => ((ClaimsPrincipal)user).FindFirst(claimType)?.Value;
}