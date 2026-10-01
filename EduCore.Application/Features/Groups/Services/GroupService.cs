using AutoMapper;
using EduCore.Application.Common.Exceptions;
using EduCore.Application.Common.Interfaces;
using EduCore.Application.Common.Pagination;
using EduCore.Application.Features.Groups.DTOs;
using EduCore.Application.Features.Groups.Interfaces;
using EduCore.Domain.Interfaces.Repositories;
using EduCore.Domain.Entities;
using EduCore.Application.Common;

namespace EduCore.Application.Features.Groups.Services;

public class GroupService : IGroupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GroupService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    private async Task<int?> ResolveCurrentTeacherIdAsync(
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated ||
            _currentUserService.UserId == Guid.Empty)
        {
            return null;
        }

        var teacher = await _unitOfWork.Teachers
            .FirstOrDefaultAsync(
                x => x.UserId == _currentUserService.UserId,
                false,
                cancellationToken);

        return teacher?.Id;
    }

    public async Task<GroupResponse> CreateAsync(
        CreateGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var teacherExists = await _unitOfWork.Teachers
            .ExistsAsync(
                x => x.Id == request.TeacherId && x.IsActive,
                cancellationToken);

        if (!teacherExists)
            throw new BadRequestException(ErrorCodes.Group.GroupNoTeacher);

        var group = _mapper.Map<Group>(request);

        group.IsActive = true;

        await _unitOfWork.Groups.AddAsync(
            group,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        var createdGroup = await _unitOfWork.Groups
            .FirstOrDefaultAsync(
                x => x.Id == group.Id,
                trackChanges: false,
                cancellationToken: cancellationToken,
                x => x.Teacher,
                x => x.Teacher.User);

        if (createdGroup is null)
            throw new NotFoundException(ErrorCodes.Group.NotFound);

        return _mapper.Map<GroupResponse>(createdGroup);
    }

    public async Task<GroupResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.IsAdmin)
        {
            var group = await _unitOfWork.Groups
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    false,
                    cancellationToken,
                    x => x.Teacher,
                    x => x.Teacher.User);

            if (group is null)
                throw new NotFoundException(ErrorCodes.Group.NotFound);

            return _mapper.Map<GroupResponse>(group);
        }

        if (_currentUserService.IsTeacher)
        {
            var currentTeacherId = await ResolveCurrentTeacherIdAsync(cancellationToken);

            if (currentTeacherId is null)
            {
                throw new NotFoundException(ErrorCodes.Group.NotFound);
            }

            var teacherGroup = await _unitOfWork.Groups
                .FirstOrDefaultAsync(
                    x => x.Id == id
                        && x.TeacherId == currentTeacherId.Value,
                    false,
                    cancellationToken,
                    x => x.Teacher,
                    x => x.Teacher.User);

            if (teacherGroup is null)
                throw new NotFoundException(ErrorCodes.Group.NotFound);

            return _mapper.Map<GroupResponse>(teacherGroup);
        }

        throw new NotFoundException(ErrorCodes.User.NotFound);
    }

    public async Task<PagedResult<GroupResponse>> GetAllAsync(
        PaginationParameters parameters,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.IsAdmin)
        {
            var (groups, totalCount) = await _unitOfWork.Groups
                .GetPagedAsync(
                    x => true,
                    x => x.Id,
                    parameters.PageNumber,
                    parameters.PageSize,
                    false,
                    cancellationToken,
                    x => x.Teacher,
                    x => x.Teacher.User);

            return new PagedResult<GroupResponse>
            {
                Items = _mapper.Map<List<GroupResponse>>(groups),
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalCount = totalCount
            };
        }

        if(_currentUserService.IsTeacher)
        {
            var currentTeacherId = await ResolveCurrentTeacherIdAsync(cancellationToken);

            if (currentTeacherId is null)
            {
                return new PagedResult<GroupResponse>
                {
                    PageNumber = parameters.PageNumber,
                    PageSize = parameters.PageSize
                };
            }

            var (teacherGroups, totalCount) = await _unitOfWork.Groups
                .GetPagedAsync(
                    x => x.TeacherId == currentTeacherId.Value,
                    x => x.Id,
                    parameters.PageNumber,
                    parameters.PageSize,
                    false,
                    cancellationToken,
                    x => x.Teacher,
                    x => x.Teacher.User);

            return new PagedResult<GroupResponse>
            {
                Items = _mapper.Map<List<GroupResponse>>(teacherGroups),
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalCount = totalCount
            };
        }

        throw new NotFoundException(ErrorCodes.User.NotFound);
    }

    public async Task<GroupResponse> UpdateAsync(
        int id,
        UpdateGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var group = await _unitOfWork.Groups
            .FirstOrDefaultAsync(
                x => x.Id == id,
                true,
                cancellationToken,
                x => x.Teacher,
                x => x.Teacher.User);

        if (group is null)
            throw new NotFoundException(ErrorCodes.Group.NotFound);

        var teacher = await _unitOfWork.Teachers
            .FirstOrDefaultAsync(
                x => (x.Id == request.TeacherId && x.IsActive),
                false,
                cancellationToken);

        if (teacher is null)
            throw new BadRequestException(ErrorCodes.Group.GroupNoTeacher);

        _mapper.Map(request, group);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return _mapper.Map<GroupResponse>(group);
    }

    public async Task DeactivateAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var group = await _unitOfWork.Groups
            .FirstOrDefaultAsync(
                x => x.Id == id,
                true,
                cancellationToken);

        if (group is null)
            throw new NotFoundException(ErrorCodes.Group.NotFound);

        group.IsActive = false;

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }

    public async Task ActivateAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var group = await _unitOfWork.Groups
            .FirstOrDefaultAsync(
                x => x.Id == id,
                true,
                cancellationToken);

        if (group is null)
            throw new NotFoundException(ErrorCodes.Group.NotFound);

        if (!await _unitOfWork.Teachers.ExistsAsync(
        x => x.Id == group.TeacherId && x.IsActive,
        cancellationToken))
        {
            throw new BadRequestException(
                ErrorCodes.Group.GroupNoTeacher);
        }

        group.IsActive = true;

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }

    public async Task SoftDeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var group = await _unitOfWork.Groups
            .FirstOrDefaultAsync(
                x => x.Id == id,
                trackChanges: true,
                cancellationToken: cancellationToken);

        if (group is null)
            throw new NotFoundException(ErrorCodes.Group.NotFound);

        group.IsActive = false;
        group.IsDeleted = true;
        group.DeletedAt = DateTime.UtcNow;
        group.DeletedBy = _currentUserService.IsAuthenticated
            ? _currentUserService.UserId
            : null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}