namespace PhotoGallery.Shared.Models.Requests;

public record class LoginRequest(string Email, string Password, bool IsPersistent);