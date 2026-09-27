using EduCore.Application.Common.Pagination;
using EduCore.Application.Features.Teachers.DTOs;



namespace EduCore.Application.Features.Teachers.Interfaces;

public interface ITeacherService
{
    Task<CreateTeacherResponse> CreateAsync(
        CreateTeacherRequest request,
        CancellationToken cancellationToken = default);

    Task<TeacherResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PagedResult<TeacherResponse>> GetAllAsync(
        PaginationParameters parameters,
        CancellationToken cancellationToken = default);

    Task<TeacherResponse> UpdateAsync(
        int id,
        UpdateTeacherRequest request,
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