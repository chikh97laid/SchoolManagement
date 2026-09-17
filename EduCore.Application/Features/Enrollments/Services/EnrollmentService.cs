using AutoMapper;
using EduCore.Application.Common;
using EduCore.Application.Common.Exceptions;
using EduCore.Application.Common.Interfaces;
using EduCore.Application.Features.Enrollments.DTOs;
using EduCore.Application.Features.Enrollments.Interfaces;
using EduCore.Domain.Entities;
using EduCore.Domain.Interfaces.Repositories;

namespace EduCore.Application.Features.Enrollments.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public EnrollmentService(
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
                x => x.UserId == _currentUserService.UserId && !x.IsDeleted,
                false,
                cancellationToken);

        return teacher?.Id;
    }

    public async Task<EnrollmentResponse> CreateAsync(
        CreateEnrollmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Students
            .FirstOrDefaultAsync(
                x => (x.Id == request.StudentId && !x.IsDeleted && x.IsActive),
                false,
                cancellationToken);

        if (student is null)
        {
            throw new BadRequestException(ErrorCodes.Enrollment.NotFound);
        }

        var group = await _unitOfWork.Groups
            .FirstOrDefaultAsync(
                x => x.Id == request.GroupId
                     && !x.IsDeleted
                     && x.IsActive,
                false,
                cancellationToken,
                x => x.Teacher);

        if (group is null)
        {
            throw new BadRequestException(ErrorCodes.Group.NotFound);
        }

        var alreadyEnrolled = await _unitOfWork.Enrollments
            .ExistsAsync(
                x => x.StudentId == request.StudentId
                     && x.GroupId == request.GroupId
                     && !x.IsDeleted, 
            cancellationToken);

        if (alreadyEnrolled)
        {
            throw new ConflictException(ErrorCodes.Student.StudentAlreadyEnrolled);
        }

        var enrollment = _mapper.Map<Enrollment>(request);

        enrollment.IsActive = true;

        await _unitOfWork.Enrollments.AddAsync(
            enrollment,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        enrollment = await _unitOfWork.Enrollments
            .GetWithDetailsAsync(enrollment.Id, cancellationToken);

        return _mapper.Map<EnrollmentResponse>(
            enrollment);
    }

    public async Task<EnrollmentResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var currentTeacherId = await ResolveCurrentTeacherIdAsync(cancellationToken);

        Enrollment? enrollment;

        if (currentTeacherId.HasValue)
        {
            enrollment = await _unitOfWork.Enrollments
                .FirstOrDefaultAsync(
                    x => x.Id == id
                        && !x.IsDeleted
                        && x.Group.TeacherId == currentTeacherId.Value,
                    false,
                    cancellationToken,
                    x => x.Student,
                    x => x.Group,
                    x => x.Group.Teacher);
        }
        else
        {
            enrollment = await _unitOfWork.Enrollments
                .GetWithDetailsAsync(
                    id,
                    cancellationToken);
        }

        if (enrollment is null)
        {
            throw new NotFoundException(ErrorCodes.Enrollment.NotFound);
        }

        // Group.Teacher is needed by the response mapping.
        if (enrollment.Group.Teacher is null)
        {
            throw new BadRequestException(ErrorCodes.Group.GroupNoTeacher);
        }

        return _mapper.Map<EnrollmentResponse>(
            enrollment);
    }

    public async Task<IEnumerable<EnrollmentResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var enrollments = await _unitOfWork.Enrollments
            .GetAllWithDetailsAsync();

        return _mapper.Map<IEnumerable<EnrollmentResponse>>(
            enrollments);
    }

    public async Task DeactivateAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await _unitOfWork.Enrollments
            .FirstOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted,
                true,
                cancellationToken);

        if (enrollment is null)
        {
            throw new NotFoundException(ErrorCodes.Enrollment.NotFound);
        }

        enrollment.IsActive = false;

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }

    public async Task ActivateAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await _unitOfWork.Enrollments
            .FirstOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted,
                true,
                cancellationToken);

        if (enrollment is null)
        {
            throw new NotFoundException(ErrorCodes.Enrollment.NotFound);
        }

        enrollment.IsActive = true;

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}