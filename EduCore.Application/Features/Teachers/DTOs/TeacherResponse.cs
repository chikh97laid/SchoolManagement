namespace EduCore.Application.Features.Teachers.DTOs;

public class TeacherResponse
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Specialization { get; set; }

    public DateTime HireDate { get; set; }

    public bool IsActive { get; set; }

    public Guid? UserId { get; set; }

    public string? UserName { get; set; }
}