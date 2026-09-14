using EduCore.Domain.Common;
using EduCore.Domain.Identity;
using EduCore.Domain.Entities.Enums;

namespace EduCore.Domain.Entities
{ 
    public class Student : BaseEntity
    {
        public string StudentCode { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? GuardianName { get; set; }

        public string? GuardianPhone { get; set; }

        public DateOnly? BirthDate { get; set; }

        public Gender Gender { get; set; }

        public bool IsActive { get; set; } = true;

        public Guid? UserId { get; set; }

        public ApplicationUser? User { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = [];
    }
}