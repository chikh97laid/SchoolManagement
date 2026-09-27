using EduCore.Application.Features.Groups.DTOs;
using EduCore.Application.Common.Pagination;

namespace EduCore.Application.Features.Groups.Interfaces;

public interface IGroupService
{
    Task<GroupResponse> CreateAsync(
        CreateGroupRequest request,
        CancellationToken cancellationToken = default);

    Task<GroupResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PagedResult<GroupResponse>> GetAllAsync(
        PaginationParameters parameters,
        CancellationToken cancellationToken = default);

    Task<GroupResponse> UpdateAsync(
        int id,
        UpdateGroupRequest request,
        CancellationToken cancellationToken = default);

    Task DeactivateAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task ActivateAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}