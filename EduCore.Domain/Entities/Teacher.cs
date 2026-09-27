using EduCore.Domain.Common;
using EduCore.Domain.Identity;


namespace EduCore.Domain.Entities;
public class Teacher : BaseEntity
{

    public string? Specialization { get; set; }

    public DateTime HireDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public Guid UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public ICollection<Group> Groups { get; set; } = [];
}