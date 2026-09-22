using EduCore.Application.Features.Attendances.DTOs;
using FluentValidation;
using EduCore.Application.Common;   

namespace EduCore.Application.Features.Attendances.Validators
{
    public class UpdateAttendanceRequestValidator : AbstractValidator<UpdateAttendanceRequest>
    {
        public UpdateAttendanceRequestValidator()
        {
            RuleFor(x => x.Date)
                .NotEmpty()
                .WithErrorCode(ErrorCodes.Attendance.DateRequired)
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                .WithErrorCode(ErrorCodes.Attendance.DateInFuture)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30))) // يمنع التواريخ الأقدم من 30 يوماً
                .WithErrorCode(ErrorCodes.Attendance.DateTooOld);

            RuleFor(x => x.Status)
                .IsInEnum().WithErrorCode(ErrorCodes.Attendance.InvalidStatus);

            RuleFor(x => x.Notes)
                .MaximumLength(300).WithErrorCode(ErrorCodes.Attendance.NotesTooLong);
        }
    }
}
