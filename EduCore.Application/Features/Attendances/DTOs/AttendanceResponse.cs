using EduCore.Domain.Entities.Enums;

namespace EduCore.Application.Features.Attendances.DTOs;
public class AttendanceResponse
{
    public int Id { get; set; }

    public int EnrollmentId { get; set; }

    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;

    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;

    public DateOnly Date { get; set; }

    public AttendanceStatus Status { get; set; }

    public string? Notes { get; set; }
}