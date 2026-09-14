namespace EduCore.Infrastructure.Identity;

public class SeedAdminSettings
{
    public const string SectionName = "SeedAdmin";

    public required string UserName { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }
}
