using FluentValidation;
using EduCore.Application.Features.Groups.DTOs;
using EduCore.Application.Common;

namespace EduCore.Application.Features.Groups.Validators;

public class UpdateGroupRequestValidator
    : AbstractValidator<UpdateGroupRequest>
{
    public UpdateGroupRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Group.NameRequired)
            .MaximumLength(100)
            .WithErrorCode(ErrorCodes.Group.NameTooLong);

        RuleFor(x => x.StandardFee)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode(ErrorCodes.Group.StandardFeeInvalid);

        RuleFor(x => x.TeacherId)
            .GreaterThan(0)
            .WithErrorCode(ErrorCodes.Group.TeacherIdInvalid);
    }
}