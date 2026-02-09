using System.Text.Json;
using RedCrossManager.Server.Domain.Entities;

namespace RedCrossManager.Server.Tests.Infrastructure;

public static class TestDataFactory
{
    public static Volunteer CreateVolunteer(Guid id, bool isMinor = false, string? email = null)
    {
        return new Volunteer
        {
            Id = id,
            FirstName = "Test",
            LastName = "Volunteer",
            Email = email ?? $"volunteer-{id:N}@example.com",
            Phone = "+15145550000",
            DateOfBirth = isMinor ? DateTime.UtcNow.AddYears(-16) : DateTime.UtcNow.AddYears(-25),
            AddressStreet = "123 Main St",
            AddressCity = "Montreal",
            AddressStateProvince = "QC",
            AddressPostalCode = "H1A 1A1",
            AddressCountry = "Canada",
            EmergencyContactName = "Emergency Contact",
            EmergencyContactPhone = "+15145551111",
            AreasOfInterest = JsonSerializer.Serialize(new[] { "First Aid" }),
            Availability = JsonSerializer.Serialize(new { DaysOfWeek = new[] { "Monday" }, TimePreference = "Morning" }),
            Status = VolunteerStatus.Pending,
            LanguagePreference = "en",
            RegisteredAt = DateTime.UtcNow,
            IsMinor = isMinor,
            SmsOptIn = false
        };
    }

    public static ParentalConsent CreateParentalConsent(Guid id, Guid volunteerId, ConsentStatus status = ConsentStatus.Requested)
    {
        return new ParentalConsent
        {
            Id = id,
            VolunteerId = volunteerId,
            GuardianName = "Parent Guardian",
            GuardianEmail = "parent@example.com",
            GuardianPhone = "+15145552222",
            ConsentStatus = status,
            SubmittedAt = DateTime.UtcNow.AddHours(-2),
            ReviewedAt = status is ConsentStatus.Approved or ConsentStatus.Rejected ? DateTime.UtcNow : null,
            ReviewerNotes = status == ConsentStatus.Rejected ? "Does not meet criteria" : null
        };
    }
}
