using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhotoGallery.DataAccessLayer.Configurations.Common;
using PhotoGallery.DataAccessLayer.Entities;

namespace PhotoGallery.DataAccessLayer.Configurations;

internal class ImageConfiguration : BaseEntityConfiguration<Image>
{
    public override void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.Property(i => i.Name).HasMaxLength(256).IsUnicode(false).IsRequired();
        builder.Property(i => i.Path).HasMaxLength(512).IsUnicode(false).IsRequired();

        builder.Property(i => i.Title).HasMaxLength(256).IsRequired();
        builder.Property(i => i.Description).HasColumnType("NVARCHAR(MAX)").IsRequired(false);

        builder.HasIndex(i => new { i.UserId, i.Name, i.Path })
            .HasDatabaseName("IX_UserImage")
            .IsUnique();

        builder.ToTable("Images");
        base.Configure(builder);
    }
}