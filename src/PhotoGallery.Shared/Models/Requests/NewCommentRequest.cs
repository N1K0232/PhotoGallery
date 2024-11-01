namespace PhotoGallery.Shared.Models.Requests;

public record class NewCommentRequest(string Title, string Text, int Score);