using EduCore.Application.Common;
using EduCore.Application.Features.Attendances.DTOs;
using FluentValidation;


namespace EduCore.Application.Features.Attendances.Validators
{
    public class CreateAttendanceRequestValidator : AbstractValidator<CreateAttendanceRequest>
    {
        public CreateAttendanceRequestValidator() 
        {
            RuleFor(x => x.EnrollmentId)
                        .GreaterThan(0)
                        .WithErrorCode(ErrorCodes.Attendance.InvalidEnrollmentId);

            RuleFor(x => x.Date)
                .NotEmpty()
                .WithErrorCode(ErrorCodes.Attendance.DateRequired)
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now))
                .WithErrorCode(ErrorCodes.Attendance.DateInFuture);

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithErrorCode(ErrorCodes.Attendance.InvalidStatus);

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .WithErrorCode(ErrorCodes.Attendance.NotesTooLong);
        }
    }
}
