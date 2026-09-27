using AutoMapper;
using EduCore.Application.Common;
using EduCore.Application.Common.Exceptions;
using EduCore.Application.Common.Interfaces;
using EduCore.Application.Common.Pagination;
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

    public async Task<EnrollmentResponse> CreateAsync(
        CreateEnrollmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Students
            .FirstOrDefaultAsync(
                x => (x.Id == request.StudentId),
                false,
                cancellationToken);

        if (student is null)
        {
            throw new NotFoundException(ErrorCodes.Student.NotFound);
        }

        if (!student.IsActive)
        {
            throw new BadRequestException(ErrorCodes.Student.NotActive);
        }

        var group = await _unitOfWork.Groups
            .FirstOrDefaultAsync(
                x => x.Id == request.GroupId,
                false,
                cancellationToken,
                x => x.Teacher);

        if (group is null)
        {
            throw new NotFoundException(ErrorCodes.Group.NotFound);
        }

        if (!group.IsActive)
        {
            throw new BadRequestException(ErrorCodes.Group.NotActive);
        }


        var alreadyEnrolled = await _unitOfWork.Enrollments
            .ExistsAsync(
                x => x.StudentId == request.StudentId
                     && x.GroupId == request.GroupId,
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
        if (_currentUserService.IsAdmin)
        {
            var enrollment = await _unitOfWork.Enrollments
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    false,
                    cancellationToken,
                    x => x.Student,
                    x => x.Student.User,
                    x => x.Group,
                    x => x.Group.Teacher,
                    x => x.Group.Teacher.User);

            if (enrollment is null)
            {
                throw new NotFoundException(ErrorCodes.Enrollment.NotFound);
            }

            return _mapper.Map<EnrollmentResponse>(enrollment);
        }

        if (_currentUserService.IsStudent)
        {
            var studentEnrollment = await _unitOfWork.Enrollments
            .FirstOrDefaultAsync(
                x => x.Id == id
                     && x.Student.UserId == _currentUserService.UserId,
                false,
                cancellationToken,
                x => x.Student,
                x => x.Student.User,
                x => x.Group,
                x => x.Group.Teacher,
                x => x.Group.Teacher.User
            );


            if (studentEnrollment is null)
                throw new NotFoundException(ErrorCodes.Enrollment.NotFound);

            return _mapper.Map<EnrollmentResponse>(studentEnrollment);
        }

        if(_currentUserService.IsTeacher)
        {
            var teacherEnrollment = await _unitOfWork.Enrollments
                .FirstOrDefaultAsync(
                    x => x.Id == id
                         && x.Group.Teacher.UserId == _currentUserService.UserId,
                    false,
                    cancellationToken,
                    x => x.Student,
                    x => x.Student.User,
                    x => x.Group,
                    x => x.Group.Teacher,
                    x => x.Group.Teacher.User
                );


            if (teacherEnrollment is null)
            {
                throw new NotFoundException(ErrorCodes.Enrollment.NotFound);
            }

            // Group.Teacher is needed by the response mapping.
            if (teacherEnrollment.Group.Teacher is null)
            {
                throw new BadRequestException(ErrorCodes.Group.GroupNoTeacher);
            }

            return _mapper.Map<EnrollmentResponse>(
                teacherEnrollment);
        }

        throw new NotFoundException(ErrorCodes.User.NotFound);
    }

    public async Task<PagedResult<EnrollmentResponse>> GetAllAsync(
        PaginationParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var (enrollments, totalCount) = await _unitOfWork.Enrollments
            .GetPagedAsync(
                x => true,
                x => x.Id,
                parameters.PageNumber,
                parameters.PageSize,
                false,
                cancellationToken,
                x => x.Student,
                x => x.Student.User,
                x => x.Group,
                x => x.Group.Teacher,
                x => x.Group.Teacher.User);

        return new PagedResult<EnrollmentResponse>
        {
            Items = _mapper.Map<List<EnrollmentResponse>>(enrollments),
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task DeactivateAsync(
    int id,
    CancellationToken cancellationToken = default)
    {
        var enrollment = await _unitOfWork.Enrollments
            .FirstOrDefaultAsync(
                x => x.Id == id,
                trackChanges: true,
                cancellationToken: cancellationToken);

        if (enrollment is null)
            throw new NotFoundException(
                ErrorCodes.Enrollment.NotFound);

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
                x => x.Id == id,
                trackChanges: true,
                cancellationToken: cancellationToken,
                x => x.Student,
                x => x.Group);

        if (enrollment is null)
            throw new NotFoundException(
                ErrorCodes.Enrollment.NotFound);

        if (!enrollment.Student.IsActive)
            throw new BadRequestException(
                ErrorCodes.Student.NotActive);

        if (!enrollment.Group.IsActive)
            throw new BadRequestException(
                ErrorCodes.Group.NotActive);

        enrollment.IsActive = true;

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }

    public async Task SoftDeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await _unitOfWork.Enrollments
            .FirstOrDefaultAsync(
                x => x.Id == id,
                trackChanges: true,
                cancellationToken: cancellationToken);

        if (enrollment is null)
            throw new NotFoundException(ErrorCodes.Enrollment.NotFound);

        enrollment.IsActive = false;
        enrollment.IsDeleted = true;
        enrollment.DeletedAt = DateTime.UtcNow;
        enrollment.DeletedBy = _currentUserService.IsAuthenticated
            ? _currentUserService.UserId
            : null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

}