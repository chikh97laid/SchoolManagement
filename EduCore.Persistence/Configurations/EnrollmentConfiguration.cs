using EduCore.Domain.Entities;
using EduCore.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCore.Persistence.Configurations
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.ToTable("Enrollments");

            builder.ConfigureBaseEntity();

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(x => new { x.StudentId, x.GroupId })
                .IsUnique();

            builder.HasOne(x => x.Student)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Group)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Attendances)
                .WithOne(x => x.Enrollment)
                .HasForeignKey(x => x.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.StudentFees)
                .WithOne(x => x.Enrollment)
                .HasForeignKey(x => x.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}