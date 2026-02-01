using FluentValidation;
using RedCrossManager.Server.DTOs.Volunteers;

namespace RedCrossManager.Server.Validators;

public class RegisterVolunteerDtoValidator : AbstractValidator<RegisterVolunteerDto>
{
    public RegisterVolunteerDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        
        // Password validation - minimum 8 characters with uppercase, lowercase, and special character
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
        
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DateOfBirth).NotEmpty().Must(BeAValidAge)
            .WithMessage("Volunteer must be between 14 and 100 years old.");
        RuleFor(x => x.AddressStreet).NotEmpty().MaximumLength(255);
        RuleFor(x => x.AddressCity).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AddressStateProvince).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AddressPostalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.AddressCountry).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EmergencyContactName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EmergencyContactPhone).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AreasOfInterest).NotEmpty();
        RuleFor(x => x.LanguagePreference).NotEmpty().Must(x => x == "en" || x == "fr")
            .WithMessage("Language must be 'en' or 'fr'.");
    }

    private bool BeAValidAge(DateTime dateOfBirth)
    {
        var age = DateTime.UtcNow.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > DateTime.UtcNow.AddYears(-age)) age--;
        return age >= 14 && age <= 100;
    }
}
