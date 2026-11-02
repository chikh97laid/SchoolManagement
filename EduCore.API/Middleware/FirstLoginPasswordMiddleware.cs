using System.Security.Claims;
using EduCore.Application.Common;
using EduCore.Application.Common.Exceptions;
using EduCore.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace EduCore.API.Middleware;

public sealed class FirstLoginPasswordMiddleware
{
    private readonly RequestDelegate _next;

    public FirstLoginPasswordMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager)
    {
        var path = context.Request.Path;
        var isAllowedPath =
            path.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/api/auth/logout", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/api/profile/change-password", StringComparison.OrdinalIgnoreCase) ||
            (HttpMethods.IsGet(context.Request.Method) &&
             path.Equals("/api/profile", StringComparison.OrdinalIgnoreCase));

        if (!path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase) ||
            isAllowedPath ||
            context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = string.IsNullOrWhiteSpace(userId)
            ? null
            : await userManager.FindByIdAsync(userId);

        if (user?.MustChangePassword == true)
            throw new ForbiddenException(ErrorCodes.User.MustChangePassword);

        await _next(context);
    }
}