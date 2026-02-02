using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedCrossManager.Server.DTOs.Onboarding;
using RedCrossManager.Server.Services.Onboarding;
using System.Security.Claims;

namespace RedCrossManager.Server.Controllers;

[ApiController]
[Route("api/v1/onboarding")]
public class OnboardingController : ControllerBase
{
    private readonly IOnboardingService _onboardingService;
    private readonly ILogger<OnboardingController> _logger;

    public OnboardingController(
        IOnboardingService onboardingService,
        ILogger<OnboardingController> logger)
    {
        _onboardingService = onboardingService;
        _logger = logger;
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(OnboardingProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OnboardingProgressDto>> GetMyProgress(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var progress = await _onboardingService.GetProgressByUserIdAsync(userId, cancellationToken);
        if (progress == null)
        {
            _logger.LogWarning("No volunteer profile found for user {UserId}. User should register through the volunteer registration flow.", userId);
            return NotFound(new { 
                error = "No volunteer profile found for this user",
                message = "Please complete the volunteer registration process first.",
                redirectTo = "/onboarding/registration"
            });
        }

        return Ok(progress);
    }

    [HttpGet("progress/{volunteerId:guid}")]
    [ProducesResponseType(typeof(OnboardingProgressDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<OnboardingProgressDto>> GetProgress(
        Guid volunteerId,
        CancellationToken cancellationToken)
    {
        var progress = await _onboardingService.GetProgressAsync(volunteerId, cancellationToken);
        return Ok(progress);
    }

    [HttpPost("{volunteerId:guid}/steps/submit")]
    [ProducesResponseType(typeof(OnboardingStepDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OnboardingStepDto>> SubmitStep(
        Guid volunteerId,
        [FromBody] SubmitStepDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _onboardingService.SubmitStepAsync(volunteerId, dto, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("me/steps/submit")]
    [Authorize]
    [ProducesResponseType(typeof(OnboardingStepDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OnboardingStepDto>> SubmitMyStep(
        [FromBody] SubmitStepDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        // Get the volunteer ID from the user ID
        var progress = await _onboardingService.GetProgressByUserIdAsync(userId, cancellationToken);
        if (progress == null)
        {
            return NotFound(new { error = "No volunteer profile found for this user" });
        }

        try
        {
            var result = await _onboardingService.SubmitStepAsync(progress.VolunteerId, dto, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("steps/pending")]
    [Authorize(Policy = "Coordinator")]
    [ProducesResponseType(typeof(List<AdminOnboardingStepDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AdminOnboardingStepDto>>> GetPendingSteps(CancellationToken cancellationToken)
    {
        var steps = await _onboardingService.GetPendingStepsAsync(cancellationToken);
        return Ok(steps);
    }

    [HttpPost("steps/{stepId:guid}/review")]
    [Authorize(Policy = "Coordinator")]
    [ProducesResponseType(typeof(OnboardingStepDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OnboardingStepDto>> ReviewStep(
        Guid stepId,
        [FromBody] ReviewStepDto dto,
        CancellationToken cancellationToken)
    {
        var reviewerId = GetCurrentUserId();
        try
        {
            var result = await _onboardingService.ReviewStepAsync(stepId, reviewerId, dto, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
