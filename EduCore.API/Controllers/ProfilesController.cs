using EduCore.Application.Features.Profiles.DTOs;
using EduCore.Application.Features.Profiles.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<ActionResult<ProfileResponse>> Get(
        CancellationToken cancellationToken)
    {
        var profile = await _profileService.GetMyProfileAsync(
            cancellationToken);

        return Ok(profile);
    }

    [HttpPut]
    public async Task<ActionResult<ProfileResponse>> Update(
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var profile = await _profileService.UpdateMyProfileAsync(
            request,
            cancellationToken);

        return Ok(profile);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _profileService.ChangePasswordAsync(
            request,
            cancellationToken);

        return NoContent();
    }
}