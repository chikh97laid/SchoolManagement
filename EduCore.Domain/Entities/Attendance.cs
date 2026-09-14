using EduCore.Domain.Common;
using EduCore.Domain.Entities.Enums;

namespace EduCore.Domain.Entities
{
    public class Attendance : BaseEntity
    {
        
        public int EnrollmentId { get; set; }

        public Enrollment Enrollment { get; set; } = null!;

        public DateOnly Date { get; set; }

        public AttendanceStatus Status { get; set; }

        public string? Notes { get; set; }
    }
}