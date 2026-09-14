using EduCore.Domain.Entities.Enums;

namespace EduCore.Application.Features.StudentFees.DTOs;

public class StudentFeeResponse
{
    public int Id { get; set; }

    public int EnrollmentId { get; set; }

    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public int GroupId { get; set; }

    public string GroupName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal RemainingAmount => TotalAmount - PaidAmount;

    public DateOnly? DueDate { get; set; }

    public StudentFeeStatus Status { get; set; }

    public string? Description { get; set; }
}