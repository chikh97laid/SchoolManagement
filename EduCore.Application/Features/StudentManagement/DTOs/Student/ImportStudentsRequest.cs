using EduCore.Domain.Entities.Enums;


namespace EduCore.Application.Features.StudentManagement.DTOs.Student;

public class ImportStudentsRequest
{
    public int RowNumber { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? GuardianName { get; set; }

    public string? GuardianPhone { get; set; }

    public DateOnly? BirthDate { get; set; }

    public Gender Gender { get; set; }
}