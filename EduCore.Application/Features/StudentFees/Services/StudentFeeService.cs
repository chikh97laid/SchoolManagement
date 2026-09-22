using AutoMapper;
using EduCore.Application.Common.Exceptions;
using EduCore.Application.Common.Interfaces;
using EduCore.Application.Features.StudentFees.DTOs;
using EduCore.Application.Features.StudentFees.Interfaces;
using EduCore.Domain.Entities;
using EduCore.Domain.Interfaces.Repositories;
using EduCore.Application.Common;

namespace EduCore.Application.Features.StudentFees.Services;

public class StudentFeeService : IStudentFeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public StudentFeeService(
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

    public async Task<StudentFeeResponse> CreateAsync(
        CreateStudentFeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await _unitOfWork.Enrollments
            .FirstOrDefaultAsync(
                x => x.Id == request.EnrollmentId && !x.IsDeleted,
                false,
                cancellationToken);

        if (enrollment is null)
            throw new NotFoundException(ErrorCodes.Enrollment.NotFound);

        if (!enrollment.IsActive)
            throw new BadRequestException(
                ErrorCodes.Enrollment.NotActive);

        var fee = _mapper.Map<StudentFee>(request);

        await _unitOfWork.StudentFees.AddAsync(
            fee,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdFee = await _unitOfWork.StudentFees
            .GetWithDetailsAsync(
                fee.Id,
                cancellationToken);

        if (createdFee is null)
            throw new NotFoundException(
                ErrorCodes.StudentFee.NotFound);

        return _mapper.Map<StudentFeeResponse>(createdFee);
    }

    public async Task<StudentFeeResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.IsAdmin)
        {
            var fee = await _unitOfWork.StudentFees
                .FirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDeleted,
                    false,
                    cancellationToken,
                    x => x.Enrollment,
                    x => x.Enrollment.Student,
                    x => x.Enrollment.Group,
                    x => x.Enrollment.Group.Teacher);

            if (fee is null || fee.IsDeleted)
                throw new NotFoundException(ErrorCodes.StudentFee.NotFound);

            return _mapper.Map<StudentFeeResponse>(fee);
        }

        if (_currentUserService.IsStudent)
        {
            var currentStudent = await _unitOfWork.Students
                .FirstOrDefaultAsync(
                    x => x.UserId == _currentUserService.UserId && !x.IsDeleted,
                    false,
                    cancellationToken);

            if (currentStudent is null)
                throw new NotFoundException(ErrorCodes.StudentFee.NotFound);

            var studentFee = await _unitOfWork.StudentFees
                .FirstOrDefaultAsync(
                    x => x.Id == id
                        && !x.IsDeleted
                        && x.Enrollment.StudentId == currentStudent.Id,
                    false,
                    cancellationToken,
                    x => x.Enrollment,
                    x => x.Enrollment.Student,
                    x => x.Enrollment.Group,
                    x => x.Enrollment.Group.Teacher);

            if (studentFee is null || studentFee.IsDeleted)
                throw new NotFoundException(ErrorCodes.StudentFee.NotFound);

            return _mapper.Map<StudentFeeResponse>(studentFee);
        }

        if (_currentUserService.IsTeacher)
        {
            var currentTeacherId = await ResolveCurrentTeacherIdAsync(cancellationToken);

            if (currentTeacherId is null)
            {
                throw new NotFoundException(ErrorCodes.StudentFee.NotFound);
            }

            var teacherFee = await _unitOfWork.StudentFees
                .FirstOrDefaultAsync(
                    x => x.Id == id
                        && !x.IsDeleted
                        && x.Enrollment.Group.TeacherId == currentTeacherId.Value,
                    false,
                    cancellationToken,
                    x => x.Enrollment,
                    x => x.Enrollment.Student,
                    x => x.Enrollment.Group,
                    x => x.Enrollment.Group.Teacher);

            if (teacherFee is null || teacherFee.IsDeleted)
                throw new NotFoundException(ErrorCodes.StudentFee.NotFound);

            return _mapper.Map<StudentFeeResponse>(teacherFee);
        }

        throw new ForbiddenException(ErrorCodes.User.NotFound);
    }

    public async Task<IEnumerable<StudentFeeResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var fees = await _unitOfWork.StudentFees
            .GetAllWithDetailsAsync(cancellationToken);

        return _mapper.Map<IEnumerable<StudentFeeResponse>>(fees);
    }

    public async Task<IEnumerable<StudentFeeResponse>> GetByStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        var fees = await _unitOfWork.StudentFees
            .GetByStudentAsync(
                studentId,
                cancellationToken);

        return _mapper.Map<IEnumerable<StudentFeeResponse>>(fees);
    }

    public async Task<IEnumerable<StudentFeeResponse>> GetByEnrollmentAsync(
        int enrollmentId,
        CancellationToken cancellationToken = default)
    {
        var fees = await _unitOfWork.StudentFees
            .GetByEnrollmentAsync(
                enrollmentId,
                cancellationToken);

        return _mapper.Map<IEnumerable<StudentFeeResponse>>(fees);
    }

    public async Task<StudentFeeResponse> UpdateAsync(
        int id,
        UpdateStudentFeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var fee = await _unitOfWork.StudentFees
            .FirstOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted,
                trackChanges: true,
                cancellationToken,
                includes: x => x.Payments);

        if (fee is null)
            throw new NotFoundException(ErrorCodes.StudentFee.NotFound);

        // Do not allow changing the amount
        // after payments have already been recorded.
        if (fee.Payments.Any() &&
            fee.TotalAmount != request.TotalAmount)
        {
            throw new BadRequestException(ErrorCodes.StudentFee.CannotChangeAmountWithPayments);
        }

        _mapper.Map(request, fee);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedFee = await _unitOfWork.StudentFees
            .GetWithDetailsAsync(
                id,
                cancellationToken);

        if (updatedFee is null)
            throw new NotFoundException(ErrorCodes.StudentFee.NotFound);

        return _mapper.Map<StudentFeeResponse>(updatedFee);
    }

    public async Task SoftDeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var fee = await _unitOfWork.StudentFees
            .FirstOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted,
                trackChanges: true,
                cancellationToken: cancellationToken);

        if (fee is null)
            throw new NotFoundException(ErrorCodes.StudentFee.NotFound);

        fee.IsDeleted = true;
        fee.DeletedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}