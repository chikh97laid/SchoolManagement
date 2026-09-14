using EduCore.Domain.Entities.Enums;

namespace EduCore.Application.Features.Payments.DTOs;

public class PaymentResponse
{
    public int Id { get; set; }

    public int StudentFeeId { get; set; }

    public decimal Amount { get; set; }

    public DateOnly PaymentDate { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus Status { get; set; }

    public string? Notes { get; set; }
}