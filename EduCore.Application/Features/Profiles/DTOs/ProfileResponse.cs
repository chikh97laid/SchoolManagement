namespace EduCore.Application.Features.Profiles.DTOs;

public class ProfileResponse
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public bool MustChangePassword { get; set; }

    public DateTime CreatedAt { get; set; }

    public IList<string> Roles { get; set; } = [];
}