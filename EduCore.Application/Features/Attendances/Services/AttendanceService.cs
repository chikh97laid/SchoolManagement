using AutoMapper;
using EduCore.Application.Common;
using EduCore.Application.Common.Exceptions;
using EduCore.Application.Common.Interfaces;
using EduCore.Application.Common.Pagination;
using EduCore.Application.Features.Attendances.DTOs;
using EduCore.Application.Features.Attendances.Interfaces;
using EduCore.Domain.Entities;
using EduCore.Domain.Interfaces.Repositories;


namespace EduCore.Application.Features.Attendances.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public AttendanceService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<AttendanceResponse> CreateAsync(
        CreateAttendanceRequest request,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await GetValidEnrollmentAsync(request.EnrollmentId, cancellationToken);

        // Prevent duplicate attendance for the same enrollment/date.
        var alreadyExists = await _unitOfWork.Attendances
            .ExistsAsync(
                x => x.EnrollmentId == request.EnrollmentId
                     && x.Date == request.Date,
                cancellationToken: cancellationToken);

        if (alreadyExists)
            throw new BadRequestException(ErrorCodes.Attendance.AlreadyExists);

        var attendance = _mapper.Map<Attendance> (request);

        await _unitOfWork.Attendances.AddAsync(
            attendance,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        var createdAttendance = await _unitOfWork.Attendances
        .FirstOrDefaultAsync(
            x => x.Id == attendance.Id,
            trackChanges: false,
            cancellationToken: cancellationToken,
            x => x.Enrollment,
            x => x.Enrollment.Student,
            x => x.Enrollment.Student.User,
            x => x.Enrollment.Group);

        if (createdAttendance is null)
            throw new NotFoundException(ErrorCodes.Attendance.NotFound);

        return _mapper.Map<AttendanceResponse>(createdAttendance);
    }

    public async Task<PagedResult<AttendanceResponse>> GetAllAsync(
        PaginationParameters parameters,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.IsAdmin)
        {
            var (attendances, totalCount) = await _unitOfWork.Attendances
                .GetPagedAsync(
                x => true,
                x => x.Id,
                parameters.PageNumber,
                parameters.PageSize,
                false,
                cancellationToken,
                x => x.Enrollment,
                x => x.Enrollment.Student,
                x => x.Enrollment.Student.User,
                x => x.Enrollment.Group);

            return new PagedResult<AttendanceResponse>
            {
                Items = _mapper.Map<List<AttendanceResponse>>(attendances),
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalCount = totalCount
            };
        }

        if(_currentUserService.IsTeacher)
        {
            var (teacherAttendances, totalCount) = await _unitOfWork.Attendances
            .GetPagedAsync(
                x => x.Enrollment.Group.Teacher.UserId == _currentUserService.UserId
                && x.Enrollment.IsActive,
                x => x.Id,
                parameters.PageNumber,
                parameters.PageSize,
                trackChanges: false,
                cancellationToken: cancellationToken,
                x => x.Enrollment,
                x => x.Enrollment.Student,
                x => x.Enrollment.Student.User,
                x => x.Enrollment.Group);

            return new PagedResult<AttendanceResponse>
            {
                Items = _mapper.Map<List<AttendanceResponse>>(teacherAttendances),
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalCount = totalCount
            };
        }

        throw new NotFoundException(ErrorCodes.User.NotFound);
    }

    public async Task<AttendanceResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.IsAdmin)
        {
            var attendance = await _unitOfWork.Attendances
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    trackChanges: false,
                    cancellationToken: cancellationToken,
                        x => x.Enrollment,
                        x => x.Enrollment.Student,
                        x => x.Enrollment.Student.User,
                        x => x.Enrollment.Group);

            if (attendance is null)
                throw new NotFoundException(ErrorCodes.Attendance.NotFound);

            return _mapper.Map<AttendanceResponse>(attendance);
        }

        if (_currentUserService.IsStudent)
        {
            var attendance = await _unitOfWork.Attendances
                .FirstOrDefaultAsync(
                    x => x.Id == id
                        && x.Enrollment.Student.UserId == _currentUserService.UserId
                        && x.Enrollment.IsActive,
                    trackChanges: false,
                    cancellationToken: cancellationToken,
                        x => x.Enrollment,
                        x => x.Enrollment.Student,
                        x => x.Enrollment.Student.User,
                        x => x.Enrollment.Group);

            if (attendance is null)
                throw new NotFoundException(ErrorCodes.Attendance.NotFound);

            return _mapper.Map<AttendanceResponse>(attendance);
        }

        if(_currentUserService.IsTeacher)
        {
            var attendance = await _unitOfWork.Attendances
                .FirstOrDefaultAsync(
                    x => x.Id == id
                        && x.Enrollment.Group.Teacher.UserId == _currentUserService.UserId
                        && x.Enrollment.IsActive,
                    trackChanges: false,
                    cancellationToken: cancellationToken,
                        x => x.Enrollment,
                        x => x.Enrollment.Student,
                        x => x.Enrollment.Student.User,
                        x => x.Enrollment.Group);

            if (attendance is null)
                throw new NotFoundException(ErrorCodes.Attendance.NotFound);

            return _mapper.Map<AttendanceResponse>(attendance);
        }

        throw new NotFoundException(ErrorCodes.User.NotFound);
    }


    public async Task<IEnumerable<AttendanceResponse>> GetByEnrollmentAsync(
        int enrollmentId,
        CancellationToken cancellationToken = default)
    {
        var isEnrollmentExist = await _unitOfWork.Enrollments
                .ExistsAsync(
                x => x.Id == enrollmentId,
                cancellationToken: cancellationToken);

        if (!isEnrollmentExist)
            throw new NotFoundException(ErrorCodes.Enrollment.NotFound);

        var attendance = await _unitOfWork.Attendances
            .FindAsync(
                x => x.EnrollmentId == enrollmentId,
                trackChanges: false,
                cancellationToken: cancellationToken,
                    x => x.Enrollment,
                    x => x.Enrollment.Student,
                    x => x.Enrollment.Student.User,
                    x => x.Enrollment.Group);

        return _mapper.Map<IEnumerable<AttendanceResponse>>(attendance);
    }

    public async Task<IEnumerable<AttendanceResponse>> GetByStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        var isStudentExist = await _unitOfWork.Students
            .ExistsAsync(
            x => x.Id == studentId,
            cancellationToken: cancellationToken);

        if (!isStudentExist)
            throw new NotFoundException(ErrorCodes.Student.NotFound);

        var attendance = await _unitOfWork.Attendances
            .FindAsync(
                x => x.Enrollment.StudentId == studentId,
                trackChanges: false,
                cancellationToken: cancellationToken,
                    x => x.Enrollment,
                    x => x.Enrollment.Student,
                    x => x.Enrollment.Student.User,
                    x => x.Enrollment.Group);

        return _mapper.Map<IEnumerable<AttendanceResponse>>(
            attendance);
    }

    public async Task<AttendanceResponse> UpdateAsync(
        int id,
        UpdateAttendanceRequest request,
        CancellationToken cancellationToken = default)
    {
        var attendance = await _unitOfWork.Attendances
        .FirstOrDefaultAsync(
            x => x.Id == id
                 && x.Enrollment.IsActive
                 && !x.Enrollment.Student.IsDeleted
                 && !x.Enrollment.Group.IsDeleted,
            trackChanges: true,
            cancellationToken: cancellationToken
        );

        if (attendance is null)
            throw new NotFoundException(ErrorCodes.Attendance.NotFound);

        var alreadyExists = await _unitOfWork.Attendances
        .ExistsAsync(
            x => x.Id != id
                 && x.EnrollmentId == attendance.EnrollmentId
                 && x.Date == request.Date,
            cancellationToken: cancellationToken);

        if (alreadyExists)
            throw new BadRequestException(
                ErrorCodes.Attendance.AlreadyExists);

        _mapper.Map(request, attendance);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedAttendance = await _unitOfWork.Attendances
            .FirstOrDefaultAsync(
                x => x.Id == id,
                trackChanges: false,
                cancellationToken: cancellationToken,
                    x => x.Enrollment,
                    x => x.Enrollment.Student,
                    x => x.Enrollment.Student.User,
                    x => x.Enrollment.Group);

        if (updatedAttendance is null)
            throw new NotFoundException(ErrorCodes.Attendance.NotFound);

        return _mapper.Map<AttendanceResponse>(updatedAttendance);
    }

    public async Task SoftDeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var attendance = await _unitOfWork.Attendances
            .FirstOrDefaultAsync(
                x => x.Id == id,
                trackChanges: true,
                cancellationToken: cancellationToken);

        if (attendance is null)
            throw new NotFoundException(ErrorCodes.Attendance.NotFound);

        attendance.IsDeleted = true;
        attendance.DeletedAt = DateTime.UtcNow;
        attendance.DeletedBy = _currentUserService.IsAuthenticated
            ? _currentUserService.UserId
            : null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }


    private async Task<Enrollment> GetValidEnrollmentAsync(
    int enrollmentId,
    CancellationToken cancellationToken = default)
    {
        var enrollment = await _unitOfWork.Enrollments
            .FirstOrDefaultAsync(
                x => x.Id == enrollmentId
                     && x.IsActive
                     && !x.Student.IsActive
                     && !x.Group.IsActive,
                trackChanges: false,
                cancellationToken: cancellationToken);

        if (enrollment is null)
            throw new NotFoundException(ErrorCodes.Enrollment.NotFound);

        return enrollment;
    }

}