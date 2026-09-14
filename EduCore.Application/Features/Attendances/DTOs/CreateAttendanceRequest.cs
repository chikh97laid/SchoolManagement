using EduCore.Domain.Entities.Enums;

namespace EduCore.Application.Features.Attendances.DTOs;
public class CreateAttendanceRequest
{
    public int EnrollmentId { get; set; }

    public DateOnly Date { get; set; }

    public AttendanceStatus Status { get; set; }

    public string? Notes { get; set; }
}