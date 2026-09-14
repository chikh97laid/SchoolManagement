
using EduCore.Application.Features.Profiles.DTOs;
using FluentValidation;
using EduCore.Application.Common;

namespace EduCore.Application.Features.Profiles.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.User.CurrentPasswordRequired)
            .MinimumLength(6)
            .WithErrorCode(ErrorCodes.User.CurrentPasswordTooShort)
            .MaximumLength(100)
            .WithErrorCode(ErrorCodes.User.CurrentPasswordTooLong);

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.User.NewPasswordRequired)
            .MinimumLength(6)
            .WithErrorCode(ErrorCodes.User.NewPasswordTooShort)
            .MaximumLength(100)
            .WithErrorCode(ErrorCodes.User.NewPasswordTooLong);

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.User.ConfirmNewPasswordRequired)
            .MinimumLength(6)
            .WithErrorCode(ErrorCodes.User.ConfirmNewPasswordTooShort)
            .MaximumLength(100)
            .WithErrorCode(ErrorCodes.User.ConfirmNewPasswordTooLong);


        RuleFor(x => x)
            .Must(x => x.NewPassword == x.ConfirmNewPassword)
            .WithErrorCode(ErrorCodes.User.NewPasswordMismatch);
    }
}