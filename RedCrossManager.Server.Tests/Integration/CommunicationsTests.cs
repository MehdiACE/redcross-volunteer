using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.Infrastructure;
using RedCrossManager.Server.Tests.Infrastructure;

namespace RedCrossManager.Server.Tests.Integration;

public class CommunicationsTests : IAsyncLifetime
{
    private readonly RedCrossDbContext _context;

    public CommunicationsTests()
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
    public async Task CommunicationMessage_ShouldCreateWithSegmentAndChannels()
    {
        // Arrange
        var message = new CommunicationMessage
        {
            Id = Guid.NewGuid(),
            Segment = "B1J - Missing Consent",
            Channels = CommunicationChannel.Email | CommunicationChannel.SMS,
            Language = "fr",
            Subject = "Consentement parental requis",
            BodyTemplate = "Bonjour, nous avons besoin du consentement parental...",
            SentAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        // Act
        _context.CommunicationMessages.Add(message);
        await _context.SaveChangesAsync();

        // Assert
        var savedMessage = await _context.CommunicationMessages.FindAsync(message.Id);
        Assert.NotNull(savedMessage);
        Assert.Equal("B1J - Missing Consent", savedMessage.Segment);
        Assert.True(savedMessage.Channels.HasFlag(CommunicationChannel.Email));
        Assert.True(savedMessage.Channels.HasFlag(CommunicationChannel.SMS));
    }

    [Fact]
    public async Task CommunicationRecipient_ShouldTrackDeliveryStatus()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "young@example.com");
        volunteer.SmsOptIn = true;

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        var messageId = Guid.NewGuid();
        var message = new CommunicationMessage
        {
            Id = messageId,
            Segment = "B1J - Missing Consent",
            Channels = CommunicationChannel.Email,
            Language = "en",
            Subject = "Parental Consent Required",
            BodyTemplate = "Hello, we need parental consent...",
            SentAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        _context.CommunicationMessages.Add(message);
        await _context.SaveChangesAsync();

        var recipient = new CommunicationRecipient
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            RecipientType = RecipientType.Volunteer,
            VolunteerId = volunteerId,
            RecipientEmail = volunteer.Email,
            Channel = CommunicationChannel.Email,
            DeliveryStatus = DeliveryStatus.Queued
        };

        // Act
        _context.CommunicationRecipients.Add(recipient);
        await _context.SaveChangesAsync();

