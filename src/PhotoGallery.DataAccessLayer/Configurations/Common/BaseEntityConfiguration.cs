using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhotoGallery.DataAccessLayer.Entities.Common;

namespace PhotoGallery.DataAccessLayer.Configurations.Common;

internal class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");

        builder.Property(x => x.CreatedAt).ValueGeneratedOnAdd().HasDefaultValueSql("getutcdate()");
        builder.Property(x => x.LastModifiedAt).IsRequired(false).ValueGeneratedOnUpdate();
    }
}