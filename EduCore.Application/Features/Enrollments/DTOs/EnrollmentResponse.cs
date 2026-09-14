namespace EduCore.Application.Features.Enrollments.DTOs;

public class EnrollmentResponse
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public int GroupId { get; set; }

    public string GroupName { get; set; } = string.Empty;

    public int TeacherId { get; set; }

    public string TeacherName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}