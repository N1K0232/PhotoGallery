using PhotoGallery.Authentication.Entities;
using PhotoGallery.DataAccessLayer.Entities.Common;

namespace PhotoGallery.DataAccessLayer.Entities;

public class Image : BaseEntity
{
    public Guid UserId { get; set; }

    public string Name { get; set; }

    public string Path { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public virtual ApplicationUser User { get; set; }
}