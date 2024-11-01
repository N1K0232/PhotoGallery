using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PhotoGallery.Authentication;
using PhotoGallery.DataAccessLayer.Entities.Common;

namespace PhotoGallery.DataAccessLayer;

public class ApplicationDbContext : AuthenticationDbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public Task DeleteAsync<T>(T entity) where T : BaseEntity
    {
        Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync<T>(IEnumerable<T> entities) where T : BaseEntity
    {
        Set<T>().RemoveRange(entities);
        return Task.CompletedTask;
    }

    public async ValueTask<T> GetAsync<T>(Guid id) where T : BaseEntity
    {
        var entity = await Set<T>().FindAsync([id]);
        return entity;
    }

    public IQueryable<T> GetData<T>(bool trackingChanges = false, string sql = null, params object[] parameters) where T : BaseEntity
    {
        var set = (!string.IsNullOrWhiteSpace(sql) && parameters.Length > 0) ? Set<T>().FromSqlRaw(sql, parameters) : Set<T>();
        return trackingChanges ? set.AsTracking() : set.AsNoTrackingWithIdentityResolution();
    }

    public async Task InsertAsync<T>(T entity) where T : BaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        await Set<T>().AddAsync(entity);
    }

    public async Task SaveAsync()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.Entity.GetType()))
            .ToList();

        foreach (var entry in entries.Where(e => e.State is EntityState.Modified))
        {
            (entry.Entity as BaseEntity).LastModifiedAt = DateTime.UtcNow;
        }

        await SaveChangesAsync(true);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();
        builder.ApplyConfigurationsFromAssembly(assembly);

        base.OnModelCreating(builder);
    }
}