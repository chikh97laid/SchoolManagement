using EduCore.Domain.Common;

namespace EduCore.Domain.Entities
{
    public class Group : BaseEntity
    {

        public string Name { get; set; } = string.Empty;

        public decimal StandardFee { get; set; }

        public int TeacherId { get; set; }

        public Teacher Teacher { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public ICollection<Enrollment> Enrollments { get; set; } = [];
    }
}
