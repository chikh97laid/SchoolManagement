using EduCore.Application.Features.StudentManagement.DTOs.Student;
using FluentValidation;
using EduCore.Application.Common;

namespace EduCore.Application.Features.StudentManagement.Validators;

public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {

        RuleFor(x => x.FirstName)
            .NotEmpty().WithErrorCode(ErrorCodes.Student.FirstNameRequired)
            .MaximumLength(50).WithErrorCode(ErrorCodes.Student.FirstNameTooLong);

        RuleFor(x => x.LastName)
            .NotEmpty().WithErrorCode(ErrorCodes.Student.LastNameRequired)
            .MaximumLength(50).WithErrorCode(ErrorCodes.Student.LastNameTooLong);

        RuleFor(x => x.GuardianName)
            .MaximumLength(100).WithErrorCode(ErrorCodes.Student.GuardianNameTooLong)
            .When(x => !string.IsNullOrEmpty(x.GuardianName));

        RuleFor(x => x.GuardianPhone)
            .Matches(@"^(05|06|07)\d{8}$")
            .WithErrorCode(ErrorCodes.Student.GuardianPhoneInvalid)
            .When(x => !string.IsNullOrEmpty(x.GuardianPhone));

        RuleFor(x => x.BirthDate)
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithErrorCode(ErrorCodes.Student.BirthDateInvalid)
            .GreaterThan(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-100)))
            .WithErrorCode(ErrorCodes.Student.BirthDateInvalid)
            .When(x => x.BirthDate.HasValue);

        RuleFor(x => x.Gender)
            .IsInEnum().WithErrorCode(ErrorCodes.Student.GenderInvalid);

        RuleFor(x => x.Email)
            .EmailAddress().WithErrorCode(ErrorCodes.Student.EmailInvalid)
            .MaximumLength(100).WithErrorCode(ErrorCodes.Student.EmailTooLong)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode(ErrorCodes.User.PasswordRequired)
            .MinimumLength(6).WithErrorCode(ErrorCodes.User.PasswordTooShort)
            .MaximumLength(100).WithErrorCode(ErrorCodes.User.PasswordTooLong);
    }
}