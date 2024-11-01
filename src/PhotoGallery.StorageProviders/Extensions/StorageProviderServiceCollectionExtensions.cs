using Azure.Storage.Blobs;
using PhotoGallery.StorageProviders;
using PhotoGallery.StorageProviders.Azure;
using PhotoGallery.StorageProviders.FileSystem;

namespace Microsoft.Extensions.DependencyInjection;

public static class StorageProviderServiceCollectionExtensions
{
    public static IServiceCollection AddAzureStorage(this IServiceCollection services, Action<AzureStorageOptions> setupAction)
    {
        var options = new AzureStorageOptions();
        setupAction.Invoke(options);

        services.AddSingleton(options);
        services.AddScoped(_ => new BlobServiceClient(options.ConnectionString));

        services.AddScoped<IStorageProvider, AzureStorageProvider>();
        return services;
    }

    public static IServiceCollection AddFileSystemStorage(this IServiceCollection services, Action<FileSystemStorageOptions> setupAction)
    {
        var options = new FileSystemStorageOptions();
        setupAction.Invoke(options);

        services.AddSingleton(options);
        services.AddScoped<IStorageProvider, FileSystemStorageProvider>();

        return services;
    }
}