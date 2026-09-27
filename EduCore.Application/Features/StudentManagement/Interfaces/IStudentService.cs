using EduCore.Application.Features.StudentManagement.DTOs;
using EduCore.Application.Features.StudentManagement.DTOs.Student;
using EduCore.Application.Common.Pagination;

namespace EduCore.Application.Features.StudentManagement.Interfaces;

public interface IStudentService
{
    Task<CreateStudentResponse> CreateStudentAsync(
        CreateStudentRequest request,
        CancellationToken cancellationToken = default);

    Task<StudentResponse> GetStudentByIdAsync(
        int studentId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<StudentResponse>> GetStudentsAsync(
        PaginationParameters parameters,
        CancellationToken cancellationToken = default);

    Task<StudentResponse> UpdateStudentAsync(
        int studentId,
        UpdateStudentRequest request,
        CancellationToken cancellationToken = default);

    Task DeactivateStudentAccountAsync(
        int studentId,
        CancellationToken cancellationToken = default);

    Task SoftDeleteStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default);

    Task<ImportStudentsResponse> ImportStudentsFromExcelAsync(
    Stream fileStream,
    CancellationToken cancellationToken = default);

}