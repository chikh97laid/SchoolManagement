namespace EduCore.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? LastModifiedAt { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }
}