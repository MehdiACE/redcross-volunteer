using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Infrastructure;

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
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "Minor",
            LastName = "Child",
            Email = "minor@example.com",
            Phone = "+15145551234",
            Status = VolunteerStatus.Pending,
            IsMinor = true,
            SmsOptIn = false
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        // Act
        var consentId = Guid.NewGuid();
        var consent = new ParentalConsent
        {
            Id = consentId,
            VolunteerId = volunteerId,
            GuardianEmail = "parent@example.com",
            GuardianFullName = "Parent Name",
            Status = ConsentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Assert
        var savedConsent = await _context.ParentalConsents.FindAsync(consentId);
        Assert.NotNull(savedConsent);
        Assert.Equal(volunteerId, savedConsent.VolunteerId);
        Assert.Equal(ConsentStatus.Pending, savedConsent.Status);
        Assert.Null(savedConsent.ApprovedAt);
    }

    [Fact]
    public async Task ParentalConsent_ShouldTransitionFromPendingToApproved()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "Young",
            LastName = "Applicant",
            Email = "young@example.com",
            Phone = "+15145552345",
            Status = VolunteerStatus.Pending,
            IsMinor = true,
            SmsOptIn = false
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var consent = new ParentalConsent
        {
            Id = consentId,
            VolunteerId = volunteerId,
            GuardianEmail = "parent@example.com",
            GuardianFullName = "Parent Name",
            Status = ConsentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act - Approve consent
        var dbConsent = await _context.ParentalConsents.FindAsync(consentId);
        dbConsent!.Status = ConsentStatus.Approved;
        dbConsent.ApprovedAt = DateTime.UtcNow;
        _context.ParentalConsents.Update(dbConsent);
        await _context.SaveChangesAsync();

        // Assert
        var approvedConsent = await _context.ParentalConsents.FindAsync(consentId);
        Assert.Equal(ConsentStatus.Approved, approvedConsent?.Status);
        Assert.NotNull(approvedConsent?.ApprovedAt);
    }

    [Fact]
    public async Task ParentalConsent_ShouldTransitionFromPendingToRejected()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "Another",
            LastName = "Minor",
            Email = "another@example.com",
            Phone = "+15145553456",
            Status = VolunteerStatus.Pending,
            IsMinor = true,
            SmsOptIn = false
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var consent = new ParentalConsent
        {
            Id = consentId,
            VolunteerId = volunteerId,
            GuardianEmail = "parent@example.com",
            GuardianFullName = "Parent Name",
            Status = ConsentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act - Reject consent
        var dbConsent = await _context.ParentalConsents.FindAsync(consentId);
        dbConsent!.Status = ConsentStatus.Rejected;
        dbConsent.RejectedAt = DateTime.UtcNow;
        dbConsent.RejectionReason = "Does not meet criteria";
        _context.ParentalConsents.Update(dbConsent);
        await _context.SaveChangesAsync();

        // Assert
        var rejectedConsent = await _context.ParentalConsents.FindAsync(consentId);
        Assert.Equal(ConsentStatus.Rejected, rejectedConsent?.Status);
        Assert.NotNull(rejectedConsent?.RejectedAt);
        Assert.NotNull(rejectedConsent?.RejectionReason);
    }

    [Fact]
    public async Task ParentalConsent_ShouldTrackSLAFor48HourReview()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "SLA",
            LastName = "Test",
            Email = "sla@example.com",
            Phone = "+15145554567",
            Status = VolunteerStatus.Pending,
            IsMinor = true,
            SmsOptIn = false
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddHours(-36); // Created 36 hours ago
        var consent = new ParentalConsent
        {
            Id = consentId,
            VolunteerId = volunteerId,
            GuardianEmail = "parent@example.com",
            GuardianFullName = "Parent Name",
            Status = ConsentStatus.Pending,
            CreatedAt = createdAt
        };

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act - Check if SLA is approaching (within 48 hours)
        var savedConsent = await _context.ParentalConsents.FindAsync(consentId);
        var hoursElapsed = (DateTime.UtcNow - savedConsent!.CreatedAt).TotalHours;
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
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "Overdue",
            LastName = "Test",
            Email = "overdue@example.com",
            Phone = "+15145555678",
            Status = VolunteerStatus.Pending,
            IsMinor = true,
            SmsOptIn = false
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddHours(-72); // Created 72 hours ago (overdue)
        var consent = new ParentalConsent
        {
            Id = consentId,
            VolunteerId = volunteerId,
            GuardianEmail = "parent@example.com",
            GuardianFullName = "Parent Name",
            Status = ConsentStatus.Pending,
            CreatedAt = createdAt
        };

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act
        var savedConsent = await _context.ParentalConsents.FindAsync(consentId);
        var hoursElapsed = (DateTime.UtcNow - savedConsent!.CreatedAt).TotalHours;
        var isOverdue = hoursElapsed >= 48 && savedConsent.Status == ConsentStatus.Pending;

        // Assert
        Assert.True(isOverdue, "Consent should be marked as overdue");
        Assert.InRange(hoursElapsed, 70, 74);
    }

    [Fact]
    public async Task ParentalConsent_ShouldPreserveIdentityVerificationToken()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "Token",
            LastName = "Test",
            Email = "token@example.com",
            Phone = "+15145556789",
            Status = VolunteerStatus.Pending,
            IsMinor = true,
            SmsOptIn = false
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var verificationToken = Guid.NewGuid().ToString();
        var consent = new ParentalConsent
        {
            Id = consentId,
            VolunteerId = volunteerId,
            GuardianEmail = "parent@example.com",
            GuardianFullName = "Parent Name",
            Status = ConsentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            EmailVerificationToken = verificationToken,
            IsEmailVerified = false
        };

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act
        var savedConsent = await _context.ParentalConsents.FindAsync(consentId);

        // Assert
        Assert.NotNull(savedConsent?.EmailVerificationToken);
        Assert.Equal(verificationToken, savedConsent.EmailVerificationToken);
        Assert.False(savedConsent.IsEmailVerified);
    }

    [Fact]
    public async Task ParentalConsent_ShouldMarkEmailAsVerifiedWhenTokenUsed()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "Verify",
            LastName = "Test",
            Email = "verify@example.com",
            Phone = "+15145557890",
            Status = VolunteerStatus.Pending,
            IsMinor = true,
            SmsOptIn = false
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var verificationToken = Guid.NewGuid().ToString();
        var consent = new ParentalConsent
        {
            Id = consentId,
            VolunteerId = volunteerId,
            GuardianEmail = "parent@example.com",
            GuardianFullName = "Parent Name",
            Status = ConsentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            EmailVerificationToken = verificationToken,
            IsEmailVerified = false
        };

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act - Verify email
        var dbConsent = await _context.ParentalConsents.FindAsync(consentId);
        dbConsent!.IsEmailVerified = true;
        dbConsent.EmailVerificationToken = null;
        _context.ParentalConsents.Update(dbConsent);
        await _context.SaveChangesAsync();

        // Assert
        var verifiedConsent = await _context.ParentalConsents.FindAsync(consentId);
        Assert.True(verifiedConsent?.IsEmailVerified);
        Assert.Null(verifiedConsent?.EmailVerificationToken);
    }

    [Fact]
    public async Task ParentalConsent_ShouldAllowSmsOptInSelection()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = new Volunteer
        {
            Id = volunteerId,
            FirstName = "SMS",
            LastName = "Test",
            Email = "sms@example.com",
            Phone = "+15145558901",
            Status = VolunteerStatus.Pending,
            IsMinor = true,
            SmsOptIn = false
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var consentId = Guid.NewGuid();
        var consent = new ParentalConsent
        {
            Id = consentId,
            VolunteerId = volunteerId,
            GuardianEmail = "parent@example.com",
            GuardianFullName = "Parent Name",
            Status = ConsentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            SmsOptIn = true
        };

        _context.ParentalConsents.Add(consent);
        await _context.SaveChangesAsync();

        // Act
        var savedConsent = await _context.ParentalConsents.FindAsync(consentId);

        // Assert
        Assert.True(savedConsent?.SmsOptIn);
    }
}
