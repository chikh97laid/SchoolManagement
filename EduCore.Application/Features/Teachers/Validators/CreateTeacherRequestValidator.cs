
using FluentValidation;
using EduCore.Application.Features.Teachers.DTOs;
using EduCore.Application.Common;   

namespace EduCore.Application.Features.Teachers.Validators
{
    public class CreateTeacherRequestValidator : AbstractValidator<CreateTeacherRequest>
    {
        public CreateTeacherRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithErrorCode(ErrorCodes.Teacher.FirstNameRequired)
                .MaximumLength(50).WithErrorCode(ErrorCodes.Teacher.FirstNameTooLong);

            RuleFor(x => x.LastName)
                .NotEmpty().WithErrorCode(ErrorCodes.Teacher.LastNameRequired)
                .MaximumLength(50).WithErrorCode(ErrorCodes.Teacher.LastNameTooLong);

            RuleFor(x => x.Specialization)
                .MaximumLength(100).WithErrorCode(ErrorCodes.Teacher.SpecializationTooLong)
                .When(x => !string.IsNullOrEmpty(x.Specialization));

            RuleFor(x => x.HireDate)
                .LessThan(DateTime.UtcNow).WithErrorCode(ErrorCodes.Teacher.HireDateInvalid);

            RuleFor(x => x.Email)
                .EmailAddress().WithErrorCode(ErrorCodes.Teacher.EmailInvalid)
                .MaximumLength(100).WithErrorCode(ErrorCodes.Teacher.EmailTooLong)
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^(05|06|07)\d{8}$")
                .WithErrorCode(ErrorCodes.Teacher.PhoneNumberInvalid)
                .MaximumLength(10).WithErrorCode(ErrorCodes.Teacher.PhoneNumberTooLong)
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.Password)
                .NotEmpty().WithErrorCode(ErrorCodes.User.PasswordRequired)
                .MinimumLength(6).WithErrorCode(ErrorCodes.User.PasswordTooShort);
        }
    }
}