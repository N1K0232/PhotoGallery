namespace PhotoGallery.StorageProviders;

public interface IStorageProvider
{
    Task DeleteAsync(string path);

    Task<Stream> ReadAsync(string path);

    Task SaveAsync(Stream stream, string path);
}