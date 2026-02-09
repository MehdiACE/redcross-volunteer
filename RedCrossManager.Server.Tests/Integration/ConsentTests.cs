using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Infrastructure;
using RedCrossManager.Server.Tests.Infrastructure;

namespace RedCrossManager.Server.Tests.Integration;

public class ConsentTests : IAsyncLifetime
{
    private readonly RedCrossDbContext _context;

    public ConsentTests()
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
    public async Task ParentalConsent_ShouldCreateWithPendingStatus()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "minor@example.com");

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        // Act
        var consentId = Guid.NewGuid();
        var consent = TestDataFactory.CreateParentalConsent(consentId, volunteerId, ConsentStatus.Requested);

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Assert
        var savedConsent = await _context.ParentalConsents.FindAsync(consentId);
        Assert.NotNull(savedConsent);
        Assert.Equal(volunteerId, savedConsent.VolunteerId);
        Assert.Equal(ConsentStatus.Requested, savedConsent.ConsentStatus);
        Assert.Null(savedConsent.ReviewedAt);
    }

    [Fact]
    public async Task ParentalConsent_ShouldTransitionFromPendingToApproved()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "young@example.com");

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var consent = TestDataFactory.CreateParentalConsent(consentId, volunteerId, ConsentStatus.Submitted);

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act - Approve consent
        var dbConsent = await _context.ParentalConsents.FindAsync(consentId);
        dbConsent!.ConsentStatus = ConsentStatus.Approved;
        dbConsent.ReviewedAt = DateTime.UtcNow;
        _context.ParentalConsents.Update(dbConsent);
        await _context.SaveChangesAsync();

        // Assert
        var approvedConsent = await _context.ParentalConsents.FindAsync(consentId);
        Assert.Equal(ConsentStatus.Approved, approvedConsent?.ConsentStatus);
        Assert.NotNull(approvedConsent?.ReviewedAt);
    }

    [Fact]
    public async Task ParentalConsent_ShouldTransitionFromPendingToRejected()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "another@example.com");

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var consent = TestDataFactory.CreateParentalConsent(consentId, volunteerId, ConsentStatus.Submitted);

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act - Reject consent
        var dbConsent = await _context.ParentalConsents.FindAsync(consentId);
        dbConsent!.ConsentStatus = ConsentStatus.Rejected;
        dbConsent.ReviewedAt = DateTime.UtcNow;
        dbConsent.ReviewerNotes = "Does not meet criteria";
        _context.ParentalConsents.Update(dbConsent);
        await _context.SaveChangesAsync();

        // Assert
        var rejectedConsent = await _context.ParentalConsents.FindAsync(consentId);
        Assert.Equal(ConsentStatus.Rejected, rejectedConsent?.ConsentStatus);
        Assert.NotNull(rejectedConsent?.ReviewedAt);
        Assert.NotNull(rejectedConsent?.ReviewerNotes);
    }

    [Fact]
    public async Task ParentalConsent_ShouldTrackSLAFor48HourReview()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "sla@example.com");

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddHours(-36); // Created 36 hours ago
        var consent = TestDataFactory.CreateParentalConsent(consentId, volunteerId, ConsentStatus.Requested);
        consent.SubmittedAt = createdAt;

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act - Check if SLA is approaching (within 48 hours)
        var savedConsent = await _context.ParentalConsents.FindAsync(consentId);
        var hoursElapsed = (DateTime.UtcNow - savedConsent!.SubmittedAt!.Value).TotalHours;
        var isWithinSLA = hoursElapsed < 48;

        // Assert
        Assert.True(isWithinSLA, "Consent should be within 48-hour SLA window");
        Assert.InRange(hoursElapsed, 35, 37);
    }

    [Fact]
    public async Task ParentalConsent_ShouldTrackSLAOverdueWhen48HoursPassed()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "overdue@example.com");

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddHours(-72); // Created 72 hours ago (overdue)
        var consent = TestDataFactory.CreateParentalConsent(consentId, volunteerId, ConsentStatus.Requested);
        consent.SubmittedAt = createdAt;

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act
        var savedConsent = await _context.ParentalConsents.FindAsync(consentId);
        var hoursElapsed = (DateTime.UtcNow - savedConsent!.SubmittedAt!.Value).TotalHours;
        var isOverdue = hoursElapsed >= 48 && savedConsent.ConsentStatus == ConsentStatus.Requested;

        // Assert
        Assert.True(isOverdue, "Consent should be marked as overdue");
        Assert.InRange(hoursElapsed, 70, 74);
    }

    [Fact]
    public async Task ParentalConsent_ShouldPreserveIdentityVerificationToken()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "token@example.com");

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var consent = TestDataFactory.CreateParentalConsent(consentId, volunteerId, ConsentStatus.Requested);

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act
        var savedConsent = await _context.ParentalConsents.FindAsync(consentId);

        // Assert
        Assert.NotNull(savedConsent);
        Assert.Equal(IdentityVerificationStatus.NotVerified, savedConsent!.IdentityVerificationStatus);
    }

    [Fact]
    public async Task ParentalConsent_ShouldMarkEmailAsVerifiedWhenTokenUsed()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "verify@example.com");

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var consent = TestDataFactory.CreateParentalConsent(consentId, volunteerId, ConsentStatus.Requested);

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act - Verify email
        var dbConsent = await _context.ParentalConsents.FindAsync(consentId);
        dbConsent!.IdentityVerificationStatus = IdentityVerificationStatus.EmailConfirmed;
        _context.ParentalConsents.Update(dbConsent);
        await _context.SaveChangesAsync();

        // Assert
        var verifiedConsent = await _context.ParentalConsents.FindAsync(consentId);
        Assert.Equal(IdentityVerificationStatus.EmailConfirmed, verifiedConsent?.IdentityVerificationStatus);
    }

    [Fact]
    public async Task ParentalConsent_ShouldAllowSmsOptInSelection()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "sms@example.com");

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var consent = TestDataFactory.CreateParentalConsent(consentId, volunteerId, ConsentStatus.Requested);
        consent.SmsOptIn = true;

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act
        var savedConsent = await _context.ParentalConsents.FindAsync(consentId);

        // Assert
        Assert.True(savedConsent?.SmsOptIn);
    }
}
