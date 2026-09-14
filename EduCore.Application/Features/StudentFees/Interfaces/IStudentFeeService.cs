using EduCore.Application.Features.StudentFees.DTOs;

namespace EduCore.Application.Features.StudentFees.Interfaces;

public interface IStudentFeeService
{
    Task<StudentFeeResponse> CreateAsync(
        CreateStudentFeeRequest request,
        CancellationToken cancellationToken = default);

    Task<StudentFeeResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<StudentFeeResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IEnumerable<StudentFeeResponse>> GetByStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<StudentFeeResponse>> GetByEnrollmentAsync(
        int enrollmentId,
        CancellationToken cancellationToken = default);

    Task<StudentFeeResponse> UpdateAsync(
        int id,
        UpdateStudentFeeRequest request,
        CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}