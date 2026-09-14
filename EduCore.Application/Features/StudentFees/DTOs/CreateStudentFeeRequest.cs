namespace EduCore.Application.Features.StudentFees.DTOs;

public class CreateStudentFeeRequest
{
    public int EnrollmentId { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateOnly? DueDate { get; set; }

    public string? Description { get; set; }
}