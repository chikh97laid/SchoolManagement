using EduCore.Domain.Entities.Enums;

namespace EduCore.Application.Features.Attendances.DTOs;

public class UpdateAttendanceRequest
{
    public DateOnly Date { get; set; }

    public AttendanceStatus Status { get; set; }

    public string? Notes { get; set; }
}