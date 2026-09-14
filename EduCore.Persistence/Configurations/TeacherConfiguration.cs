using EduCore.Domain.Entities;
using EduCore.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCore.Persistence.Configurations
{
    public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
    {
        public void Configure(EntityTypeBuilder<Teacher> builder)
        {
            builder.ToTable("Teachers");

            builder.ConfigureBaseEntity();

            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Specialization)
                .HasMaxLength(100);

            builder.Property(x => x.HireDate)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);


            builder.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<Teacher>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Groups)
                .WithOne(x => x.Teacher)
                .HasForeignKey(x => x.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}