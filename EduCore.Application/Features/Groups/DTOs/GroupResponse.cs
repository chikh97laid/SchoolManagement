namespace EduCore.Application.Features.Groups.DTOs;

public class GroupResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal StandardFee { get; set; }

    public int TeacherId { get; set; }

    public string TeacherName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}