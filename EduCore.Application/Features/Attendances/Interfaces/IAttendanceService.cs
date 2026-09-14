using EduCore.Application.Features.Attendances.DTOs;

namespace EduCore.Application.Features.Attendances.Interfaces;
public interface IAttendanceService
{
    Task<AttendanceResponse> CreateAsync(
        CreateAttendanceRequest request,
        CancellationToken cancellationToken = default);

    Task<AttendanceResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<AttendanceResponse>> GetAllAsync(CancellationToken cancellationToken = default);

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
}