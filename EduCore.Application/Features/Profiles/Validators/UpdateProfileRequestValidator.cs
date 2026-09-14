using EduCore.Application.Features.Profiles.DTOs;
using FluentValidation;
using EduCore.Application.Common;

namespace EduCore.Application.Features.Profiles.Validators
{
    public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
    {
        public UpdateProfileRequestValidator() 
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithErrorCode(ErrorCodes.User.FirstNameRequired)
                .MaximumLength(50).WithErrorCode(ErrorCodes.User.FirstNameTooLong);

            RuleFor(x => x.LastName)
                .NotEmpty().WithErrorCode(ErrorCodes.User.LastNameRequired)
                .MaximumLength(50).WithErrorCode(ErrorCodes.User.LastNameTooLong);

            RuleFor(x => x.Email)
                .EmailAddress()
                .WithErrorCode(ErrorCodes.User.EmailInvalid)
                .MaximumLength(100)
                .WithErrorCode(ErrorCodes.User.EmailTooLong)
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^(05|06|07)\d{8}$")
                .WithErrorCode(ErrorCodes.User.PhoneNumberInvalid)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        }
    }
}
