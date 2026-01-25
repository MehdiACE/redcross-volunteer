using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedCrossManager.Server.DTOs.Consents;
using RedCrossManager.Server.Services.Consents;
using System.Security.Claims;

namespace RedCrossManager.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ConsentsController : ControllerBase
{
    private readonly IConsentService _consentService;
    private readonly ILogger<ConsentsController> _logger;

    public ConsentsController(
        IConsentService consentService,
        ILogger<ConsentsController> logger)
    {
        _consentService = consentService;
        _logger = logger;
    }

    [HttpPost("{volunteerId:guid}/request")]
    [ProducesResponseType(typeof(ParentalConsentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ParentalConsentDto>> RequestConsent(
        Guid volunteerId,
        [FromBody] RequestConsentDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _consentService.RequestConsentAsync(volunteerId, dto, cancellationToken);
            return CreatedAtAction(nameof(GetByVolunteerId), new { volunteerId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("{volunteerId:guid}")]
    [ProducesResponseType(typeof(ParentalConsentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ParentalConsentDto>> SubmitConsent(
        Guid volunteerId,
        [FromBody] SubmitConsentDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _consentService.SubmitConsentAsync(volunteerId, dto, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{consentId:guid}/review")]
    [Authorize(Policy = "Coordinator")]
    [ProducesResponseType(typeof(ParentalConsentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ParentalConsentDto>> ReviewConsent(
        Guid consentId,
        [FromBody] ReviewConsentDto dto,
        CancellationToken cancellationToken)
    {
        var reviewerId = GetCurrentUserId();
        try
        {
            var result = await _consentService.ReviewConsentAsync(consentId, reviewerId, dto, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{volunteerId:guid}")]
    [ProducesResponseType(typeof(ParentalConsentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParentalConsentDto>> GetByVolunteerId(
        Guid volunteerId,
        CancellationToken cancellationToken)
    {
        var consent = await _consentService.GetByVolunteerIdAsync(volunteerId, cancellationToken);
        return consent == null ? NotFound() : Ok(consent);
    }

    [HttpGet("pending")]
    [Authorize(Policy = "Coordinator")]
    [ProducesResponseType(typeof(List<ParentalConsentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ParentalConsentDto>>> GetPendingReview(
        CancellationToken cancellationToken)
    {
        var consents = await _consentService.GetPendingReviewAsync(cancellationToken);
        return Ok(consents);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
