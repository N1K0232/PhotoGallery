using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PhotoGallery.Authentication.Entities;

namespace PhotoGallery.Authentication.Requirements;

public class UserActiveHandler(UserManager<ApplicationUser> userManager) : AuthorizationHandler<UserActiveRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserActiveRequirement requirement)
    {
        var user = await userManager.GetUserAsync(context.User);
        var lockedOut = await userManager.IsLockedOutAsync(user);

        if (!lockedOut)
        {
            context.Succeed(requirement);
        }
    }
}