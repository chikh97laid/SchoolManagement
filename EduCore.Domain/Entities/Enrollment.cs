using EduCore.Domain.Common;

namespace EduCore.Domain.Entities
{
    public class Enrollment : BaseEntity
    {
        
        public int StudentId { get; set; }

        public Student Student { get; set; } = null!;

        public int GroupId { get; set; }

        public Group Group { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public ICollection<Attendance> Attendances { get; set; } = [];

        public ICollection<StudentFee> StudentFees { get; set; } = [];
    }
}