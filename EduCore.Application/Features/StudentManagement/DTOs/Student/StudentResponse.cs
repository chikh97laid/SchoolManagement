
using EduCore.Domain.Entities.Enums;

namespace EduCore.Application.Features.StudentManagement.DTOs.Student;

public class StudentResponse
{
    public int Id { get; set; }

    public string StudentCode { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? GuardianName { get; set; }

    public string? GuardianPhone { get; set; }

    public DateOnly? BirthDate { get; set; }

    public Gender Gender { get; set; }

    public bool IsActive { get; set; }

    public Guid? UserId { get; set; }

    public string? UserName { get; set; }
}