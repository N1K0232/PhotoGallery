namespace PhotoGallery.StorageProviders.FileSystem;

public class FileSystemStorageProvider(FileSystemStorageOptions options) : IStorageProvider
{
    public Task DeleteAsync(string path)
    {
        var fullPath = CreatePath(path);
        if (!File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<Stream> ReadAsync(string path)
    {
        var fullPath = CreatePath(path);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream>(null);
        }

        var stream = File.OpenRead(fullPath);
        return Task.FromResult<Stream>(stream);
    }

    public async Task SaveAsync(Stream stream, string path)
    {
        var fullPath = CreatePath(path);
        var directoryName = Path.GetDirectoryName(fullPath);

        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        using var fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write);
        stream.Position = 0L;

        await stream.CopyToAsync(fileStream);
        fileStream.Close();
    }

    private string CreatePath(string path) => Path.Combine(options.StorageFolder, path);
}