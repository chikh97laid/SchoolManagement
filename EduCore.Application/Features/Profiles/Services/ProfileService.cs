using EduCore.Application.Common.Exceptions;
using EduCore.Application.Common.Interfaces;
using EduCore.Application.Features.Profiles.DTOs;
using EduCore.Application.Features.Profiles.Interfaces;
using EduCore.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using EduCore.Application.Common;

namespace EduCore.Application.Features.Profiles.Services;

public class ProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;

    public ProfileService(
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
    }

    public async Task<ProfileResponse> GetMyProfileAsync(
        CancellationToken cancellationToken = default)
    {
        var user = await GetCurrentUserAsync();

        return await BuildProfileResponseAsync(user);
    }

    public async Task<ProfileResponse> UpdateMyProfileAsync(
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await GetCurrentUserAsync();

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.PhoneNumber = request.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            // Get the first error code from the result and throw a BadRequestException
            var firstErrorCode = result.Errors.First().Code;
            throw new BadRequestException($"IDENTITY_{firstErrorCode.ToUpper()}");
        }

        return await BuildProfileResponseAsync(user);
    }

    public async Task ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await GetCurrentUserAsync();

        var result = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
        {
            var firstErrorCode = result.Errors.First().Code;
            throw new BadRequestException($"IDENTITY_{firstErrorCode.ToUpper()}");
        }
    }

    private async Task<ApplicationUser> GetCurrentUserAsync()
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException(ErrorCodes.User.MustBeLoggedIn);

        var userId = _currentUserService.UserId;

        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null)
            throw new NotFoundException(ErrorCodes.User.NotFound);

        if (!user.IsActive)
            throw new ForbiddenException(ErrorCodes.User.NotActive);

        return user;
    }

    private async Task<ProfileResponse> BuildProfileResponseAsync(
    ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new ProfileResponse
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Roles = roles
        };
    }
}