using AutoMapper;
using EduCore.Application.Common.Exceptions;
using EduCore.Application.Features.Teachers.DTOs;
using EduCore.Application.Features.Teachers.Interfaces;
using EduCore.Domain.Entities;
using EduCore.Domain.Identity;
using EduCore.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using EduCore.Application.Common;
using EduCore.Shared.Constants;
using EduCore.Application.Common.Interfaces;

namespace EduCore.Application.Features.Teachers.Services;

public class TeacherService : ITeacherService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public TeacherService(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<CreateTeacherResponse> CreateAsync(
        CreateTeacherRequest request,
        CancellationToken cancellationToken = default)
    {

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var nextSeq = await _unitOfWork.Students.GetNextStudentSequenceAsync(cancellationToken);
            var teacherCode = $"THR-{DateTime.UtcNow.Year}-{nextSeq:D4}";

            // 1. Create Identity user
            var user = new ApplicationUser
            {
                UserName = teacherCode,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsActive = true
            };

            var userResult = await _userManager.CreateAsync(
                user,
                request.Password);

            if (!userResult.Succeeded)
            {
                var firstErrorCode = userResult.Errors.First().Code;
                throw new BadRequestException($"IDENTITY_{firstErrorCode.ToUpper()}");
            }

            // 2. Assign Teacher role
            var roleResult = await _userManager.AddToRoleAsync(
                user,
                Roles.Teacher);

            if (!roleResult.Succeeded)
            {
                var firstErrorCode = userResult.Errors.First().Code;
                throw new BadRequestException($"IDENTITY_{firstErrorCode.ToUpper()}");
            }

            // 3. Create Teacher entity
            var teacher = _mapper.Map<Teacher>(request);

            teacher.UserId = user.Id;
            teacher.IsActive = true;

            await _unitOfWork.Teachers.AddAsync(
                teacher,
                cancellationToken);

            // 4. Save both User + Teacher
            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            await _unitOfWork.CommitTransactionAsync(
                cancellationToken);

            return new CreateTeacherResponse
            {
                TeacherId = teacher.Id,
                UserId = user.Id,
                UserName = user.UserName
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(
                cancellationToken);

            throw;
        }
    }

    public async Task<TeacherResponse> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.IsAdmin)
        {
            var adminTeacher = await _unitOfWork.Teachers
                .FirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDeleted,
                    false,
                    cancellationToken,
                    x => x.User);

            if (adminTeacher is null)
                throw new NotFoundException(ErrorCodes.Teacher.NotFound);

            return _mapper.Map<TeacherResponse>(adminTeacher);
        }

        if (!_currentUserService.IsAuthenticated ||
            _currentUserService.UserId == Guid.Empty)
        {
            throw new NotFoundException(ErrorCodes.Teacher.NotFound);
        }

        if (_currentUserService.IsStudent)
        {
            var student = await _unitOfWork.Students
                .FirstOrDefaultAsync(
                    x => x.UserId == _currentUserService.UserId
                        && !x.IsDeleted,
                    false,
                    cancellationToken);

            if (student is null)
                throw new NotFoundException(ErrorCodes.Teacher.NotFound);

            var teacher = await _unitOfWork.Teachers
                .FirstOrDefaultAsync(
                    x => x.Id == id
                        && !x.IsDeleted,
                    false,
                    cancellationToken,
                    x => x.User);

            if (teacher is null)
                throw new NotFoundException(ErrorCodes.Teacher.NotFound);

            var isStudentLinkedToTeacher =
                await _unitOfWork.Enrollments.ExistsAsync(
                    x => x.StudentId == student.Id
                        && x.Group.TeacherId == teacher.Id
                        && !x.IsDeleted
                        && !x.Group.IsDeleted,
                    cancellationToken);

            if (!isStudentLinkedToTeacher)
                throw new NotFoundException(ErrorCodes.Teacher.NotFound);

            return _mapper.Map<TeacherResponse>(teacher);
        }

        if (_currentUserService.IsTeacher)
        {
            var teacher = await _unitOfWork.Teachers
                .FirstOrDefaultAsync(
                    x => x.Id == id
                        && !x.IsDeleted
                        && x.UserId == _currentUserService.UserId,
                    false,
                    cancellationToken,
                    x => x.User);

            if (teacher is null)
                throw new NotFoundException(ErrorCodes.Teacher.NotFound);

            return _mapper.Map<TeacherResponse>(teacher);
        }

        throw new NotFoundException(ErrorCodes.Teacher.NotFound);
    }

    public async Task<IEnumerable<TeacherResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var teachers = await _unitOfWork.Teachers
            .FindAsync(
                x => !x.IsDeleted,
                false,
                cancellationToken,
                x => x.User);

        return _mapper.Map<IEnumerable<TeacherResponse>>(teachers);
    }

    public async Task<TeacherResponse> UpdateAsync(
        int id,
        UpdateTeacherRequest request,
        CancellationToken cancellationToken = default)
    {
        var teacher = await _unitOfWork.Teachers
            .FirstOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted,
                true,
                cancellationToken,
                x => x.User);

        if (teacher is null)
            throw new NotFoundException(ErrorCodes.Teacher.NotFound);

        // Update Teacher entity
        _mapper.Map(request, teacher);

        // Update Identity user
        if (teacher.User is not null)
        {
            teacher.User.FirstName = request.FirstName;
            teacher.User.LastName = request.LastName;
            teacher.Specialization = request.Specialization;
            teacher.HireDate = request.HireDate;
            teacher.User.Email = request.Email;
            teacher.User.PhoneNumber = request.PhoneNumber;

            var result = await _userManager.UpdateAsync(
                teacher.User);

            if (!result.Succeeded)
            {
                var firstErrorCode = result.Errors.First().Code;
                throw new BadRequestException($"IDENTITY_{firstErrorCode.ToUpper()}");
            }
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return _mapper.Map<TeacherResponse>(teacher);
    }

    public async Task DeactivateAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var teacher = await _unitOfWork.Teachers
            .FirstOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted,
                true,
                cancellationToken,
                x => x.User);

        if (teacher is null)
            throw new NotFoundException(ErrorCodes.Teacher.NotFound);

        teacher.IsActive = false;

        if (teacher.User is not null)
            teacher.User.IsActive = false;

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }

    public async Task ActivateAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var teacher = await _unitOfWork.Teachers
            .FirstOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted,
                true,
                cancellationToken,
                x => x.User);

        if (teacher is null)
            throw new NotFoundException(ErrorCodes.Teacher.NotFound);

        teacher.IsActive = true;

        if (teacher.User is not null)
            teacher.User.IsActive = true;

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}