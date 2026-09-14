

namespace EduCore.Application.Features.Teachers.DTOs;

public class CreateTeacherResponse
{
    public int TeacherId { get; set; }

    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;
}