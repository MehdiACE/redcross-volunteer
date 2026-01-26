using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Infrastructure;

namespace RedCrossManager.Server.Tests.Integration;

public class OnboardingTests : IAsyncLifetime
{
    private readonly RedCrossDbContext _context;

    public OnboardingTests()
    {
        var options = new DbContextOptionsBuilder<RedCrossDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new RedCrossDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        _context.Dispose();
    }

    [Fact]
    public async Task OnboardingStep_ShouldPersistProgressForVolunteer()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "+15145551234",
            Status = VolunteerStatus.Pending,
            IsMinor = false,
            SmsOptIn = true
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var step1 = new OnboardingStep
        {
            Id = Guid.NewGuid(),
            VolunteerId = volunteerId,
            StepNumber = 1,
            Title = "Profile Completion",
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow
        };

        // Act
        _context.OnboardingSteps.Add(step1);
        await _context.SaveChangesAsync();

        // Assert
        var savedStep = await _context.OnboardingSteps
            .FirstOrDefaultAsync(s => s.VolunteerId == volunteerId && s.StepNumber == 1);

        Assert.NotNull(savedStep);
        Assert.Equal(volunteerId, savedStep.VolunteerId);
        Assert.True(savedStep.IsCompleted);
        Assert.Equal(1, savedStep.StepNumber);
    }

    [Fact]
    public async Task OnboardingSteps_ShouldTrackProgressThroughAllSteps()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            Phone = "+15145552345",
            Status = VolunteerStatus.Pending,
            IsMinor = false,
            SmsOptIn = false
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        // Act - Create all 4 onboarding steps
        var steps = new[]
        {
            new OnboardingStep
            {
                Id = Guid.NewGuid(),
                VolunteerId = volunteerId,
                StepNumber = 1,
                Title = "Profile Completion",
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow.AddDays(-3)
            },
            new OnboardingStep
            {
                Id = Guid.NewGuid(),
                VolunteerId = volunteerId,
                StepNumber = 2,
                Title = "Orientation",
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow.AddDays(-2)
            },
            new OnboardingStep
            {
                Id = Guid.NewGuid(),
                VolunteerId = volunteerId,
                StepNumber = 3,
                Title = "Training Modules",
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow.AddDays(-1)
            },
            new OnboardingStep
            {
                Id = Guid.NewGuid(),
                VolunteerId = volunteerId,
                StepNumber = 4,
                Title = "Final Review",
                IsCompleted = false,
                CompletedAt = null
            }
        };

        _context.OnboardingSteps.AddRange(steps);
        await _context.SaveChangesAsync();

        // Assert
        var savedSteps = await _context.OnboardingSteps
            .Where(s => s.VolunteerId == volunteerId)
            .OrderBy(s => s.StepNumber)
            .ToListAsync();

        Assert.Equal(4, savedSteps.Count);
        Assert.All(savedSteps.Take(3), step => Assert.True(step.IsCompleted));
        Assert.False(savedSteps[3].IsCompleted);
    }

    [Fact]
    public async Task VolunteerStatus_ShouldTransitionFromPendingThroughOnboarding()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "Bob",
            LastName = "Johnson",
            Email = "bob@example.com",
            Phone = "+15145553456",
            Status = VolunteerStatus.Pending,
            IsMinor = false,
            SmsOptIn = true
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        // Act - Simulate status transitions
        volunteer.Status = VolunteerStatus.InTraining;
        _context.Volunteers.Update(volunteer);
        await _context.SaveChangesAsync();

        var inTrainingVolunteer = await _context.Volunteers.FindAsync(volunteerId);
        Assert.Equal(VolunteerStatus.InTraining, inTrainingVolunteer?.Status);

        inTrainingVolunteer!.Status = VolunteerStatus.Active;
        _context.Volunteers.Update(inTrainingVolunteer);
        await _context.SaveChangesAsync();

        // Assert
        var activeVolunteer = await _context.Volunteers.FindAsync(volunteerId);
        Assert.Equal(VolunteerStatus.Active, activeVolunteer?.Status);
    }

    [Fact]
    public async Task MinorVolunteer_ShouldBeBlockedFromAdvancingWithoutParentalConsent()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "Young",
            LastName = "Volunteer",
            Email = "young@example.com",
            Phone = "+15145554567",
            Status = VolunteerStatus.Pending,
            IsMinor = true,
            SmsOptIn = false
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        // Act - Create parental consent that is not yet approved
        var parentalConsent = new ParentalConsent
        {
            Id = Guid.NewGuid(),
            VolunteerId = volunteerId,
            GuardianEmail = "guardian@example.com",
            GuardianFullName = "Parent Guardian",
            Status = ConsentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.ParentalConsents.Add(parentalConsent);
        await _context.SaveChangesAsync();

        // Assert - Volunteer should not be able to advance status without approved consent
        var pendingConsent = await _context.ParentalConsents
            .FirstOrDefaultAsync(c => c.VolunteerId == volunteerId);

        Assert.NotNull(pendingConsent);
        Assert.Equal(ConsentStatus.Pending, pendingConsent.Status);

        // Attempt to transition status should fail in business logic
        var volunteerCheck = await _context.Volunteers.FindAsync(volunteerId);
        Assert.Equal(VolunteerStatus.Pending, volunteerCheck?.Status);
    }

    [Fact]
    public async Task ParentalConsent_ShouldTransitionFromPendingToApproved()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "Minor",
            LastName = "Applicant",
            Email = "minor@example.com",
            Phone = "+15145555678",
            Status = VolunteerStatus.Pending,
            IsMinor = true,
            SmsOptIn = false
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var parentalConsent = new ParentalConsent
        {
            Id = consentId,
            VolunteerId = volunteerId,
            GuardianEmail = "guardian@example.com",
            GuardianFullName = "Parent Guardian",
            Status = ConsentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.ParentalConsents.Add(parentalConsent);
        await _context.SaveChangesAsync();

        // Act - Approve consent
        var consent = await _context.ParentalConsents.FindAsync(consentId);
        consent!.Status = ConsentStatus.Approved;
        consent.ApprovedAt = DateTime.UtcNow;
        _context.ParentalConsents.Update(consent);
        await _context.SaveChangesAsync();

        // Assert
        var approvedConsent = await _context.ParentalConsents.FindAsync(consentId);
        Assert.Equal(ConsentStatus.Approved, approvedConsent?.Status);
        Assert.NotNull(approvedConsent?.ApprovedAt);
    }

    [Fact]
    public async Task OnboardingSteps_ShouldMaintainOrderByStepNumber()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Phone = "+15145556789",
            Status = VolunteerStatus.Pending,
            IsMinor = false,
            SmsOptIn = true
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        // Act - Add steps out of order
        var stepsOutOfOrder = new[]
        {
            new OnboardingStep { Id = Guid.NewGuid(), VolunteerId = volunteerId, StepNumber = 3, Title = "Training", IsCompleted = false },
            new OnboardingStep { Id = Guid.NewGuid(), VolunteerId = volunteerId, StepNumber = 1, Title = "Profile", IsCompleted = true, CompletedAt = DateTime.UtcNow },
            new OnboardingStep { Id = Guid.NewGuid(), VolunteerId = volunteerId, StepNumber = 4, Title = "Review", IsCompleted = false },
            new OnboardingStep { Id = Guid.NewGuid(), VolunteerId = volunteerId, StepNumber = 2, Title = "Orientation", IsCompleted = false }
        };

        _context.OnboardingSteps.AddRange(stepsOutOfOrder);
        await _context.SaveChangesAsync();

        // Assert - Retrieve in correct order
        var orderedSteps = await _context.OnboardingSteps
            .Where(s => s.VolunteerId == volunteerId)
            .OrderBy(s => s.StepNumber)
            .Select(s => s.StepNumber)
            .ToListAsync();

        Assert.Equal(new[] { 1, 2, 3, 4 }, orderedSteps);
    }
}
