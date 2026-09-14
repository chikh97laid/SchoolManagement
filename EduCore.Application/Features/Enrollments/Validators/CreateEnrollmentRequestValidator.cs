using EduCore.Application.Features.Enrollments.DTOs;
using FluentValidation;
using EduCore.Application.Common;

namespace EduCore.Application.Features.Enrollments.Validators;

public class CreateEnrollmentRequestValidator
    : AbstractValidator<CreateEnrollmentRequest>
{
    public CreateEnrollmentRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0).WithErrorCode(ErrorCodes.Enrollment.StudentIdInvalid);

        RuleFor(x => x.GroupId)
            .GreaterThan(0).WithErrorCode(ErrorCodes.Enrollment.GroupIdInvalid);
    }
}