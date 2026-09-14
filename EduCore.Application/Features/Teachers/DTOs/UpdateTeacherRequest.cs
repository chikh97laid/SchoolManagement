namespace EduCore.Application.Features.Teachers.DTOs;

public class UpdateTeacherRequest
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Specialization { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime HireDate { get; set; }
}