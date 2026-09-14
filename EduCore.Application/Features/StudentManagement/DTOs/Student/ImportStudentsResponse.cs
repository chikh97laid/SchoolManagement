namespace EduCore.Application.Features.StudentManagement.DTOs;

public class ImportStudentsResponse
{
    public int TotalRows { get; set; }

    public int SuccessfulRows { get; set; }

    public int FailedRows { get; set; }

    public List<StudentImportError> Errors { get; set; } = [];
}