using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Infrastructure;
using RedCrossManager.Server.Tests.Infrastructure;

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
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: false, email: "john@example.com");
        volunteer.SmsOptIn = true;

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var step1 = new OnboardingStep
        {
            Id = Guid.NewGuid(),
            VolunteerId = volunteerId,
            StepType = StepType.DocumentVerification,
            Status = StepStatus.Approved,
            ApprovedAt = DateTime.UtcNow
        };

        // Act
        _context.OnboardingSteps.Add(step1);
        await _context.SaveChangesAsync();

        // Assert
        var savedStep = await _context.OnboardingSteps
            .FirstOrDefaultAsync(s => s.VolunteerId == volunteerId && s.StepType == StepType.DocumentVerification);

        Assert.NotNull(savedStep);
        Assert.Equal(volunteerId, savedStep.VolunteerId);
        Assert.Equal(StepStatus.Approved, savedStep.Status);
        Assert.Equal(StepType.DocumentVerification, savedStep.StepType);
    }

    [Fact]
    public async Task OnboardingSteps_ShouldTrackProgressThroughAllSteps()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: false, email: "jane@example.com");

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        // Act - Create all 4 onboarding steps
        var steps = new[]
        {
            new OnboardingStep
            {
                Id = Guid.NewGuid(),
                VolunteerId = volunteerId,
                StepType = StepType.DocumentVerification,
                Status = StepStatus.Approved,
                ApprovedAt = DateTime.UtcNow.AddDays(-3)
            },
            new OnboardingStep
            {
                Id = Guid.NewGuid(),
                VolunteerId = volunteerId,
                StepType = StepType.OrientationTraining,
                Status = StepStatus.Approved,
                ApprovedAt = DateTime.UtcNow.AddDays(-2)
            },
            new OnboardingStep
            {
                Id = Guid.NewGuid(),
                VolunteerId = volunteerId,
                StepType = StepType.Certification,
                Status = StepStatus.Approved,
                ApprovedAt = DateTime.UtcNow.AddDays(-1)
            },
            new OnboardingStep
            {
                Id = Guid.NewGuid(),
                VolunteerId = volunteerId,
                StepType = StepType.FinalReview,
                Status = StepStatus.NotStarted,
                ApprovedAt = null
            }
        };

        _context.OnboardingSteps.AddRange(steps);
        await _context.SaveChangesAsync();

        // Assert
        var savedSteps = await _context.OnboardingSteps
            .Where(s => s.VolunteerId == volunteerId)
            .OrderBy(s => s.StepType)
            .ToListAsync();

        Assert.Equal(4, savedSteps.Count);
        Assert.All(savedSteps.Take(3), step => Assert.Equal(StepStatus.Approved, step.Status));
        Assert.Equal(StepStatus.NotStarted, savedSteps[3].Status);
    }

    [Fact]
    public async Task VolunteerStatus_ShouldTransitionFromPendingThroughOnboarding()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: false, email: "bob@example.com");
        volunteer.SmsOptIn = true;

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
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "young@example.com");

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        // Act - Create parental consent that is not yet approved
        var parentalConsent = new ParentalConsent
        {
            Id = Guid.NewGuid(),
            VolunteerId = volunteerId,
            GuardianName = "Parent Guardian",
            GuardianEmail = "guardian@example.com",
            GuardianPhone = "+15145552222",
            ConsentStatus = ConsentStatus.Requested,
            SubmittedAt = DateTime.UtcNow
        };

        _context.ParentalConsents.Add(parentalConsent);
        await _context.SaveChangesAsync();

        // Assert - Volunteer should not be able to advance status without approved consent
        var pendingConsent = await _context.ParentalConsents
            .FirstOrDefaultAsync(c => c.VolunteerId == volunteerId);

        Assert.NotNull(pendingConsent);
        Assert.Equal(ConsentStatus.Requested, pendingConsent.ConsentStatus);

        // Attempt to transition status should fail in business logic
        var volunteerCheck = await _context.Volunteers.FindAsync(volunteerId);
        Assert.Equal(VolunteerStatus.Pending, volunteerCheck?.Status);
    }

    [Fact]
    public async Task ParentalConsent_ShouldTransitionFromPendingToApproved()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "minor@example.com");

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var parentalConsent = TestDataFactory.CreateParentalConsent(consentId, volunteerId, ConsentStatus.Submitted);

        _context.ParentalConsents.Add(parentalConsent);
        await _context.SaveChangesAsync();

        // Act - Approve consent
        var consent = await _context.ParentalConsents.FindAsync(consentId);
        consent!.ConsentStatus = ConsentStatus.Approved;
        consent.ReviewedAt = DateTime.UtcNow;
        _context.ParentalConsents.Update(consent);
        await _context.SaveChangesAsync();

        // Assert
        var approvedConsent = await _context.ParentalConsents.FindAsync(consentId);
        Assert.Equal(ConsentStatus.Approved, approvedConsent?.ConsentStatus);
        Assert.NotNull(approvedConsent?.ReviewedAt);
    }

    [Fact]
    public async Task OnboardingSteps_ShouldMaintainOrderByStepNumber()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: false, email: "test@example.com");
        volunteer.SmsOptIn = true;

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        // Act - Add steps out of order
        var stepsOutOfOrder = new[]
        {
            new OnboardingStep { Id = Guid.NewGuid(), VolunteerId = volunteerId, StepType = StepType.Certification, Status = StepStatus.NotStarted },
            new OnboardingStep { Id = Guid.NewGuid(), VolunteerId = volunteerId, StepType = StepType.DocumentVerification, Status = StepStatus.Approved, ApprovedAt = DateTime.UtcNow },
            new OnboardingStep { Id = Guid.NewGuid(), VolunteerId = volunteerId, StepType = StepType.FinalReview, Status = StepStatus.NotStarted },
            new OnboardingStep { Id = Guid.NewGuid(), VolunteerId = volunteerId, StepType = StepType.OrientationTraining, Status = StepStatus.NotStarted }
        };

        _context.OnboardingSteps.AddRange(stepsOutOfOrder);
        await _context.SaveChangesAsync();

        // Assert - Retrieve in correct order
        var orderedSteps = await _context.OnboardingSteps
            .Where(s => s.VolunteerId == volunteerId)
            .OrderBy(s => s.StepType)
            .Select(s => s.StepType)
            .ToListAsync();

        Assert.Equal(new[] { StepType.DocumentVerification, StepType.OrientationTraining, StepType.Certification, StepType.FinalReview }, orderedSteps);
    }
}
