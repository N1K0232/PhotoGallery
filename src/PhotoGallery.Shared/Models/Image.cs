using PhotoGallery.Shared.Models.Common;

namespace PhotoGallery.Shared.Models;

public class Image : BaseObject
{
    public string Name { get; set; }

    public string Path { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string User { get; set; }
}