using EduCore.Domain.Entities.Enums;

namespace EduCore.Application.Features.Payments.DTOs;

public class CreatePaymentRequest
{
    public int StudentFeeId { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public string? Notes { get; set; }
}