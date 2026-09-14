using EduCore.Domain.Entities;
using EduCore.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCore.Persistence.Configurations
{
    public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> builder)
        {
            builder.ToTable("Attendances");

            builder.ConfigureBaseEntity();

            builder.Property(x => x.Status)
                .HasConversion<int>();

            builder.Property(x => x.Notes)
                .HasMaxLength(300);

            builder.HasIndex(x => new { x.EnrollmentId, x.Date })
                .IsUnique();

            builder.HasOne(x => x.Enrollment)
                .WithMany(x => x.Attendances)
                .HasForeignKey(x => x.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}