using EduCore.Application.Features.Authentication.DTOs.Login;
using FluentValidation;
using EduCore.Application.Common;

namespace EduCore.Application.Features.Authentication.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.User.UserNameRequired)
            .MaximumLength(20)
            .WithErrorCode(ErrorCodes.User.UserNameTooLong);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.User.PasswordRequired)
            .MinimumLength(6)
            .WithErrorCode(ErrorCodes.User.PasswordTooShort)
            .MaximumLength(100)
            .WithErrorCode(ErrorCodes.User.PasswordTooLong);
    }
}