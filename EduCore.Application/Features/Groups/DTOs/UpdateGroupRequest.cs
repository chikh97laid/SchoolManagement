namespace EduCore.Application.Features.Groups.DTOs;

public class UpdateGroupRequest
{
    public string Name { get; set; } = string.Empty;

    public decimal StandardFee { get; set; }

    public int TeacherId { get; set; }
}