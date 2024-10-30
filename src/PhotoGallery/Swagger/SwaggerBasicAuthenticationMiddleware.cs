using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using PhotoGallery.BusinessLayer.Settings;
using PhotoGallery.Extensions;
using TinyHelpers.Extensions;

namespace PhotoGallery.Swagger;

public class SwaggerBasicAuthenticationMiddleware(RequestDelegate next, IOptions<SwaggerSettings> swaggerOptions)
{
    private readonly SwaggerSettings swagger = swaggerOptions.Value;

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (httpContext.IsSwaggerRequest() && swagger.UserName.HasValue() && swagger.Password.HasValue())
        {
            string authenticationHeader = httpContext.Request.Headers[HeaderNames.Authorization];
            if (authenticationHeader?.StartsWith("Basic ") ?? false)
            {
                var (userName, password) = GetCredentials(authenticationHeader);
                if (userName == swagger.UserName && password == swagger.Password)
                {
                    await next.Invoke(httpContext);
                    return;
                }
            }

            httpContext.Response.Headers.WWWAuthenticate = new StringValues("Basic");
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            await next.Invoke(httpContext);
        }
    }

    private static (string UserName, string Password) GetCredentials(string authenticationHeader)
    {
        var header = AuthenticationHeaderValue.Parse(authenticationHeader);
        var parameter = Convert.FromBase64String(header.Parameter);
        var credentials = Encoding.UTF8.GetString(parameter).Split(':', count: 2);

        return (credentials.ElementAtOrDefault(0), credentials.ElementAtOrDefault(1));
    }
}