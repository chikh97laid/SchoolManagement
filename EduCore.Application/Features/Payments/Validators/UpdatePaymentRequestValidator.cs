using EduCore.Application.Features.Payments.DTOs;
using FluentValidation;
using EduCore.Application.Common;

namespace EduCore.Application.Features.Payments.Validators;

public class UpdatePaymentRequestValidator
    : AbstractValidator<UpdatePaymentRequest>
{
    public UpdatePaymentRequestValidator()
    {

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithErrorCode(ErrorCodes.Payment.AmountInvalid);

        RuleFor(x => x.PaymentMethod)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.Payment.PaymentMetodeRequired);

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithErrorCode(ErrorCodes.Payment.PaymentNotesTooLong);
    }
}