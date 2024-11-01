namespace PhotoGallery.Contracts;

public interface IUserService
{
    Task<Guid> GetIdAsync();

    Task<string> GetUserNameAsync();
}