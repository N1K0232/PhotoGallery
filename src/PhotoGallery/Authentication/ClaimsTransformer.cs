using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using PhotoGallery.BusinessLayer.Settings;

namespace PhotoGallery.Authentication;

public class ClaimsTransformer(IOptions<AppSettings> appSettingsOptions) : IClaimsTransformation
{
    private readonly AppSettings appSettings = appSettingsOptions.Value;

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = (ClaimsIdentity)principal.Identity;
        identity.AddClaim(new Claim(ClaimTypes.System, appSettings.ApplicationName));

        return Task.FromResult(principal);
    }
}