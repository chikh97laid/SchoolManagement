namespace EduCore.Application.Features.StudentManagement.DTOs.Student;

public class StudentImportSettings
{
    public const string SectionName = "StudentImport";
    public string DefaultPassword { get; set; } = string.Empty;
}