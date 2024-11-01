using System.Net.Mime;
using System.Security.Claims;
using AutoMapper;
using FluentEmail.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using OperationResults;
using PhotoGallery.Authentication;
using PhotoGallery.Authentication.DataProtection;
using PhotoGallery.Authentication.Entities;
using PhotoGallery.BusinessLayer.Services.Interfaces;
using PhotoGallery.Contracts;
using PhotoGallery.Shared.Models.Requests;
using PhotoGallery.Shared.Models.Responses;
using SimpleAuthentication.JwtBearer;
using TinyHelpers.Extensions;

namespace PhotoGallery.BusinessLayer.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly LinkGenerator linkGenerator;
    private readonly IJwtBearerService jwtBearerService;
    private readonly IFluentEmail fluentEmail;
    private readonly IMapper mapper;
    private readonly IQRCodeGeneratorService qrCodeGeneratorService;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IDataProtectionService dataProtectionService;

    public IdentityService(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        LinkGenerator linkGenerator,
        IJwtBearerService jwtBearerService,
        IFluentEmail fluentEmail,
        IMapper mapper,
        IQRCodeGeneratorService qrCodeGeneratorService,
        IHttpContextAccessor httpContextAccessor,
        IDataProtectionService dataProtectionService)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.linkGenerator = linkGenerator;
        this.jwtBearerService = jwtBearerService;
        this.fluentEmail = fluentEmail;
        this.mapper = mapper;
        this.qrCodeGeneratorService = qrCodeGeneratorService;
        this.httpContextAccessor = httpContextAccessor;
        this.dataProtectionService = dataProtectionService;
    }

    public async Task<Result<ByteArrayFileContent>> GetQRCodeAsync(string token)
    {
        ApplicationUser user;

        try
        {
            var userId = await dataProtectionService.UnprotectAsync(token);
            user = await userManager.FindByIdAsync(userId);
        }
        catch
        {
            return Result.Fail(FailureReasons.ClientError);
        }

        if (user is null || await HasAuthenticatorKeyAsync(user))
        {
            return Result.Fail(FailureReasons.ClientError);
        }

        var secret = await ResetAndGetAuthenticatorKeyAsync(user);
        var qrCodeBytes = await qrCodeGeneratorService.GenerateAsync(user.Email, secret);

        return new ByteArrayFileContent(qrCodeBytes, MediaTypeNames.Image.Png);
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        var result = await signInManager.PasswordSignInAsync(user, request.Password, request.IsPersistent, true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                return Result.Fail(FailureReasons.ClientError, $"You're locked out until {user.LockoutEnd}");
            }

            var emailConfirmed = await userManager.IsEmailConfirmedAsync(user);
            if (!emailConfirmed)
            {
                return Result.Fail(FailureReasons.ClientError, "You have to verify your email first");
            }

            if (result.RequiresTwoFactor)
            {
                var token = await dataProtectionService.ProtectAsync(user.Id.ToString(), TimeSpan.FromMinutes(15));
                return new LoginResponse(token);
            }
        }

        return await CreateTokenAsync(user);
    }

    public async Task<Result> RegisterAsync(RegisterRequest request)
    {
        var user = mapper.Map<ApplicationUser>(request);
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(',', result.Errors.Select(e => e.Description));
            return Result.Fail(FailureReasons.ClientError, "Couldn't registrate", errors);
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var httpContext = httpContextAccessor.HttpContext;

        var scheme = httpContext.Request.Scheme;
        var values = new RouteValueDictionary
        {
            ["userId"] = user.Id,
            ["token"] = token
        };

        var endpoint = linkGenerator.GetUriByRouteValues(httpContext, "verifyemail", values, scheme);
        var sendResult = await fluentEmail.To(user.Email).Subject("Confirm your email")
            .Body($"Please confirm your email by clicking this link: <a href='{endpoint}'>Confirm Email</a>", true)
            .SendAsync();

        if (!sendResult.Successful)
        {
            var errors = string.Join(',', sendResult.ErrorMessages);
            return Result.Fail(FailureReasons.ClientError, "Couldn't send verification email", errors);
        }

        return Result.Ok();
    }

    public async Task<Result<LoginResponse>> ValidateAsync(TwoFactorValidationRequest request)
    {
        ApplicationUser user;

        try
        {
            var userId = await dataProtectionService.UnprotectAsync(request.Token);
            user = await userManager.FindByIdAsync(userId);
        }
        catch
        {
            return Result.Fail(FailureReasons.ClientError);
        }

        if (user is null)
        {
            return Result.Fail(FailureReasons.ClientError);
        }

        var tokenProvider = userManager.Options.Tokens.AuthenticatorTokenProvider;
        var isValidTotpCode = await userManager.VerifyTwoFactorTokenAsync(user, tokenProvider, request.Code);

        if (!isValidTotpCode)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid two-factor code");
        }

        return await CreateTokenAsync(user);
    }

    public async Task<Result> VerifyEmailAsync(string userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        var result = await userManager.ConfirmEmailAsync(user, token);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, RoleNames.User);
            return Result.Ok();
        }

        var errors = string.Join(',', result.Errors.Select(e => e.Description));
        return Result.Fail(FailureReasons.ClientError, "Couldn't verify your email", errors);
    }

    private async Task<LoginResponse> CreateTokenAsync(ApplicationUser user)
    {
        var userRoles = await userManager.GetRolesAsync(user);
        await userManager.UpdateSecurityStampAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.SerialNumber, user.SecurityStamp)
        }
        .Union(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = await jwtBearerService.CreateTokenAsync(user.UserName, claims.ToList());
        return new LoginResponse(token);
    }

    private async Task<bool> HasAuthenticatorKeyAsync(ApplicationUser user)
    {
        var secret = await userManager.GetAuthenticatorKeyAsync(user);
        return secret.HasValue();
    }

    private async Task<string> ResetAndGetAuthenticatorKeyAsync(ApplicationUser user)
    {
        await userManager.ResetAuthenticatorKeyAsync(user);
        return await userManager.GetAuthenticatorKeyAsync(user);
    }
}