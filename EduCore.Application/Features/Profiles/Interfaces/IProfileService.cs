using EduCore.Application.Features.Profiles.DTOs;

namespace EduCore.Application.Features.Profiles.Interfaces;

public interface IProfileService
{
    Task<ProfileResponse> GetMyProfileAsync(
        CancellationToken cancellationToken = default);

    Task<ProfileResponse> UpdateMyProfileAsync(
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default);
}