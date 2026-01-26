namespace RedCrossManager.Server.DTOs.Volunteers;

public record RegisterVolunteerDto(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateTime DateOfBirth,
    string AddressStreet,
    string AddressCity,
    string AddressStateProvince,
    string AddressPostalCode,
    string AddressCountry,
    string EmergencyContactName,
    string EmergencyContactPhone,
    List<string> AreasOfInterest,
    AvailabilityDto Availability,
    string LanguagePreference
);

public record AvailabilityDto(
    List<string> DaysOfWeek,
    string TimePreference
);

public record VolunteerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateTime DateOfBirth,
    string Status,
    string LanguagePreference,
    DateTime RegisteredAt,
    bool IsMinor,
    bool SmsOptIn
);

public record UpdateStatusDto(
    string Status
);
public record SmsOptInDto(
    bool SmsOptIn
);