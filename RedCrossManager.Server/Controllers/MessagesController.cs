using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RedCrossManager.Server.DTOs.Messages;
using RedCrossManager.Server.Services.Messages;

namespace RedCrossManager.Server.Controllers;

[ApiController]
[Route("api/v1/messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpGet("inbox")]
    public async Task<ActionResult<List<MessageDto>>> GetInbox(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var messages = await _messageService.GetInboxAsync(userGuid, cancellationToken);
        return Ok(messages);
    }

    [HttpGet("sent")]
    public async Task<ActionResult<List<MessageDto>>> GetSent(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var messages = await _messageService.GetSentAsync(userGuid, cancellationToken);
        return Ok(messages);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var count = await _messageService.GetUnreadCountAsync(userGuid, cancellationToken);
        return Ok(count);
    }

    [HttpGet("conversation/{otherUserId}")]
    public async Task<ActionResult<List<MessageDto>>> GetConversation(Guid otherUserId, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        var messages = await _messageService.GetConversationAsync(userGuid, otherUserId, cancellationToken);
        return Ok(messages);
    }

    [HttpPost("send")]
    public async Task<ActionResult<MessageDto>> SendMessage([FromBody] CreateMessageDto dto, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest("Message content cannot be empty");

        var message = await _messageService.SendMessageAsync(userGuid, dto, cancellationToken);
        return CreatedAtAction(nameof(GetConversation), message);
    }

    [HttpPost("send-to-volunteer")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<MessageDto>> SendToVolunteer([FromBody] SendToVolunteerDto dto, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest("Message content cannot be empty");

        var message = await _messageService.SendToVolunteerAsync(userGuid, dto.VolunteerId, dto.Content, cancellationToken);
        return CreatedAtAction(nameof(GetInbox), message);
    }

    [HttpPost("{messageId}/read")]
    public async Task<IActionResult> MarkAsRead(Guid messageId, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        await _messageService.MarkAsReadAsync(messageId, cancellationToken);
        return NoContent();
    }
}
