using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedCrossManager.Server.DTOs.Onboarding;
using RedCrossManager.Server.Services.Onboarding;
using System.Security.Claims;

namespace RedCrossManager.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
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
