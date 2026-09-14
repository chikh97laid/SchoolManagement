namespace EduCore.Application.Features.Authentication.DTOs;

public class JwtTokenResult
{
    public required string Token { get; init; }

    public DateTime Expiration { get; init; }
}