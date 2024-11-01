using PhotoGallery.DataAccessLayer.Entities.Common;

namespace PhotoGallery.DataAccessLayer;
public interface IApplicationDbContext
{
    Task DeleteAsync<T>(T entity) where T : BaseEntity;

    Task DeleteAsync<T>(IEnumerable<T> entities) where T : BaseEntity;

    ValueTask<T> GetAsync<T>(Guid id) where T : BaseEntity;

    IQueryable<T> GetData<T>(bool trackingChanges = false, string sql = null, params object[] parameters) where T : BaseEntity;

    Task InsertAsync<T>(T entity) where T : BaseEntity;

    Task SaveAsync();
}