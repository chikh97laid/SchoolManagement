using EduCore.Domain.Common;
using EduCore.Domain.Identity;


namespace EduCore.Domain.Entities;
public class Teacher : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Specialization { get; set; }

    public DateTime HireDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public Guid? UserId { get; set; }

    public ApplicationUser? User { get; set; }

    public ICollection<Group> Groups { get; set; } = [];
}