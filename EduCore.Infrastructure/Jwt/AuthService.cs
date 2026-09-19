using EduCore.Application.Common.Exceptions;
using EduCore.Application.Features.Authentication.DTOs.Login;
using EduCore.Application.Features.Authentication.Interfaces;
using EduCore.Domain.Identity;
using EduCore.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using EduCore.Application.Common;

namespace EduCore.Infrastructure.Jwt;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IUnitOfWork UnitOfWork)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(request.UserName.Trim());

        if (user is null)
            throw new NotFoundException(ErrorCodes.User.NotFound);

        if (!user.IsActive)
            throw new ForbiddenException(ErrorCodes.User.NotActive);

        if (await _userManager.IsLockedOutAsync(user))
            throw new ForbiddenException(ErrorCodes.User.Forbidden);

        var validPassword =
            await _userManager.CheckPasswordAsync(user, request.Password);

        if (!validPassword)
        {
            await _userManager.AccessFailedAsync(user);

            throw new UnauthorizedException(ErrorCodes.User.InvalidUserNamePassword);
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var roles = await _userManager.GetRolesAsync(user);

        var token = _jwtTokenService.GenerateToken(user, roles);

        return new LoginResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Token = token.Token,
            Expiration = token.Expiration,
            Roles = roles.ToList()
        };
    }

}