using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OperationResults;
using PhotoGallery.Authentication.Entities;
using PhotoGallery.BusinessLayer.Services.Interfaces;
using PhotoGallery.Shared.Models;

namespace PhotoGallery.BusinessLayer.Services;

public class MeService(UserManager<ApplicationUser> userManager, IMapper mapper) : IMeService
{
    public async Task<Result<User>> GetUserAsync(ClaimsPrincipal principal)
    {
        var dbUser = await userManager.GetUserAsync(principal);
        var user = mapper.Map<User>(dbUser);

        return user;
    }
}