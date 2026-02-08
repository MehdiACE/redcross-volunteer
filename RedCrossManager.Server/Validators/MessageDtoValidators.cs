using FluentValidation;
using RedCrossManager.Server.DTOs.Messages;

namespace RedCrossManager.Server.Validators;

public class ComposeMessageDtoValidator : AbstractValidator<ComposeMessageDto>
{
    public ComposeMessageDtoValidator()
    {
        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Message subject is required.")
            .MaximumLength(200);

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Message body is required.")
            .MaximumLength(10000);

        RuleFor(x => x.RecipientType)
            .NotEmpty().WithMessage("Recipient type is required.")
            .Must(x => x == "User" || x == "Volunteer")
            .WithMessage("Recipient type must be 'User' or 'Volunteer'.");

        RuleFor(x => x.RecipientIds)
            .NotEmpty().WithMessage("At least one recipient is required.")
            .Must(x => x.Count <= 100).WithMessage("Cannot send to more than 100 recipients at once.");
    }
}
