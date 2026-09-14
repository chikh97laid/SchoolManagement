using EduCore.Application.Features.Authentication.DTOs.Login;

namespace EduCore.Application.Features.Authentication.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}