using AutoMapper;
using EduCore.Application.Common;
using EduCore.Application.Common.Exceptions;
using EduCore.Application.Common.Interfaces;
using EduCore.Application.Features.Attendances.DTOs;
using EduCore.Application.Features.Attendances.Interfaces;
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
        var enrollment = await _unitOfWork.Enrollments
            .FirstOrDefaultAsync(
                x => x.Id == request.EnrollmentId
                     && !x.IsDeleted,
                trackChanges: false,
                cancellationToken: cancellationToken);

        if (enrollment is null)
            throw new NotFoundException(ErrorCodes.Enrollment.NotFound);

        if (!enrollment.IsActive)
            throw new BadRequestException(ErrorCodes.Enrollment.NotActive);

        // Prevent duplicate attendance for the same enrollment/date.
        var alreadyExists = await _unitOfWork.Attendances
            .ExistsAsync(
                x => x.EnrollmentId == request.EnrollmentId
                     && x.Date == request.Date
                     && !x.IsDeleted,
                cancellationToken: cancellationToken);

        if (alreadyExists)
            throw new BadRequestException(ErrorCodes.Attendance.AlreadyExists);

        var attendance = _mapper.Map<Domain.Entities.Attendance> (request);

        await _unitOfWork.Attendances.AddAsync(
            attendance,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        var createdAttendance = await _unitOfWork.Attendances
        .FirstOrDefaultAsync(
            x => x.Id == attendance.Id && !x.IsDeleted,
            trackChanges: false,
            cancellationToken: cancellationToken,
            x => x.Enrollment,
            x => x.Enrollment.Student,
            x => x.Enrollment.Group);

        if (createdAttendance is null)
            throw new NotFoundException(ErrorCodes.Attendance.NotFound);

        return _mapper.Map<AttendanceResponse>(createdAttendance);
    }

    public async Task<IEnumerable<AttendanceResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.IsAdmin)
        {
            var attendances = await _unitOfWork.Attendances
                .FindAsync(
                x => !x.IsDeleted,
                false,
                cancellationToken,
                x => x.Enrollment,
                x => x.Enrollment.Student,
                x => x.Enrollment.Group);

            return _mapper.Map<IEnumerable<AttendanceResponse>>(attendances);
        }

        if(_currentUserService.IsTeacher)
        {
            var teacherAttendances = await _unitOfWork.Attendances
            .FindAsync(
                x => !x.IsDeleted
                    && x.Enrollment.Group.Teacher.UserId == _currentUserService.UserId,
                trackChanges: false,
                cancellationToken: cancellationToken,
                x => x.Enrollment,
                x => x.Enrollment.Student,
                x => x.Enrollment.Group);

            return _mapper.Map<IEnumerable<AttendanceResponse>>(teacherAttendances);
        }

        throw new ForbiddenException(ErrorCodes.User.Forbidden);
    }

    public async Task<AttendanceResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.IsAdmin)
        {
            var attendance = await _unitOfWork.Attendances
                .FirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDeleted,
                    trackChanges: false,
                    cancellationToken: cancellationToken,
                        x => x.Enrollment,
                        x => x.Enrollment.Student,
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
                        && !x.IsDeleted
                        && x.Enrollment.Student.UserId == _currentUserService.UserId,
                    trackChanges: false,
                    cancellationToken: cancellationToken,
                        x => x.Enrollment,
                        x => x.Enrollment.Student,
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
                        && !x.IsDeleted
                        && x.Enrollment.Group.Teacher.UserId == _currentUserService.UserId,
                    trackChanges: false,
                    cancellationToken: cancellationToken,
                        x => x.Enrollment,
                        x => x.Enrollment.Student,
                        x => x.Enrollment.Group);

            if (attendance is null)
                throw new NotFoundException(ErrorCodes.Attendance.NotFound);

            return _mapper.Map<AttendanceResponse>(attendance);
        }

        throw new ForbiddenException(ErrorCodes.User.Forbidden);
    }


    public async Task<IEnumerable<AttendanceResponse>> GetByEnrollmentAsync(
        int enrollmentId,
        CancellationToken cancellationToken = default)
    {
        var isEnrollmentExist = await _unitOfWork.Enrollments
                .ExistsAsync(
                x => x.Id == enrollmentId
                     && !x.IsDeleted,
                cancellationToken: cancellationToken);

        if (!isEnrollmentExist)
            throw new NotFoundException(ErrorCodes.Enrollment.NotFound);

        var attendance = await _unitOfWork.Attendances
            .FindAsync(
                x => x.EnrollmentId == enrollmentId
                     && !x.IsDeleted,
                trackChanges: false,
                cancellationToken: cancellationToken,
                    x => x.Enrollment,
                    x => x.Enrollment.Student,
                    x => x.Enrollment.Group);

        return _mapper.Map<IEnumerable<AttendanceResponse>>(attendance);
    }

    public async Task<IEnumerable<AttendanceResponse>> GetByStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        var isStudentExist = await _unitOfWork.Students
            .ExistsAsync(
            x => x.Id == studentId
                 && !x.IsDeleted,
            cancellationToken: cancellationToken);

        if (!isStudentExist)
            throw new NotFoundException(ErrorCodes.Student.NotFound);

        var attendance = await _unitOfWork.Attendances
            .FindAsync(
                x => x.Enrollment.StudentId == studentId
                     && !x.IsDeleted,
                trackChanges: false,
                cancellationToken: cancellationToken,
                    x => x.Enrollment,
                    x => x.Enrollment.Student,
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
                x => x.Id == id && !x.IsDeleted,
                trackChanges: true,
                cancellationToken: cancellationToken);

        if (attendance is null)
            throw new NotFoundException(ErrorCodes.Attendance.NotFound);

        var alreadyExists = await _unitOfWork.Attendances
        .ExistsAsync(
            x => x.Id != id
                 && x.EnrollmentId == attendance.EnrollmentId
                 && x.Date == request.Date
                 && !x.IsDeleted,
            cancellationToken: cancellationToken);

        if (alreadyExists)
            throw new BadRequestException(
                ErrorCodes.Attendance.AlreadyExists);

        _mapper.Map(request, attendance);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedAttendance = await _unitOfWork.Attendances
            .FirstOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted,
                trackChanges: false,
                cancellationToken: cancellationToken,
                    x => x.Enrollment,
                    x => x.Enrollment.Student,
                    x => x.Enrollment.Group);

        if (updatedAttendance is null)
            throw new NotFoundException(ErrorCodes.Attendance.NotFound);

        return _mapper.Map<AttendanceResponse>(updatedAttendance);
    }
}