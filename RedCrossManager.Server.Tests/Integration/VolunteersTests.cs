using System.Net;
using System.Net.Http.Json;
using RedCrossManager.Server.DTOs.Volunteers;
using RedCrossManager.Server.Tests.Infrastructure;
using Xunit;

namespace RedCrossManager.Server.Tests.Integration;

public class VolunteersTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public VolunteersTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ValidVolunteer_ReturnsCreated()
    {
        // Arrange
        var registerDto = new RegisterVolunteerDto(
            FirstName: "John",
            LastName: "Doe",
            Email: "john.doe@example.com",
            Phone: "+15145551234",
            DateOfBirth: new DateTime(1990, 1, 1),
            AddressStreet: "123 Main St",
            AddressCity: "Montreal",
            AddressStateProvince: "QC",
            AddressPostalCode: "H1A 1A1",
            AddressCountry: "Canada",
            EmergencyContactName: "Jane Doe",
            EmergencyContactPhone: "+15145555678",
            AreasOfInterest: new List<string> { "First Aid", "Disaster Response" },
            Availability: new AvailabilityDto(
                DaysOfWeek: new List<string> { "Monday", "Wednesday", "Friday" },
                TimePreference: "Evenings"
            ),
            LanguagePreference: "en"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/volunteers/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<VolunteerDto>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("john.doe@example.com", result.Email);
        Assert.Equal("+15145551234", result.Phone);
        Assert.Equal("Pending", result.Status);
        Assert.Equal("en", result.LanguagePreference);
        Assert.False(result.IsMinor);
        Assert.False(result.SmsOptIn);
    }

    [Fact]
    public async Task Register_MinorVolunteer_ReturnsCreatedWithIsMinorTrue()
    {
        // Arrange
        var birthDate = DateTime.UtcNow.AddYears(-16); // 16 years old
        var registerDto = new RegisterVolunteerDto(
            FirstName: "Alice",
            LastName: "Smith",
            Email: "alice.smith@example.com",
            Phone: "+15145551111",
            DateOfBirth: birthDate,
            AddressStreet: "456 Elm St",
            AddressCity: "Quebec City",
            AddressStateProvince: "QC",
            AddressPostalCode: "G1A 1A1",
            AddressCountry: "Canada",
            EmergencyContactName: "Bob Smith",
            EmergencyContactPhone: "+15145552222",
            AreasOfInterest: new List<string> { "Community Programs" },
            Availability: new AvailabilityDto(
                DaysOfWeek: new List<string> { "Saturday" },
                TimePreference: "Morning"
            ),
            LanguagePreference: "fr"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/volunteers/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<VolunteerDto>();
        Assert.NotNull(result);
        Assert.True(result.IsMinor);
        Assert.Equal("fr", result.LanguagePreference);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        // Arrange - First registration
        var registerDto = new RegisterVolunteerDto(
            FirstName: "Bob",
            LastName: "Johnson",
            Email: "bob.johnson@example.com",
            Phone: "+15145553333",
            DateOfBirth: new DateTime(1985, 5, 15),
            AddressStreet: "789 Oak St",
            AddressCity: "Laval",
            AddressStateProvince: "QC",
            AddressPostalCode: "H7A 1A1",
            AddressCountry: "Canada",
            EmergencyContactName: "Mary Johnson",
            EmergencyContactPhone: "+15145554444",
            AreasOfInterest: new List<string> { "Blood Drive" },
            Availability: new AvailabilityDto(
                DaysOfWeek: new List<string> { "Tuesday", "Thursday" },
                TimePreference: "Afternoons"
            ),
            LanguagePreference: "en"
        );

        // Act - First registration (should succeed)
        var response1 = await _client.PostAsJsonAsync("/api/v1/volunteers/register", registerDto);
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);

        // Act - Second registration with same email (should fail)
        var response2 = await _client.PostAsJsonAsync("/api/v1/volunteers/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
        
        var errorContent = await response2.Content.ReadAsStringAsync();
        Assert.Contains("already exists", errorContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetById_ExistingVolunteer_ReturnsOk()
    {
        // Arrange - Register a volunteer first
        var registerDto = new RegisterVolunteerDto(
            FirstName: "Charlie",
            LastName: "Brown",
            Email: "charlie.brown@example.com",
            Phone: "+15145555555",
            DateOfBirth: new DateTime(1992, 3, 10),
            AddressStreet: "321 Pine St",
            AddressCity: "Sherbrooke",
            AddressStateProvince: "QC",
            AddressPostalCode: "J1A 1A1",
            AddressCountry: "Canada",
            EmergencyContactName: "Lucy Brown",
            EmergencyContactPhone: "+15145556666",
            AreasOfInterest: new List<string> { "First Aid" },
            Availability: new AvailabilityDto(
                DaysOfWeek: new List<string> { "Monday" },
                TimePreference: "Morning"
            ),
            LanguagePreference: "en"
        );

        var createResponse = await _client.PostAsJsonAsync("/api/v1/volunteers/register", registerDto);
        var createdVolunteer = await createResponse.Content.ReadFromJsonAsync<VolunteerDto>();
        Assert.NotNull(createdVolunteer);

        // Act
        var response = await _client.GetAsync($"/api/v1/volunteers/{createdVolunteer.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<VolunteerDto>();
        Assert.NotNull(result);
        Assert.Equal(createdVolunteer.Id, result.Id);
        Assert.Equal("Charlie", result.FirstName);
        Assert.Equal("charlie.brown@example.com", result.Email);
    }

    [Fact]
    public async Task GetById_NonExistentVolunteer_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/volunteers/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByEmail_ExistingVolunteer_ReturnsOk()
    {
        // Arrange - Register a volunteer first
        var registerDto = new RegisterVolunteerDto(
            FirstName: "Diana",
            LastName: "Prince",
            Email: "diana.prince@example.com",
            Phone: "+15145557777",
            DateOfBirth: new DateTime(1988, 7, 20),
            AddressStreet: "654 Maple St",
            AddressCity: "Gatineau",
            AddressStateProvince: "QC",
            AddressPostalCode: "J8A 1A1",
            AddressCountry: "Canada",
            EmergencyContactName: "Steve Trevor",
            EmergencyContactPhone: "+15145558888",
            AreasOfInterest: new List<string> { "Disaster Response" },
            Availability: new AvailabilityDto(
                DaysOfWeek: new List<string> { "Friday" },
                TimePreference: "Evening"
            ),
            LanguagePreference: "fr"
        );

        await _client.PostAsJsonAsync("/api/v1/volunteers/register", registerDto);

        // Act
        var response = await _client.GetAsync($"/api/v1/volunteers/by-email/diana.prince@example.com");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<VolunteerDto>();
        Assert.NotNull(result);
        Assert.Equal("Diana", result.FirstName);
        Assert.Equal("diana.prince@example.com", result.Email);
    }
}
