using PhotoGallery.Shared.Models.Common;

namespace PhotoGallery.Shared.Models;

public class User : BaseObject
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string UserName { get; set; }

    public string Email { get; set; }
}