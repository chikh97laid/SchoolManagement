using System.Security.Claims;
using System.Text;
using EduCore.Application.Features.Authentication.DTOs;
using EduCore.Application.Features.Authentication.Interfaces;
using EduCore.Domain.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace EduCore.Infrastructure.Jwt;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;    
    }

    public JwtTokenResult GenerateToken(ApplicationUser user, IList<string> roles)
    {

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            new(ClaimTypes.NameIdentifier, user.Id.ToString()),

            new(ClaimTypes.GivenName, user.FirstName), 
            new(ClaimTypes.Surname, user.LastName),    
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),

        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.Key));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var expiration = DateTime.UtcNow.AddMinutes(_settings.ExpiryInMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            SigningCredentials = credentials
        };

        var handler = new JsonWebTokenHandler();

        var token = handler.CreateToken(descriptor);

        return new JwtTokenResult
        {
            Token = token,
            Expiration = expiration
        };
    }
}