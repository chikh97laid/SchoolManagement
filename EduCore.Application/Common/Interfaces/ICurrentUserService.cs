namespace EduCore.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }

    string? UserName { get; }

    bool IsAuthenticated { get; }

    bool IsAdmin { get; }

    bool IsStudent { get; }

    bool IsTeacher { get; }
}