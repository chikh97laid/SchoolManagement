using EduCore.Application.Features.Authentication.DTOs;
using EduCore.Domain.Identity;

namespace EduCore.Application.Features.Authentication.Interfaces;

public interface IJwtTokenService
{
    JwtTokenResult GenerateToken(ApplicationUser user, IList<string> roles);
}