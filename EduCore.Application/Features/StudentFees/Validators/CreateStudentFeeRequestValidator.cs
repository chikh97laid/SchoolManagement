using EduCore.Application.Features.StudentFees.DTOs;
using FluentValidation;
using EduCore.Application.Common;

namespace EduCore.Application.Features.StudentFees.Validators;

public class CreateStudentFeeRequestValidator
    : AbstractValidator<CreateStudentFeeRequest>
{
    public CreateStudentFeeRequestValidator()
    {
        RuleFor(x => x.EnrollmentId)
            .GreaterThan(0)
            .WithErrorCode(ErrorCodes.StudentFee.EnrollmentIdInvalid);

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.StudentFee.TitleRequired)
            .MaximumLength(200)
            .WithErrorCode(ErrorCodes.StudentFee.TitleTooLong);

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0)
            .WithErrorCode(ErrorCodes.StudentFee.TotalAmountInvalid);

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithErrorCode(ErrorCodes.StudentFee.DueDateInvalid)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)))
            .WithErrorCode(ErrorCodes.StudentFee.DueDateInvalid);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithErrorCode(ErrorCodes.StudentFee.DescriptionTooLong);
    }
}