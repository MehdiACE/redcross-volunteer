using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.DTOs.Missions;
using RedCrossManager.Server.Infrastructure;
using RedCrossManager.Server.Tests.Infrastructure;
using Xunit;

namespace RedCrossManager.Server.Tests.Integration;

public class MissionsTests : IAsyncLifetime
{
    private TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private string _coordinatorToken = null!;
    private string _volunteerToken = null!;
    private Guid _coordinatorId;
    private Guid _volunteerId;

    public async Task InitializeAsync()
    {
        _factory = new TestWebApplicationFactory();
        _client = _factory.CreateClient();
        await _factory.InitializeDatabaseAsync();

        var seedResult = await _factory.SeedTestUsersAsync();
        _coordinatorToken = seedResult["coordinator"];
        _volunteerToken = seedResult["volunteer"];
        _coordinatorId = Guid.Parse(seedResult["coordinatorId"]);
        _volunteerId = Guid.Parse(seedResult["volunteerId"]);

        await SeedVolunteerCertificationAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Fact]
    public async Task CreateMission_WithValidData_ReturnsCreatedMission()
    {
        var createDto = new CreateMissionDto
        {
            Title = "Blood Drive Support",
            Description = "Assist with donor intake and logistics",
            MissionType = "BloodDrive",
            Location = "Montreal Center",
            StartAt = DateTime.UtcNow.AddDays(7),
            EndAt = DateTime.UtcNow.AddDays(7).AddHours(4),
            RequiredCertifications = new List<string> { "FirstAid" },
            VolunteersNeeded = 5,
            TravelBufferMinutes = 120,
            Published = true,
            CreatedByCoordinatorId = _coordinatorId
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _coordinatorToken);
        var response = await _client.PostAsJsonAsync("/api/v1/missions", createDto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var mission = await response.Content.ReadFromJsonAsync<MissionDto>();
        Assert.NotNull(mission);
        Assert.Equal(createDto.Title, mission!.Title);
        Assert.Equal(createDto.MissionType, mission.MissionType);
        Assert.Equal(createDto.Location, mission.Location);
    }

    [Fact]
    public async Task ApplyAssignConfirmFlow_WorksForQualifiedVolunteer()
    {
        var mission = await CreateMissionAsync();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _volunteerToken);
        var applyResponse = await _client.PostAsJsonAsync($"/api/v1/missions/{mission.Id}/apply", new ApplyMissionDto
        {
            VolunteerId = _volunteerId
        });

        Assert.Equal(HttpStatusCode.Created, applyResponse.StatusCode);

        var appliedAssignment = await applyResponse.Content.ReadFromJsonAsync<AssignmentDto>();
        Assert.NotNull(appliedAssignment);
        Assert.Equal("Pending", appliedAssignment!.Status);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _coordinatorToken);
        var assignResponse = await _client.PostAsJsonAsync($"/api/v1/missions/{mission.Id}/assign", new AssignVolunteersDto
        {
            VolunteerIds = new List<Guid> { _volunteerId },
            RoleDescription = "Support"
        });

        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);

        var assignments = await assignResponse.Content.ReadFromJsonAsync<List<AssignmentDto>>();
        Assert.NotNull(assignments);
        Assert.Single(assignments!);
        Assert.Equal("Confirmed", assignments![0].Status);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _volunteerToken);
        var confirmResponse = await _client.PostAsync($"/api/v1/assignments/{assignments[0].Id}/confirm", null);

        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

        var confirmedAssignment = await confirmResponse.Content.ReadFromJsonAsync<AssignmentDto>();
        Assert.NotNull(confirmedAssignment);
        Assert.Equal("Confirmed", confirmedAssignment!.Status);
    }

    private async Task<MissionDto> CreateMissionAsync()
    {
        var createDto = new CreateMissionDto
        {
            Title = "First Aid Station",
            Description = "Provide first aid support during event",
            MissionType = "CommunityProgram",
            Location = "Quebec City",
            StartAt = DateTime.UtcNow.AddDays(10),
            EndAt = DateTime.UtcNow.AddDays(10).AddHours(3),
            RequiredCertifications = new List<string> { "FirstAid" },
            VolunteersNeeded = 2,
            TravelBufferMinutes = 120,
            Published = true,
            CreatedByCoordinatorId = _coordinatorId
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _coordinatorToken);
        var response = await _client.PostAsJsonAsync("/api/v1/missions", createDto);
        response.EnsureSuccessStatusCode();

        var mission = await response.Content.ReadFromJsonAsync<MissionDto>();
        return mission!;
    }

    private async Task SeedVolunteerCertificationAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RedCrossDbContext>();

        var cert = new Certification
        {
            Id = Guid.NewGuid(),
            VolunteerId = _volunteerId,
            Type = CertificationType.FirstAid,
            IssuedAt = DateTime.UtcNow.AddMonths(-2),
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            Issuer = "Red Cross",
            VerificationStatus = VerificationStatus.Approved
        };

        dbContext.Certifications.Add(cert);
        await dbContext.SaveChangesAsync();
    }
}
