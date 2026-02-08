using FluentValidation;
using RedCrossManager.Server.DTOs.Training;

namespace RedCrossManager.Server.Validators;

public class CreateTrainingDtoValidator : AbstractValidator<CreateTrainingDto>
{
    public CreateTrainingDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Training title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Training description is required.")
            .MaximumLength(2000);

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Training category is required.")
            .MaximumLength(100);

        RuleFor(x => x.MaxEnrollment)
            .GreaterThan(0).WithMessage("Maximum enrollment must be greater than 0.")
            .LessThanOrEqualTo(500).WithMessage("Maximum enrollment cannot exceed 500.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.")
            .GreaterThan(DateTime.UtcNow).WithMessage("Start date must be in the future.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date.");

        RuleFor(x => x.LocationName)
            .NotEmpty().WithMessage("Location is required.")
            .MaximumLength(300);

        RuleFor(x => x.CreatedByCoordinatorId)
            .NotEmpty().WithMessage("Coordinator ID is required.");
    }
}

public class EnrollTrainingDtoValidator : AbstractValidator<EnrollTrainingDto>
{
    public EnrollTrainingDtoValidator()
    {
        RuleFor(x => x.VolunteerId)
            .NotEmpty().WithMessage("Volunteer ID is required.");
    }
}

public class MarkAttendanceDtoValidator : AbstractValidator<MarkAttendanceDto>
{
    public MarkAttendanceDtoValidator()
    {
        RuleFor(x => x.EnrollmentId)
            .NotEmpty().WithMessage("Enrollment ID is required.");

        When(x => x.Attended && !string.IsNullOrEmpty(x.CertificateNumber), () =>
        {
            RuleFor(x => x.CertificateNumber)
                .MaximumLength(100);
        });
    }
}
