using EduCore.Domain.Entities.Enums;

public class UpdatePaymentRequest
{
    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public string? Notes { get; set; }
}