namespace PhotoGallery.Shared.Models.Requests;

public record class RegisterRequest(string FirstName, string LastName, string UserName, string Email, string Password);