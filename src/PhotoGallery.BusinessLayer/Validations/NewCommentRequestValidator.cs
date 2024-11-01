using FluentValidation;
using PhotoGallery.Shared.Models.Requests;

namespace PhotoGallery.BusinessLayer.Validations;

public class NewCommentRequestValidator : AbstractValidator<NewCommentRequest>
{
    public NewCommentRequestValidator()
    {
        RuleFor(c => c.Title)
            .MaximumLength(256)
            .NotEmpty()
            .WithMessage("The title is required");

        RuleFor(c => c.Text)
            .MaximumLength(5000)
            .NotEmpty()
            .WithMessage("The text is required");

        RuleFor(c => c.Score)
            .InclusiveBetween(1, 5)
            .WithMessage("You can only rate the image between 1 and 5");
    }
}