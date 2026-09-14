namespace EduCore.Application.Features.StudentManagement.DTOs.Student;

public class CreateStudentResponse
{
    public int StudentId { get; set; }

    public Guid UserId { get; set; }

    public string StudentCode { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;
}