using EduCore.Application.Common;
using EduCore.Application.Features.StudentManagement.DTOs.Student;
using FluentValidation;

namespace EduCore.Application.Features.StudentManagement.Validators;

public class ImportStudentsRequestValidator : AbstractValidator<ImportStudentsRequest>
{
    public ImportStudentsRequestValidator()
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
    }
}