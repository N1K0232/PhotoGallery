using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhotoGallery.DataAccessLayer.Configurations.Common;
using PhotoGallery.DataAccessLayer.Entities;

namespace PhotoGallery.DataAccessLayer.Configurations;

internal class CommentConfiguration : BaseEntityConfiguration<Comment>
{
    public override void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.Property(c => c.Title).HasMaxLength(256).IsRequired();
        builder.Property(c => c.Text).HasColumnType("NVARCHAR(MAX)").IsRequired();

        builder.HasOne(c => c.Image)
            .WithMany(i => i.Comments)
            .HasForeignKey(c => c.ImageId)
            .IsRequired();

        builder.ToTable("Comments");
        base.Configure(builder);
    }
}