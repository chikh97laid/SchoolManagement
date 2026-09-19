using System.Security.Claims;
using EduCore.Application.Common.Interfaces;
using EduCore.Shared.Constants;
using Microsoft.AspNetCore.Http;

namespace EduCore.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userId, out var id)
                ? id
                : Guid.Empty;
        }
    }

    public string? UserName =>
        _httpContextAccessor.HttpContext?
            .User
            .Identity?
            .Name;

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?
            .User
            .Identity?
            .IsAuthenticated ?? false;

    public bool IsAdmin =>
        _httpContextAccessor.HttpContext?
            .User
            .IsInRole(Roles.Admin) ?? false;
}