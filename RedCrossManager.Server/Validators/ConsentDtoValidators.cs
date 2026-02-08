using FluentValidation;
using RedCrossManager.Server.DTOs.Consents;

namespace RedCrossManager.Server.Validators;

public class RequestConsentDtoValidator : AbstractValidator<RequestConsentDto>
{
    public RequestConsentDtoValidator()
    {
        RuleFor(x => x.GuardianEmail)
            .NotEmpty().WithMessage("Guardian email is required.")
            .EmailAddress().WithMessage("Guardian email must be a valid email address.")
            .MaximumLength(255);

        RuleFor(x => x.GuardianFullName)
            .NotEmpty().WithMessage("Guardian full name is required.")
            .MaximumLength(200);
    }
}

public class SubmitConsentDtoValidator : AbstractValidator<SubmitConsentDto>
{
    public SubmitConsentDtoValidator()
    {
        RuleFor(x => x.GuardianInfo).NotNull();
        
        RuleFor(x => x.GuardianInfo.FullName)
            .NotEmpty().WithMessage("Guardian full name is required.")
            .MaximumLength(200);

        RuleFor(x => x.GuardianInfo.Email)
            .NotEmpty().WithMessage("Guardian email is required.")
            .EmailAddress().WithMessage("Must be a valid email address.")
            .MaximumLength(255);

        RuleFor(x => x.GuardianInfo.Phone)
            .NotEmpty().WithMessage("Guardian phone is required.")
            .MaximumLength(50);

        RuleFor(x => x.GuardianInfo.Relationship)
            .NotEmpty().WithMessage("Relationship is required.")
            .MaximumLength(100);

        RuleFor(x => x.GuardianAgreement)
            .Equal(true).WithMessage("Guardian agreement must be accepted.");

        RuleFor(x => x.DataProcessingAgreement)
            .Equal(true).WithMessage("Data processing agreement must be accepted.");

        RuleFor(x => x.Signature)
            .NotEmpty().WithMessage("Signature is required.");
    }
}

public class ReviewConsentDtoValidator : AbstractValidator<ReviewConsentDto>
{
    public ReviewConsentDtoValidator()
    {
        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required.")
            .Must(x => x == "Approve" || x == "Reject")
            .WithMessage("Action must be 'Approve' or 'Reject'.");

        When(x => x.Action == "Reject", () =>
        {
            RuleFor(x => x.ReviewerNotes)
                .NotEmpty().WithMessage("Reviewer notes are required when rejecting consent.");
        });

        RuleFor(x => x.ReviewerNotes)
            .MaximumLength(2000);
    }
}
