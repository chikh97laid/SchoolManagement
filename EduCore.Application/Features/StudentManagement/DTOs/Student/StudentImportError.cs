namespace EduCore.Application.Features.StudentManagement.DTOs;

public class StudentImportError
{
    public int RowNumber { get; set; }

    public string Error { get; set; } = string.Empty;
}