using EduCore.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace EduCore.Persistence.Configurations
{
    public class GroupConfiguration : IEntityTypeConfiguration<Domain.Entities.Group>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.Group> builder)
        {
            builder.ToTable("Groups");

            builder.ConfigureBaseEntity();

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.StandardFee)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.HasOne(x => x.Teacher)
                .WithMany(x => x.Groups)
                .HasForeignKey(x => x.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Enrollments)
                .WithOne(x => x.Group)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}