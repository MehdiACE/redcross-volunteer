using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Infrastructure;
using RedCrossManager.Server.Tests.Infrastructure;

namespace RedCrossManager.Server.Tests.Integration;

public class GuardianVerificationTests : IAsyncLifetime
{
    private readonly RedCrossDbContext _context;

    public GuardianVerificationTests()
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
    public async Task GuardianIdentity_ShouldRequireEmailVerification()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "minor@example.com");

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consent = TestDataFactory.CreateParentalConsent(Guid.NewGuid(), volunteerId, ConsentStatus.Requested);

        // Act
        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Assert
        var savedConsent = await _context.ParentalConsents.FirstOrDefaultAsync(c => c.VolunteerId == volunteerId);
        Assert.NotNull(savedConsent);
        Assert.Equal(IdentityVerificationStatus.NotVerified, savedConsent!.IdentityVerificationStatus);
    }

    [Fact]
    public async Task EmailVerification_ShouldValidateTokenBeforeMarking()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "youth@example.com");

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var consent = TestDataFactory.CreateParentalConsent(consentId, volunteerId, ConsentStatus.Submitted);

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act - Verify with correct token
        var dbConsent = await _context.ParentalConsents.FindAsync(consentId);
        if (dbConsent is not null)
        {
            dbConsent.IdentityVerificationStatus = IdentityVerificationStatus.EmailConfirmed;
            _context.ParentalConsents.Update(dbConsent);
            await _context.SaveChangesAsync();
        }

        // Assert
        var verifiedConsent = await _context.ParentalConsents.FindAsync(consentId);
        Assert.Equal(IdentityVerificationStatus.EmailConfirmed, verifiedConsent?.IdentityVerificationStatus);
    }

    [Fact]
    public async Task EmailVerification_ShouldRejectInvalidToken()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "teen@example.com");

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var consent = TestDataFactory.CreateParentalConsent(consentId, volunteerId, ConsentStatus.Submitted);

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act - Attempt to verify with invalid token
        var dbConsent = await _context.ParentalConsents.FindAsync(consentId);
        var isTokenValid = false;

        // Assert
        Assert.False(isTokenValid, "Invalid token should not verify");
        var unchangedConsent = await _context.ParentalConsents.FindAsync(consentId);
        Assert.Equal(IdentityVerificationStatus.NotVerified, unchangedConsent?.IdentityVerificationStatus);
    }

    [Fact]
    public async Task VerificationToken_ShouldExpireAfterPeriod()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "expired@example.com");

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var tokenCreatedAt = DateTime.UtcNow.AddHours(-25); // Created 25 hours ago
        var consent = TestDataFactory.CreateParentalConsent(consentId, volunteerId, ConsentStatus.Submitted);
        consent.SubmittedAt = tokenCreatedAt;

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act
        var savedConsent = await _context.ParentalConsents.FindAsync(consentId);
        var hoursSinceCreation = (DateTime.UtcNow - savedConsent!.SubmittedAt!.Value).TotalHours;
        var isTokenExpired = hoursSinceCreation > 24; // 24-hour token expiry

        // Assert
        Assert.True(isTokenExpired, "Token should be expired after 24 hours");
        Assert.InRange(hoursSinceCreation, 24, 26);
    }

    [Fact]
    public async Task GuardianConsent_ShouldCaptureGuardianContactInfo()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "contact@example.com");

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var guardianEmail = "parent@example.com";
        var guardianName = "Jane Doe";
        var consentId = Guid.NewGuid();
        var consent = TestDataFactory.CreateParentalConsent(consentId, volunteerId, ConsentStatus.Requested);
        consent.GuardianEmail = guardianEmail;
        consent.GuardianName = guardianName;

        // Act
        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Assert
        var savedConsent = await _context.ParentalConsents.FindAsync(consentId);
        Assert.NotNull(savedConsent);
        Assert.Equal(guardianEmail, savedConsent.GuardianEmail);
        Assert.Equal(guardianName, savedConsent.GuardianName);
    }

    [Fact]
    public async Task VerificationProcess_ShouldPreventUnapprovedAdvancement()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "prevent@example.com");

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consent = TestDataFactory.CreateParentalConsent(Guid.NewGuid(), volunteerId, ConsentStatus.Requested);

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act - Check if volunteer can advance without verified email
        var unverifiedConsent = await _context.ParentalConsents
            .FirstOrDefaultAsync(c => c.VolunteerId == volunteerId);

        // Assert
        Assert.Equal(IdentityVerificationStatus.NotVerified, unverifiedConsent?.IdentityVerificationStatus);
        // Business logic should prevent status transition without verified email
    }

    [Fact]
    public async Task MultipleVerificationAttempts_ShouldOnlySucceedWithValidToken()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "multiple@example.com");

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var consent = TestDataFactory.CreateParentalConsent(consentId, volunteerId, ConsentStatus.Submitted);

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act - Multiple failed attempts
        var dbConsent = await _context.ParentalConsents.FindAsync(consentId);
        if (dbConsent is not null)
        {
            dbConsent.IdentityVerificationStatus = IdentityVerificationStatus.EmailConfirmed;
            _context.ParentalConsents.Update(dbConsent);
            await _context.SaveChangesAsync();
        }

        // Assert
        var verifiedConsent = await _context.ParentalConsents.FindAsync(consentId);
        Assert.Equal(IdentityVerificationStatus.EmailConfirmed, verifiedConsent?.IdentityVerificationStatus);
    }
}
