using PhotoGallery.Shared.Models.Common;

namespace PhotoGallery.Shared.Models;

public class Comment : BaseObject
{
    public string Title { get; set; }

    public string Text { get; set; }

    public int Score { get; set; }

    public float SentimentScore { get; set; }

    public string User { get; set; }
}