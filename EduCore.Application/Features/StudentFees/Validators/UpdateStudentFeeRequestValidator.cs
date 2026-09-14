using EduCore.Application.Common;
using EduCore.Application.Features.StudentFees.DTOs;
using FluentValidation;

namespace EduCore.Application.Features.StudentFees.Validators;

public class UpdateStudentFeeRequestValidator
    : AbstractValidator<UpdateStudentFeeRequest>
{
    public UpdateStudentFeeRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.StudentFee.TitleRequired)
            .MaximumLength(200)
            .WithErrorCode(ErrorCodes.StudentFee.TitleTooLong);

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0)
            .WithErrorCode(ErrorCodes.StudentFee.TotalAmountInvalid);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithErrorCode(ErrorCodes.StudentFee.DescriptionTooLong);
    }
}