using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedCrossManager.Server.DTOs.Auth;
using RedCrossManager.Server.Services.Auth;

namespace RedCrossManager.Server.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password, cancellationToken);
        if (result is null)
        {
            return Unauthorized();
        }

        return Ok(result);
    }
}
