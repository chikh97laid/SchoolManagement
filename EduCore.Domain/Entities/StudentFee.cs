using EduCore.Domain.Common;
using EduCore.Domain.Entities.Enums;

namespace EduCore.Domain.Entities
{
    public class StudentFee : BaseEntity
    {
        
        public int EnrollmentId { get; set; }

        public Enrollment Enrollment { get; set; } = null!;

        public string Title { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public decimal PaidAmount { get; set; } = 0;

        public DateOnly? DueDate { get; set; }

        public StudentFeeStatus Status { get; set; } = StudentFeeStatus.Pending;

        public string? Description { get; set; }

        public ICollection<Payment> Payments { get; set; } = [];
    }
}