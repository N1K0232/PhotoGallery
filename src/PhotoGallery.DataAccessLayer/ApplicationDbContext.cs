using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PhotoGallery.Authentication;

namespace PhotoGallery.DataAccessLayer;

public class ApplicationDbContext : AuthenticationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();
        builder.ApplyConfigurationsFromAssembly(assembly);

        base.OnModelCreating(builder);
    }
}