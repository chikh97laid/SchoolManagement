using EduCore.Application.Features.Authentication.DTOs.Login;
using EduCore.Application.Features.Authentication.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _authService.LoginAsync(request);

        return Ok(result);
    }
}