using EduCore.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduCore.Persistence.Configurations.Common;

public static class BaseEntityConfiguration
{
    public static void ConfigureBaseEntity<TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt);

        builder.Property(x => x.CreatedBy);

        builder.Property(x => x.LastModifiedAt);

        builder.Property(x => x.LastModifiedBy);

        builder.Property(x => x.DeletedAt);

        builder.Property(x => x.DeletedBy);
    }
}