using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PhotoGallery.Authentication.Entities;

namespace PhotoGallery.Authentication;

public class AuthenticationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>,
    ApplicationUserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>, IDataProtectionKeyContext
{
    public AuthenticationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b =>
        {
            b.Property(u => u.FirstName).HasMaxLength(256).IsRequired();
            b.Property(u => u.LastName).HasMaxLength(256).IsRequired();
        });

        builder.Entity<ApplicationUserRole>(b =>
        {
            b.HasKey(ur => new { ur.UserId, ur.RoleId });

            b.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            b.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
        });

        builder.Entity<DataProtectionKey>(b =>
        {
            b.ToTable("DataProtectionKeys");
            b.HasKey(k => k.Id);

            b.Property(k => k.FriendlyName).HasColumnType("NVARCHAR(MAX)").IsRequired(false);
            b.Property(k => k.Xml).HasColumnType("NVARCHAR(MAX)").IsRequired(false);
        });
    }
}