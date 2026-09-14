using EduCore.Application.Features.Enrollments.DTOs;

namespace EduCore.Application.Features.Enrollments.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentResponse> CreateAsync(
        CreateEnrollmentRequest request,
        CancellationToken cancellationToken = default);

    Task<EnrollmentResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<EnrollmentResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task ActivateAsync(
        int id,
        CancellationToken cancellationToken = default);
}