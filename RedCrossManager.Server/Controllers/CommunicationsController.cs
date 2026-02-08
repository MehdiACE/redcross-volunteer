using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedCrossManager.Server.Domain.Entities;
using RedCrossManager.Server.DTOs.Communications;
using RedCrossManager.Server.Infrastructure;
using RedCrossManager.Server.Services.Communications;
using System.Security.Claims;

namespace RedCrossManager.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(LoggingActionFilter))]
public class CommunicationsController : ControllerBase
{
    private readonly ICommunicationService _communicationService;
    private readonly ILogger<CommunicationsController> _logger;

    public CommunicationsController(
        ICommunicationService communicationService,
        ILogger<CommunicationsController> logger)
    {
        _communicationService = communicationService;
        _logger = logger;
    }

    /// <summary>
    /// Send a new communication message to volunteers/guardians.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<ActionResult<CommunicationMessageDto>> SendCommunication([FromBody] SendCommunicationRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

            var message = await _communicationService.SendCommunicationAsync(
                request.Segment,
                request.Channels,
                request.Language,
                request.Subject ?? string.Empty,
                request.BodyTemplate,
                request.RecipientVolunteerIds,
                userId);

            var dto = MapToDto(message);

            _logger.LogInformation("Communication {MessageId} sent by user {UserId} to segment '{Segment}'",
                message.Id, userId, request.Segment);

            return CreatedAtAction(nameof(GetCommunication), new { id = message.Id }, dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send communication");
            return StatusCode(500, new { error = "Failed to send communication" });
        }
    }

    /// <summary>
    /// Get details of a specific communication message.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<ActionResult<CommunicationMessageDto>> GetCommunication(Guid id)
    {
        try
        {
            var (message, stats) = await _communicationService.GetCommunicationStatusAsync(id);
            var dto = MapToDto(message, stats);

            return Ok(dto);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get communication {MessageId}", id);
            return StatusCode(500, new { error = "Failed to retrieve communication" });
        }
    }

    /// <summary>
    /// Get recent communications (admin view).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Coordinator")]
    public async Task<ActionResult<List<CommunicationMessageDto>>> GetRecentCommunications([FromQuery] int count = 50)
    {
        try
        {
            var messages = await _communicationService.GetRecentCommunicationsAsync(count);
            var dtos = messages.Select(m => MapToDto(m)).ToList();

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recent communications");
            return StatusCode(500, new { error = "Failed to retrieve communications" });
        }
    }

    /// <summary>
    /// Get communication history for a specific volunteer.
    /// </summary>
    [HttpGet("volunteer/{volunteerId}")]
    [Authorize]
    public async Task<ActionResult<List<CommunicationRecipientDto>>> GetVolunteerHistory(Guid volunteerId)
    {
        try
        {
            // TODO: Add authorization check - volunteers can only see their own history
            var recipients = await _communicationService.GetVolunteerCommunicationHistoryAsync(volunteerId);
            var dtos = recipients.Select(MapRecipientToDto).ToList();

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get communication history for volunteer {VolunteerId}", volunteerId);
            return StatusCode(500, new { error = "Failed to retrieve communication history" });
        }
    }

    /// <summary>
    /// Manually trigger processing of queued communications (for testing/admin).
    /// </summary>
    [HttpPost("process-queue")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> ProcessQueue([FromQuery] int maxRecipients = 100)
    {
        try
        {
            var successCount = await _communicationService.ProcessQueuedCommunicationsAsync(maxRecipients);

            return Ok(new { processed = maxRecipients, succeeded = successCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process communication queue");
            return StatusCode(500, new { error = "Failed to process queue" });
        }
    }

    private CommunicationMessageDto MapToDto(CommunicationMessage message, Dictionary<DeliveryStatus, int>? stats = null)
    {
        stats ??= message.Recipients
            .GroupBy(r => r.DeliveryStatus)
            .ToDictionary(g => g.Key, g => g.Count());

        return new CommunicationMessageDto
        {
            Id = message.Id,
            Segment = message.Segment,
            Channels = message.Channels,
            Language = message.Language,
            Subject = message.Subject,
            BodyTemplate = message.BodyTemplate,
            SentAt = message.SentAt,
            CreatedBy = message.CreatedBy,
            TotalRecipients = message.Recipients.Count,
            QueuedCount = stats.GetValueOrDefault(DeliveryStatus.Queued, 0),
            SentCount = stats.GetValueOrDefault(DeliveryStatus.Sent, 0),
            FailedCount = stats.GetValueOrDefault(DeliveryStatus.Failed, 0),
            BouncedCount = stats.GetValueOrDefault(DeliveryStatus.Bounced, 0)
        };
    }

    private CommunicationRecipientDto MapRecipientToDto(CommunicationRecipient recipient)
    {
        return new CommunicationRecipientDto
        {
            Id = recipient.Id,
            MessageId = recipient.MessageId,
            RecipientType = recipient.RecipientType,
            VolunteerId = recipient.VolunteerId,
            VolunteerName = recipient.Volunteer != null 
                ? $"{recipient.Volunteer.FirstName} {recipient.Volunteer.LastName}" 
                : null,
            RecipientEmail = recipient.RecipientEmail,
            RecipientPhone = recipient.RecipientPhone,
            Channel = recipient.Channel,
            DeliveryStatus = recipient.DeliveryStatus,
            DeliveredAt = recipient.DeliveredAt,
            RetriedCount = recipient.RetriedCount,
            LastError = recipient.LastError,
            MessageSubject = recipient.Message?.Subject ?? string.Empty,
            MessageSentAt = recipient.Message?.SentAt ?? DateTime.MinValue
        };
    }
}
