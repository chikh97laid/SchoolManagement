using EduCore.Domain.Entities;
using EduCore.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCore.Persistence.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.ToTable("Students");
            
            builder.ConfigureBaseEntity();


            builder.Property(x => x.StudentCode)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasIndex(x => x.StudentCode)
                .IsUnique();

            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.GuardianName)
                .HasMaxLength(100);

            builder.Property(x => x.GuardianPhone)
                .HasMaxLength(20);

            builder.Property(x => x.Gender)
                .HasConversion<int>();

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<Student>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Enrollments)
                .WithOne(x => x.Student)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}