        // Assert
        var savedRecipient = await _context.CommunicationRecipients.FindAsync(recipient.Id);
        Assert.NotNull(savedRecipient);
        Assert.Equal(DeliveryStatus.Queued, savedRecipient.DeliveryStatus);
        Assert.Equal(volunteerId, savedRecipient.VolunteerId);
    }

    [Fact]
    public async Task CommunicationRecipient_ShouldTransitionFromQueuedToSent()
    {
        // Arrange
        var recipientId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var volunteerId = Guid.NewGuid();

        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "test@example.com");
        _context.Volunteers.Add(volunteer);

        var message = new CommunicationMessage
        {
            Id = messageId,
            Segment = "B1J - In Onboarding",
            Channels = CommunicationChannel.Email,
            Language = "fr",
            Subject = "Mise à jour de l'embarquement",
            BodyTemplate = "Bonjour...",
            SentAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        _context.CommunicationMessages.Add(message);

        var recipient = new CommunicationRecipient
        {
            Id = recipientId,
            MessageId = messageId,
            RecipientType = RecipientType.Volunteer,
            VolunteerId = volunteerId,
            RecipientEmail = volunteer.Email,
            Channel = CommunicationChannel.Email,
            DeliveryStatus = DeliveryStatus.Queued
        };

        _context.CommunicationRecipients.Add(recipient);
        await _context.SaveChangesAsync();

        // Act - Simulate delivery
        var dbRecipient = await _context.CommunicationRecipients.FindAsync(recipientId);
        dbRecipient!.DeliveryStatus = DeliveryStatus.Sent;
        dbRecipient.DeliveredAt = DateTime.UtcNow;
        _context.CommunicationRecipients.Update(dbRecipient);
        await _context.SaveChangesAsync();

        // Assert
        var sentRecipient = await _context.CommunicationRecipients.FindAsync(recipientId);
        Assert.Equal(DeliveryStatus.Sent, sentRecipient?.DeliveryStatus);
        Assert.NotNull(sentRecipient?.DeliveredAt);
    }

    [Fact]
    public async Task CommunicationRecipient_ShouldTrackFailuresAndRetries()
    {
        // Arrange
        var recipientId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var volunteerId = Guid.NewGuid();

        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "retry@example.com");
        volunteer.SmsOptIn = true;

        _context.Volunteers.Add(volunteer);

        var message = new CommunicationMessage
        {
            Id = messageId,
            Segment = "B1J - Assigned",
            Channels = CommunicationChannel.SMS,
            Language = "en",
            Subject = string.Empty,
            BodyTemplate = "You have been assigned to a mission...",
            SentAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        _context.CommunicationMessages.Add(message);

        var recipient = new CommunicationRecipient
        {
            Id = recipientId,
            MessageId = messageId,
            RecipientType = RecipientType.Volunteer,
            VolunteerId = volunteerId,
            RecipientPhone = volunteer.Phone,
            Channel = CommunicationChannel.SMS,
            DeliveryStatus = DeliveryStatus.Queued,
            RetriedCount = 0
        };

        _context.CommunicationRecipients.Add(recipient);
        await _context.SaveChangesAsync();

        // Act - Simulate failure and retry
        var dbRecipient = await _context.CommunicationRecipients.FindAsync(recipientId);
        dbRecipient!.DeliveryStatus = DeliveryStatus.Failed;
        dbRecipient.LastError = "SMS gateway timeout";
        dbRecipient.RetriedCount = 1;
        _context.CommunicationRecipients.Update(dbRecipient);
        await _context.SaveChangesAsync();

        // Assert
        var failedRecipient = await _context.CommunicationRecipients.FindAsync(recipientId);
        Assert.Equal(DeliveryStatus.Failed, failedRecipient?.DeliveryStatus);
        Assert.Equal(1, failedRecipient?.RetriedCount);
        Assert.NotNull(failedRecipient?.LastError);
    }

    [Fact]
    public async Task CommunicationMessage_ShouldSupportMultipleRecipients()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var message = new CommunicationMessage
        {
            Id = messageId,
            Segment = "B1J - Missing Consent",
            Channels = CommunicationChannel.Email,
            Language = "fr",
            Subject = "Action requise",
            BodyTemplate = "Bonjour...",
            SentAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        _context.CommunicationMessages.Add(message);
        await _context.SaveChangesAsync();

        var recipients = new[]
        {
            new CommunicationRecipient
            {
                Id = Guid.NewGuid(),
                MessageId = messageId,
                RecipientType = RecipientType.Volunteer,
                VolunteerId = Guid.NewGuid(),
                RecipientEmail = "volunteer1@example.com",
                Channel = CommunicationChannel.Email,
                DeliveryStatus = DeliveryStatus.Sent,
                DeliveredAt = DateTime.UtcNow
            },
            new CommunicationRecipient
            {
                Id = Guid.NewGuid(),
                MessageId = messageId,
                RecipientType = RecipientType.Guardian,
                VolunteerId = Guid.NewGuid(),
                RecipientEmail = "guardian@example.com",
                Channel = CommunicationChannel.Email,
                DeliveryStatus = DeliveryStatus.Queued
            },
            new CommunicationRecipient
            {
                Id = Guid.NewGuid(),
                MessageId = messageId,
                RecipientType = RecipientType.Volunteer,
                VolunteerId = Guid.NewGuid(),
                RecipientEmail = "volunteer2@example.com",
                Channel = CommunicationChannel.Email,
                DeliveryStatus = DeliveryStatus.Failed,
                LastError = "Invalid email"
            }
        };

        // Act
        _context.CommunicationRecipients.AddRange(recipients);
        await _context.SaveChangesAsync();

        // Assert
        var messageRecipients = await _context.CommunicationRecipients
            .Where(r => r.MessageId == messageId)
            .ToListAsync();

        Assert.Equal(3, messageRecipients.Count);
        Assert.Single(messageRecipients, r => r.DeliveryStatus == DeliveryStatus.Sent);
        Assert.Single(messageRecipients, r => r.DeliveryStatus == DeliveryStatus.Queued);
        Assert.Single(messageRecipients, r => r.DeliveryStatus == DeliveryStatus.Failed);
    }

    [Fact]
    public async Task CommunicationMessage_ShouldSupportBothEmailAndSmsChannels()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var volunteerId = Guid.NewGuid();

        var volunteer = TestDataFactory.CreateVolunteer(volunteerId, isMinor: true, email: "multi@example.com");
        volunteer.Status = VolunteerStatus.Active;
        volunteer.SmsOptIn = true;

        _context.Volunteers.Add(volunteer);

        var message = new CommunicationMessage
        {
            Id = messageId,
            Segment = "B1J - Urgent Alert",
            Channels = CommunicationChannel.Email | CommunicationChannel.SMS,
            Language = "en",
            Subject = "Urgent: Action Required",
            BodyTemplate = "Urgent message...",
            SentAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };

        _context.CommunicationMessages.Add(message);
        await _context.SaveChangesAsync();

        // Act - Create both email and SMS recipients
        var emailRecipient = new CommunicationRecipient
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            RecipientType = RecipientType.Volunteer,
            VolunteerId = volunteerId,
            RecipientEmail = volunteer.Email,
            Channel = CommunicationChannel.Email,
            DeliveryStatus = DeliveryStatus.Sent,
            DeliveredAt = DateTime.UtcNow
        };

        var smsRecipient = new CommunicationRecipient
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            RecipientType = RecipientType.Volunteer,
            VolunteerId = volunteerId,
            RecipientPhone = volunteer.Phone,
            Channel = CommunicationChannel.SMS,
            DeliveryStatus = DeliveryStatus.Sent,
            DeliveredAt = DateTime.UtcNow
        };

        _context.CommunicationRecipients.AddRange(new[] { emailRecipient, smsRecipient });
        await _context.SaveChangesAsync();

        // Assert
        var recipients = await _context.CommunicationRecipients
            .Where(r => r.MessageId == messageId && r.VolunteerId == volunteerId)
            .ToListAsync();

        Assert.Equal(2, recipients.Count);
        Assert.Contains(recipients, r => r.Channel == CommunicationChannel.Email);
        Assert.Contains(recipients, r => r.Channel == CommunicationChannel.SMS);
        Assert.All(recipients, r => Assert.Equal(DeliveryStatus.Sent, r.DeliveryStatus));
    }
}
