using PhotoGallery.Authentication.Entities;
using PhotoGallery.DataAccessLayer.Entities.Common;

namespace PhotoGallery.DataAccessLayer.Entities;

public class Comment : BaseEntity
{
    public Guid ImageId { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; }

    public string Text { get; set; }

    public int Score { get; set; }

    public float SentimentScore { get; set; }

    public virtual Image Image { get; set; }

    public virtual ApplicationUser User { get; set; }
}