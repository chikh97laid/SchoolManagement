using EduCore.Application.Features.Attendances.DTOs;
using EduCore.Application.Common.Pagination;

namespace EduCore.Application.Features.Attendances.Interfaces;
public interface IAttendanceService
{
    Task<AttendanceResponse> CreateAsync(
        CreateAttendanceRequest request,
        CancellationToken cancellationToken = default);

    Task<AttendanceResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PagedResult<AttendanceResponse>> GetAllAsync(
        PaginationParameters parameters,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<AttendanceResponse>> GetByEnrollmentAsync(
        int enrollmentId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<AttendanceResponse>> GetByStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default);

    Task<AttendanceResponse> UpdateAsync(
        int id,
        UpdateAttendanceRequest request,
        CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}