using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using MinimalHelpers.Routing;
using MinimalHelpers.Validation;
using OperationResults.AspNetCore.Http;
using PhotoGallery.Authentication;
using PhotoGallery.Authentication.DataProtection;
using PhotoGallery.Authentication.Entities;
using PhotoGallery.Authentication.Requirements;
using PhotoGallery.BusinessLayer.Mapping;
using PhotoGallery.BusinessLayer.Services;
using PhotoGallery.BusinessLayer.Settings;
using PhotoGallery.Contracts;
using PhotoGallery.DataAccessLayer;
using PhotoGallery.Extensions;
using PhotoGallery.Services;
using PhotoGallery.Swagger;
using QRCoder;
using Serilog;
using SimpleAuthentication;
using TinyHelpers.AspNetCore.Extensions;
using TinyHelpers.AspNetCore.Swagger;
using TinyHelpers.Extensions;
using TinyHelpers.Json.Serialization;
using ResultErrorResponseFormat = OperationResults.AspNetCore.Http.ErrorResponseFormat;
using ValidationErrorResponseFormat = MinimalHelpers.Validation.ErrorResponseFormat;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", true, true);

builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
});

var settings = builder.Services.ConfigureAndGet<AppSettings>(builder.Configuration, nameof(AppSettings));
var email = builder.Services.ConfigureAndGet<EmailSettings>(builder.Configuration, nameof(EmailSettings));
var swagger = builder.Services.ConfigureAndGet<SwaggerSettings>(builder.Configuration, nameof(SwaggerSettings));

builder.Services.AddHttpContextAccessor();
builder.Services.AddRequestLocalization(settings.SupportedCultures);

builder.Services.AddDefaultExceptionHandler();
builder.Services.AddDefaultProblemDetails();

builder.Services.AddRazorPages();
builder.Services.AddWebOptimizer(minifyCss: true, minifyJavaScript: builder.Environment.IsProduction());

builder.Services.AddDataProtection().SetApplicationName(settings.ApplicationName).PersistKeysToDbContext<ApplicationDbContext>();
builder.Services.AddScoped<IDataProtectionService, DataProtectionService>();

builder.Services.AddScoped(services =>
{
    var dataProtectionProvider = services.GetRequiredService<IDataProtectionProvider>();
    var protector = dataProtectionProvider.CreateProtector(settings.ApplicationName);

    return protector.ToTimeLimitedDataProtector();
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
    options.SerializerOptions.Converters.Add(new UtcDateTimeConverter());
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddAutoMapper(typeof(UserMapperProfile).Assembly);

builder.Services.AddOperationResult(options =>
{
    options.ErrorResponseFormat = ResultErrorResponseFormat.List;
});

builder.Services.ConfigureValidation(options =>
{
    options.ErrorResponseFormat = ValidationErrorResponseFormat.List;
});

builder.Services.AddSimpleAuthentication(builder.Configuration, addAuthorizationServices: false);
builder.Services.AddFluentEmail(email.EmailAddress).WithSendinblue();

if (swagger.Enabled)
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "PhotoGallery Api",
            Version = "v1"
        });

        options.AddSimpleAuthentication(builder.Configuration);
        options.AddDefaultResponse();
        options.AddAcceptLanguageHeader();
    });
}

builder.Services.AddScoped<IUserService, HttpUserService>();
builder.Services.AddScoped<IAuthorizationHandler, UserActiveHandler>();

builder.Services.AddTransient<IClaimsTransformation, ClaimsTransformer>();
builder.Services.AddAuthorization(options =>
{
    var policyBuilder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
    policyBuilder.Requirements.Add(new UserActiveRequirement());

    options.DefaultPolicy = policyBuilder.Build();

    options.AddPolicy("UserActive", policy =>
    {
        policy.RequireRole(RoleNames.User);
        policy.Requirements.Add(new UserActiveRequirement());
    });
});

builder.Services.AddScoped<IQRCodeGeneratorService, QRCodeGeneratorService>();
builder.Services.AddScoped(_ => new QRCodeGenerator());

var connectionString = builder.Configuration.GetConnectionString("SqlConnection");
builder.Services.AddSqlServer<ApplicationDbContext>(connectionString, options => options.EnableRetryOnFailure(10, TimeSpan.FromSeconds(2), null));

builder.Services.AddScoped<IApplicationDbContext>(services => services.GetRequiredService<ApplicationDbContext>());
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var azureStorageConnectionString = builder.Configuration.GetConnectionString("AzureStorageConnection");
if (azureStorageConnectionString.HasValue())
{
    builder.Services.AddAzureStorage(options =>
    {
        options.ConnectionString = azureStorageConnectionString;
        options.ContainerName = settings.StorageFolder;
    });
}
else
{
    builder.Services.AddFileSystemStorage(options =>
    {
        options.StorageFolder = settings.StorageFolder;
    });
}

builder.Services.Scan(scan => scan.FromAssemblyOf<IdentityService>()
    .AddClasses(classes => classes.InNamespaceOf<IdentityService>())
    .AsImplementedInterfaces()
    .WithScopedLifetime());

var app = builder.Build();
app.Environment.ApplicationName = settings.ApplicationName;

app.UseHttpsRedirection();
app.UseRequestLocalization();

app.UseRouting();
app.UseWebOptimizer();

app.UseWhen(context => context.IsWebRequest(), builder =>
{
    if (!app.Environment.IsDevelopment())
    {
        builder.UseExceptionHandler("/Errors/500");
        builder.UseHsts();
    }

    builder.UseStatusCodePagesWithReExecute("/Errors/{0}");
});

app.UseWhen(context => context.IsApiRequest(), builder =>
{
    builder.UseExceptionHandler();
    builder.UseStatusCodePages();
});

if (swagger.Enabled)
{
    app.UseMiddleware<SwaggerBasicAuthenticationMiddleware>();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PhotoGallery Api v1");
        options.InjectStylesheet("/css/swagger.css");
    });
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseSerilogRequestLogging(options =>
{
    options.IncludeQueryInRequestPath = true;
});

app.MapRazorPages();
app.MapEndpoints();

await app.RunAsync();