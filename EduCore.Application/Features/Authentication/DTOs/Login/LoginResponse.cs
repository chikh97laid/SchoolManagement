namespace EduCore.Application.Features.Authentication.DTOs.Login;

public class LoginResponse
{
    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public DateTime Expiration { get; set; }

    public List<string> Roles { get; set; } = [];
}