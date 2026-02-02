using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.DTOs.Auth;
using RedCrossManager.Server.DTOs.Dashboard;
using RedCrossManager.Server.Infrastructure;
using RedCrossManager.Server.Tests.Infrastructure;
using Xunit;

namespace RedCrossManager.Server.Tests.Integration;

public class DashboardTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public DashboardTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDashboard_ReturnsDashboardShapeForVolunteer()
    {
        await _factory.SeedDatabaseAsync();
        var user = await _factory.CreateTestUserAsync("volunteer.dashboard@redcross.local", "SecurePassword123!", "Admin");

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<RedCrossDbContext>();

            var volunteer = new Volunteer
            {
                Id = Guid.NewGuid(),
                FirstName = "Dora",
                LastName = "Durand",
                Email = "volunteer.dashboard@redcross.local",
                Phone = "+15145550000",
                DateOfBirth = new DateTime(1994, 5, 12),
                AddressStreet = "123 Main St",
                AddressCity = "Montreal",
                AddressStateProvince = "QC",
                AddressPostalCode = "H1A 1A1",
                AddressCountry = "Canada",
                EmergencyContactName = "Parent",
                EmergencyContactPhone = "+15145551111",
                AreasOfInterest = "[]",
                Availability = "{}",
                Status = VolunteerStatus.Pending,
                LanguagePreference = "fr",
                IsMinor = false,
                SmsOptIn = false,
                UserId = user.Id
            };

            dbContext.Volunteers.Add(volunteer);
            dbContext.OnboardingSteps.AddRange(
                new OnboardingStep
                {
                    Id = Guid.NewGuid(),
                    VolunteerId = volunteer.Id,
                    StepType = StepType.DocumentVerification,
                    Status = StepStatus.Approved,
                    ApprovedAt = DateTime.UtcNow.AddDays(-2)
                },
                new OnboardingStep
                {
                    Id = Guid.NewGuid(),
                    VolunteerId = volunteer.Id,
                    StepType = StepType.OrientationTraining,
                    Status = StepStatus.InProgress,
                    StartedAt = DateTime.UtcNow.AddDays(-1)
                },
                new OnboardingStep
                {
                    Id = Guid.NewGuid(),
                    VolunteerId = volunteer.Id,
                    StepType = StepType.Certification,
                    Status = StepStatus.NotStarted
                },
                new OnboardingStep
                {
                    Id = Guid.NewGuid(),
                    VolunteerId = volunteer.Id,
                    StepType = StepType.FinalReview,
                    Status = StepStatus.NotStarted
                }
            );

            await dbContext.SaveChangesAsync();
        }

        var loginRequest = new LoginRequestDto(
            Email: "volunteer.dashboard@redcross.local",
            Password: "SecurePassword123!"
        );

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginResult);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);

        var response = await _client.GetAsync("/api/v1/volunteers/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dashboard = await response.Content.ReadFromJsonAsync<VolunteerDashboardDto>();
        Assert.NotNull(dashboard);
        Assert.Equal("Dora", dashboard!.FirstName);
        Assert.Equal("Durand", dashboard.LastName);
        Assert.Equal(VolunteerStatus.Pending.ToString(), dashboard.Status);
        Assert.NotNull(dashboard.Onboarding);
        Assert.Equal(1, dashboard.Onboarding.CompletedCount);
        Assert.Equal(4, dashboard.Onboarding.TotalCount);
        Assert.Equal(2, dashboard.Onboarding.CurrentStepNumber);
        Assert.False(dashboard.Onboarding.IsComplete);
        Assert.Empty(dashboard.UpcomingAssignments);
        Assert.Empty(dashboard.Trainings);
        Assert.Empty(dashboard.Certifications);
        Assert.Empty(dashboard.Alerts);
    }
}
