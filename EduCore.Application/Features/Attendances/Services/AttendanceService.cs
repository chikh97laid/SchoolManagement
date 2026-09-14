using AutoMapper;
using EduCore.Application.Common.Exceptions;
using EduCore.Application.Features.Attendances.DTOs;
using EduCore.Application.Features.Attendances.Interfaces;
using EduCore.Domain.Entities;
using EduCore.Domain.Interfaces.Repositories;
using EduCore.Application.Common;

namespace EduCore.Application.Features.Attendances.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AttendanceService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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

        var attendance = _mapper.Map<Attendance>(request);

        await _unitOfWork.Attendances.AddAsync(
            attendance,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return _mapper.Map<AttendanceResponse>(
            attendance);
    }

    public async Task<IEnumerable<AttendanceResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var attendances = await _unitOfWork.Attendances
            .GetAllAsync(
                trackChanges: false,
                cancellationToken: cancellationToken,
                includes: x => x.Enrollment);

        return _mapper.Map<IEnumerable<AttendanceResponse>>(attendances);
    }

    public async Task<AttendanceResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var attendance = await _unitOfWork.Attendances
            .FirstOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted,
                trackChanges: false,
                cancellationToken: cancellationToken,
                includes: x => x.Enrollment);

        if (attendance is null)
            throw new NotFoundException(ErrorCodes.Attendance.NotFound);

        return _mapper.Map<AttendanceResponse>(
            attendance);
    }

    public async Task<IEnumerable<AttendanceResponse>> GetByEnrollmentAsync(
        int enrollmentId,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await _unitOfWork.Enrollments
            .FirstOrDefaultAsync(
                x => x.Id == enrollmentId && !x.IsDeleted,
                trackChanges: false,
                cancellationToken: cancellationToken);

        if (enrollment is null)
            throw new NotFoundException(ErrorCodes.Enrollment.NotFound);

        var attendance = await _unitOfWork.Attendances
            .FindAsync(
                x => x.EnrollmentId == enrollmentId
                     && !x.IsDeleted,
                trackChanges: false,
                cancellationToken: cancellationToken,   
                includes: x => x.Enrollment);

        return _mapper.Map<IEnumerable<AttendanceResponse>>(attendance);
    }

    public async Task<IEnumerable<AttendanceResponse>> GetByStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        var attendance = await _unitOfWork.Attendances
            .FindAsync(
                x => x.Enrollment.StudentId == studentId
                     && !x.IsDeleted,
                trackChanges: false,
                cancellationToken: cancellationToken,
                includes: x => x.Enrollment);

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

        _mapper.Map(request, attendance);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedAttendance = await _unitOfWork.Attendances
            .FirstOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted,
                trackChanges: false,
                cancellationToken: cancellationToken,
                includes: x => x.Enrollment);

        if (updatedAttendance is null)
            throw new NotFoundException(ErrorCodes.Attendance.NotFound);

        return _mapper.Map<AttendanceResponse>(updatedAttendance);
    }
}