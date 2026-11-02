using Microsoft.AspNetCore.Identity;

namespace EduCore.Domain.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}