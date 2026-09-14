using EduCore.Domain.Entities;
using EduCore.Domain.Entities.Enums;
using EduCore.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCore.Persistence.Configurations
{
    public class StudentFeeConfiguration : IEntityTypeConfiguration<StudentFee>
    {
        public void Configure(EntityTypeBuilder<StudentFee> builder)
        {
            builder.ToTable("StudentFees");

            builder.ConfigureBaseEntity();

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.TotalAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.PaidAmount)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .HasDefaultValue(StudentFeeStatus.Pending);

            builder.Property(x => x.Description)
                .HasMaxLength(300);

            builder.HasOne(x => x.Enrollment)
                .WithMany(x => x.StudentFees)
                .HasForeignKey(x => x.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Payments)
                .WithOne(x => x.StudentFee)
                .HasForeignKey(x => x.StudentFeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}