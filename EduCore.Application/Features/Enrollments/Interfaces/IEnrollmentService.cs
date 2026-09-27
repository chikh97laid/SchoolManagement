using EduCore.Application.Features.Enrollments.DTOs;
using EduCore.Application.Common.Pagination;

namespace EduCore.Application.Features.Enrollments.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentResponse> CreateAsync(
        CreateEnrollmentRequest request,
        CancellationToken cancellationToken = default);

    Task<EnrollmentResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PagedResult<EnrollmentResponse>> GetAllAsync(
        PaginationParameters parameters,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task ActivateAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}