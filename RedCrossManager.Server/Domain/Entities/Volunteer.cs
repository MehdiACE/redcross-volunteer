namespace RedCrossManager.Server.Domain.Entities;

public class Volunteer
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public required string AddressStreet { get; set; }
    public required string AddressCity { get; set; }
    public required string AddressStateProvince { get; set; }
    public required string AddressPostalCode { get; set; }
    public required string AddressCountry { get; set; }
    public required string EmergencyContactName { get; set; }
    public required string EmergencyContactPhone { get; set; }
    public required string AreasOfInterest { get; set; } // JSON serialized list
    public required string Availability { get; set; } // JSON serialized structure
    public VolunteerStatus Status { get; set; } = VolunteerStatus.Pending;
    public required string LanguagePreference { get; set; } = "en";
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsMinor { get; set; } // Computed at registration
    public Guid? GuardianContactId { get; set; }
    public bool SmsOptIn { get; set; } = false;
    public Guid? UserId { get; set; } // Link to User for authentication

    // Navigation properties
    public User? User { get; set; }
    public ParentalConsent? ParentalConsent { get; set; }
    public ICollection<OnboardingStep> OnboardingSteps { get; set; } = new List<OnboardingStep>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<TrainingEnrollment> TrainingEnrollments { get; set; } = new List<TrainingEnrollment>();
    public ICollection<Certification> Certifications { get; set; } = new List<Certification>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}

public enum VolunteerStatus
{
    Pending,
    InTraining,
    Active,
    Inactive
}
