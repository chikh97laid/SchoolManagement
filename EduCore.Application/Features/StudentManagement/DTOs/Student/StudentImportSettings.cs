namespace EduCore.Application.Features.StudentManagement.DTOs.Student;

public class StudentImportSettings
{
    public const string SectionName = "StudentImport";
    public const long MaxFileSizeBytes = 10 * 1024 * 1024;
    public const long MaxRequestSizeBytes = MaxFileSizeBytes + 64 * 1024;
    public const int MaxRows = 1000;

    public string DefaultPassword { get; set; } = string.Empty;
}