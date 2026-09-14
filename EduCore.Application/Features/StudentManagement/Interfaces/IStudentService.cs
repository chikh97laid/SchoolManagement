using EduCore.Application.Features.StudentManagement.DTOs;
using EduCore.Application.Features.StudentManagement.DTOs.Student;

namespace EduCore.Application.Features.StudentManagement.Interfaces;

public interface IStudentService
{
    Task<CreateStudentResponse> CreateStudentAsync(
        CreateStudentRequest request,
        CancellationToken cancellationToken = default);

    Task<StudentResponse> GetStudentByIdAsync(
        int studentId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<StudentResponse>> GetStudentsAsync(
        CancellationToken cancellationToken = default);

    Task<StudentResponse> UpdateStudentAsync(
        int studentId,
        UpdateStudentRequest request,
        CancellationToken cancellationToken = default);

    Task DeactivateStudentAccountAsync(
        int studentId,
        CancellationToken cancellationToken = default);

    Task<ImportStudentsResponse> ImportStudentsFromExcelAsync(
    Stream fileStream,
    CancellationToken cancellationToken = default);

}