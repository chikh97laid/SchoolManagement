using EduCore.Domain.Common;
using EduCore.Domain.Entities.Enums;

namespace EduCore.Domain.Entities
{
    public class Payment : BaseEntity
    {
        
        public int StudentFeeId { get; set; }

        public StudentFee StudentFee { get; set; } = null!;

        public decimal Amount { get; set; }

        public DateOnly PaymentDate { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Completed;

        public string? Notes { get; set; }
    }
}