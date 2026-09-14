using EduCore.Domain.Entities;
using EduCore.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCore.Persistence.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.ConfigureBaseEntity();

            builder.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.PaymentMethod)
                .HasConversion<int>();

            builder.Property(x => x.Notes)
                .HasMaxLength(300);

            builder.HasOne(x => x.StudentFee)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.StudentFeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}