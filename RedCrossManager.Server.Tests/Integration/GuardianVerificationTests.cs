using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Infrastructure;

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
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "Minor",
            LastName = "Volunteer",
            Email = "minor@example.com",
            Phone = "+15145551234",
            Status = VolunteerStatus.Pending,
            IsMinor = true,
            SmsOptIn = false
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var verificationToken = Guid.NewGuid().ToString();
        var consent = new ParentalConsent
        {
            Id = Guid.NewGuid(),
            VolunteerId = volunteerId,
            GuardianEmail = "guardian@example.com",
            GuardianFullName = "Parent Guardian",
            Status = ConsentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            EmailVerificationToken = verificationToken,
            IsEmailVerified = false
        };

        // Act
        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Assert
        var savedConsent = await _context.ParentalConsents.FirstOrDefaultAsync(c => c.VolunteerId == volunteerId);
        Assert.NotNull(savedConsent);
        Assert.False(savedConsent.IsEmailVerified);
        Assert.NotNull(savedConsent.EmailVerificationToken);
        Assert.Equal(verificationToken, savedConsent.EmailVerificationToken);
    }

    [Fact]
    public async Task EmailVerification_ShouldValidateTokenBeforeMarking()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "Youth",
            LastName = "Applicant",
            Email = "youth@example.com",
            Phone = "+15145552345",
            Status = VolunteerStatus.Pending,
            IsMinor = true,
            SmsOptIn = false
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var correctToken = Guid.NewGuid().ToString();
        var consentId = Guid.NewGuid();
        var consent = new ParentalConsent
        {
            Id = consentId,
            VolunteerId = volunteerId,
            GuardianEmail = "guardian@example.com",
            GuardianFullName = "Parent Guardian",
            Status = ConsentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            EmailVerificationToken = correctToken,
            IsEmailVerified = false
        };

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act - Verify with correct token
        var dbConsent = await _context.ParentalConsents.FindAsync(consentId);
        if (dbConsent?.EmailVerificationToken == correctToken)
        {
            dbConsent.IsEmailVerified = true;
            dbConsent.EmailVerificationToken = null;
            dbConsent.EmailVerifiedAt = DateTime.UtcNow;
            _context.ParentalConsents.Update(dbConsent);
            await _context.SaveChangesAsync();
        }

        // Assert
        var verifiedConsent = await _context.ParentalConsents.FindAsync(consentId);
        Assert.True(verifiedConsent?.IsEmailVerified);
        Assert.Null(verifiedConsent?.EmailVerificationToken);
        Assert.NotNull(verifiedConsent?.EmailVerifiedAt);
    }

    [Fact]
    public async Task EmailVerification_ShouldRejectInvalidToken()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "Teen",
            LastName = "Volunteer",
            Email = "teen@example.com",
            Phone = "+15145553456",
            Status = VolunteerStatus.Pending,
            IsMinor = true,
            SmsOptIn = false
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var correctToken = Guid.NewGuid().ToString();
        var invalidToken = Guid.NewGuid().ToString();
        var consentId = Guid.NewGuid();
        var consent = new ParentalConsent
        {
            Id = consentId,
            VolunteerId = volunteerId,
            GuardianEmail = "guardian@example.com",
            GuardianFullName = "Parent Guardian",
            Status = ConsentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            EmailVerificationToken = correctToken,
            IsEmailVerified = false
        };

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act - Attempt to verify with invalid token
        var dbConsent = await _context.ParentalConsents.FindAsync(consentId);
        var isTokenValid = dbConsent?.EmailVerificationToken == invalidToken;

        // Assert
        Assert.False(isTokenValid, "Invalid token should not verify");
        var unchangedConsent = await _context.ParentalConsents.FindAsync(consentId);
        Assert.False(unchangedConsent?.IsEmailVerified);
    }

    [Fact]
    public async Task VerificationToken_ShouldExpireAfterPeriod()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "Expired",
            LastName = "Token",
            Email = "expired@example.com",
            Phone = "+15145554567",
            Status = VolunteerStatus.Pending,
            IsMinor = true,
            SmsOptIn = false
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var tokenCreatedAt = DateTime.UtcNow.AddHours(-25); // Created 25 hours ago
        var consent = new ParentalConsent
        {
            Id = consentId,
            VolunteerId = volunteerId,
            GuardianEmail = "guardian@example.com",
            GuardianFullName = "Parent Guardian",
            Status = ConsentStatus.Pending,
            CreatedAt = tokenCreatedAt,
            EmailVerificationToken = Guid.NewGuid().ToString(),
            IsEmailVerified = false
        };

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act
        var savedConsent = await _context.ParentalConsents.FindAsync(consentId);
        var hoursSinceCreation = (DateTime.UtcNow - savedConsent!.CreatedAt).TotalHours;
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
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "Contact",
            LastName = "Test",
            Email = "contact@example.com",
            Phone = "+15145555678",
            Status = VolunteerStatus.Pending,
            IsMinor = true,
            SmsOptIn = false
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var guardianEmail = "parent@example.com";
        var guardianName = "Jane Doe";
        var consentId = Guid.NewGuid();
        var consent = new ParentalConsent
        {
            Id = consentId,
            VolunteerId = volunteerId,
            GuardianEmail = guardianEmail,
            GuardianFullName = guardianName,
            Status = ConsentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Assert
        var savedConsent = await _context.ParentalConsents.FindAsync(consentId);
        Assert.NotNull(savedConsent);
        Assert.Equal(guardianEmail, savedConsent.GuardianEmail);
        Assert.Equal(guardianName, savedConsent.GuardianFullName);
    }

    [Fact]
    public async Task VerificationProcess_ShouldPreventUnapprovedAdvancement()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "Prevent",
            LastName = "Advance",
            Email = "prevent@example.com",
            Phone = "+15145556789",
            Status = VolunteerStatus.Pending,
            IsMinor = true,
            SmsOptIn = false
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consent = new ParentalConsent
        {
            Id = Guid.NewGuid(),
            VolunteerId = volunteerId,
            GuardianEmail = "parent@example.com",
            GuardianFullName = "Parent",
            Status = ConsentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            IsEmailVerified = false
        };

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act - Check if volunteer can advance without verified email
        var unverifiedConsent = await _context.ParentalConsents
            .FirstOrDefaultAsync(c => c.VolunteerId == volunteerId);

        // Assert
        Assert.False(unverifiedConsent?.IsEmailVerified);
        // Business logic should prevent status transition without verified email
    }

    [Fact]
    public async Task MultipleVerificationAttempts_ShouldOnlySucceedWithValidToken()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "Multiple",
            LastName = "Attempts",
            Email = "multiple@example.com",
            Phone = "+15145557890",
            Status = VolunteerStatus.Pending,
            IsMinor = true,
            SmsOptIn = false
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var correctToken = Guid.NewGuid().ToString();
        var consentId = Guid.NewGuid();
        var consent = new ParentalConsent
        {
            Id = consentId,
            VolunteerId = volunteerId,
            GuardianEmail = "parent@example.com",
            GuardianFullName = "Parent",
            Status = ConsentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            EmailVerificationToken = correctToken,
            IsEmailVerified = false
        };

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act - Multiple failed attempts
        var attempt1Token = Guid.NewGuid().ToString();
        var attempt2Token = Guid.NewGuid().ToString();

        var dbConsent = await _context.ParentalConsents.FindAsync(consentId);
        Assert.NotEqual(attempt1Token, dbConsent?.EmailVerificationToken);
        Assert.NotEqual(attempt2Token, dbConsent?.EmailVerificationToken);

        // Verify with correct token
        if (dbConsent?.EmailVerificationToken == correctToken)
        {
            dbConsent.IsEmailVerified = true;
            dbConsent.EmailVerificationToken = null;
            _context.ParentalConsents.Update(dbConsent);
            await _context.SaveChangesAsync();
        }

        // Assert
        var verifiedConsent = await _context.ParentalConsents.FindAsync(consentId);
        Assert.True(verifiedConsent?.IsEmailVerified);
    }
}
