using FluentEmail.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PhotoGallery.Email;

namespace PhotoGallery.Extensions;

public static class FluentEmailBuilderExtensions
{
    public static FluentEmailServicesBuilder WithSendinblue(this FluentEmailServicesBuilder builder)
    {
        builder.Services.TryAddSingleton<ISender, SendinblueSender>();
        return builder;
    }
}