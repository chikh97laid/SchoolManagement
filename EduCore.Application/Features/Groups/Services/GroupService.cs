using AutoMapper;
using EduCore.Application.Common.Exceptions;
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

    public GroupService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<GroupResponse> CreateAsync(
        CreateGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var teacher = await _unitOfWork.Teachers
            .FirstOrDefaultAsync(
                x => (x.Id == request.TeacherId && !x.IsDeleted && x.IsActive),
                     true,
                     cancellationToken);

        if (teacher is null)
            throw new BadRequestException(ErrorCodes.Group.GroupNoTeacher);

        var group = _mapper.Map<Group>(request);

        group.IsActive = true;

        await _unitOfWork.Groups.AddAsync(
            group,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return _mapper.Map<GroupResponse>(group);
    }

    public async Task<GroupResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var group = await _unitOfWork.Groups
            .FirstOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted,
                false,
                cancellationToken,
                x => x.Teacher);

        if (group is null)
            throw new NotFoundException(ErrorCodes.Group.NotFound);

        return _mapper.Map<GroupResponse>(group);
    }

    public async Task<IEnumerable<GroupResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var groups = await _unitOfWork.Groups
            .FindAsync(
                x => !x.IsDeleted,
                false,
                cancellationToken,
                x => x.Teacher);

        return _mapper.Map<IEnumerable<GroupResponse>>(groups);
    }

    public async Task<GroupResponse> UpdateAsync(
        int id,
        UpdateGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        var group = await _unitOfWork.Groups
            .FirstOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted,
                true,
                cancellationToken,
                x => x.Teacher);

        if (group is null)
            throw new NotFoundException(ErrorCodes.Group.NotFound);

        var teacher = await _unitOfWork.Teachers
            .FirstOrDefaultAsync(
                x => (x.Id == request.TeacherId && !x.IsDeleted && x.IsActive),
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
                x => x.Id == id && !x.IsDeleted,
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
                x => x.Id == id && !x.IsDeleted,
                true,
                cancellationToken);

        if (group is null)
            throw new NotFoundException(ErrorCodes.Group.NotFound);

        group.IsActive = true;

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}