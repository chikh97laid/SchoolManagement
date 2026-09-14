using AutoMapper;
using ClosedXML.Excel;
using EduCore.Application.Common;
using EduCore.Application.Common.Exceptions;
using EduCore.Application.Features.StudentManagement.DTOs;
using EduCore.Application.Features.StudentManagement.DTOs.Student;
using EduCore.Application.Features.StudentManagement.Interfaces;
using EduCore.Domain.Entities;
using EduCore.Domain.Entities.Enums;
using EduCore.Domain.Identity;
using EduCore.Domain.Interfaces.Repositories;
using EduCore.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Globalization;


namespace EduCore.Application.Features.StudentManagement.Services;

public class StudentService : IStudentService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly StudentImportSettings _settings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public StudentService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IOptions<StudentImportSettings> settings)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _settings = settings.Value;
    }

    public async Task<CreateStudentResponse> CreateStudentAsync(
    CreateStudentRequest request,
    CancellationToken cancellationToken = default)
    {
    
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {

            var nextSeq = await _unitOfWork.Students.GetNextStudentSequenceAsync(cancellationToken);
            var studentCode = $"STD-{DateTime.UtcNow.Year}-{nextSeq:D4}";

            var user = new ApplicationUser
            {
                UserName = studentCode,
                Email = request.Email?.Trim(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                IsActive = true
            };

            var userResult = await _userManager.CreateAsync(user, request.Password);

            if (!userResult.Succeeded)
            {
                var firstErrorCode = userResult.Errors.First().Code; 
                throw new BadRequestException($"IDENTITY_{firstErrorCode.ToUpper()}");
            }

            var roleResult = await _userManager.AddToRoleAsync(user, Roles.Student);
            if (!roleResult.Succeeded)
            {
                var firstErrorCode = roleResult.Errors.First().Code;
                throw new BadRequestException($"IDENTITY_{firstErrorCode.ToUpper()}");
            }

            var student = new Student
            {
                StudentCode = studentCode, 
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                GuardianName = request.GuardianName?.Trim(),
                GuardianPhone = request.GuardianPhone?.Trim(),
                BirthDate = request.BirthDate,
                Gender = request.Gender,
                UserId = user.Id,
                IsActive = true
            };


            await _unitOfWork.Students.AddAsync(student, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new CreateStudentResponse
            {
                StudentId = student.Id,
                UserId = user.Id,
                StudentCode = student.StudentCode,
                UserName = user.UserName!
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }


    public async Task<StudentResponse> GetStudentByIdAsync(
    int studentId,
    CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Students.FirstOrDefaultAsync(
            x => x.Id == studentId && !x.IsDeleted,
            false,
            cancellationToken,
            x => x.User);

        if (student is null)
            throw new NotFoundException(ErrorCodes.Student.NotFound);

        return _mapper.Map<StudentResponse>(student);
    }

    public async Task<IEnumerable<StudentResponse>> GetStudentsAsync(
    CancellationToken cancellationToken = default)
    {
        var students = await _unitOfWork.Students.FindAsync(
            x => !x.IsDeleted,
            false,
            cancellationToken,
            x => x.User);

        students = students.OrderBy(s => s.Id);

        return _mapper.Map<IEnumerable<StudentResponse>>(students);
    }


    public async Task<StudentResponse> UpdateStudentAsync(
    int studentId,
    UpdateStudentRequest request,
    CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Students.FirstOrDefaultAsync(
            x => x.Id == studentId && !x.IsDeleted,
            true,
            cancellationToken,
            x => x.User);

        if (student is null)
            throw new NotFoundException(ErrorCodes.Student.NotFound);

        _mapper.Map(request, student);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<StudentResponse>(student);
    }

    public async Task DeactivateStudentAccountAsync(
    int studentId,
    CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Students.FirstOrDefaultAsync(
            x => x.Id == studentId && !x.IsDeleted,
            true,
            cancellationToken,
            x => x.User);

        if (student is null)
            throw new NotFoundException(ErrorCodes.Student.NotFound);

        student.IsActive = false;

        if (student.User is not null)
        {
            student.User.IsActive = false;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }


    public async Task<ImportStudentsResponse> ImportStudentsFromExcelAsync(
     Stream fileStream,
     CancellationToken cancellationToken = default)
    {
        if (fileStream is null || fileStream.Length == 0)
            throw new BadRequestException(ErrorCodes.StudentImport.EmptyFile);

      
        var (validRows, readErrors, totalDataRows) = ReadExcelRows(fileStream);

        if (totalDataRows == 0)
            throw new BadRequestException(ErrorCodes.StudentImport.NoDataRows);

        
        var response = new ImportStudentsResponse
        {
            TotalRows = totalDataRows,
            FailedRows = readErrors.Count,
            SuccessfulRows = validRows.Count,
            Errors = readErrors
        };

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            
            foreach (var row in validRows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await ImportStudentRowAsync(row, _settings.DefaultPassword, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return response;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static (List<ImportStudentsRequest> ValidRows, List<StudentImportError> ReadErrors, int TotalDataRows) ReadExcelRows(Stream fileStream)
    {
        var validRows = new List<ImportStudentsRequest>();
        var readErrors = new List<StudentImportError>();

        using var workbook = new XLWorkbook(fileStream);
        var worksheet = workbook.Worksheets.FirstOrDefault();

        if (worksheet is null)
            return (validRows, readErrors, 0);

        var usedRows = worksheet.RowsUsed().ToList();
        if (usedRows.Count <= 1)
            return (validRows, readErrors, 0);

        int totalDataRows = usedRows.Count - 1;

        
        string[] allowedDateFormats = {
            "yyyy-MM-dd", "yyyy/MM/dd",
            "MM/dd/yyyy", "M/d/yyyy",
            "dd/MM/yyyy", "d/M/yyyy"
        };

        for (int i = 1; i < usedRows.Count; i++)
        {
            int rowNumber = i + 1;
            var row = usedRows[i];

            var firstName = row.Cell(2).GetString().Trim();
            var lastName = row.Cell(3).GetString().Trim();
            var guardianName = row.Cell(4).GetString().Trim();
            var guardianPhone = row.Cell(5).GetString().Trim();
            var genderText = row.Cell(7).GetString().Trim();

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                readErrors.Add(new StudentImportError
                {
                    RowNumber = rowNumber,
                    Error = ErrorCodes.StudentImport.NameRequired
                });
                continue;
            }

            // check validate date
            DateOnly? birthDate = null;
            var dateCell = row.Cell(6);

            // 1. read directly if the cell defined as a datetime
            if (dateCell.DataType == XLDataType.DateTime)
            {
                birthDate = DateOnly.FromDateTime(dateCell.GetDateTime());
            }
            else
            {
                var birthDateText = dateCell.GetString().Trim();
                if (!string.IsNullOrWhiteSpace(birthDateText))
                {
                    
                    if (DateOnly.TryParseExact(birthDateText, allowedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedExactDate))
                    {
                        birthDate = parsedExactDate;
                    }
                    else if (DateOnly.TryParse(birthDateText, CultureInfo.InvariantCulture, out var parsedFallbackDate))
                    {
                        birthDate = parsedFallbackDate;
                    }
                    else
                    {
                        readErrors.Add(new StudentImportError
                        {
                            RowNumber = rowNumber,
                            Error = ErrorCodes.StudentImport.InvalidBirthDate
                        });
                        continue;
                    }
                }
            }

            // check gender
            if (!Enum.TryParse<Gender>(genderText, true, out var gender))
            {
                readErrors.Add(new StudentImportError
                {
                    RowNumber = rowNumber,
                    Error = ErrorCodes.StudentImport.InvalidGender
                });
                continue;
            }

            validRows.Add(new ImportStudentsRequest
            {
                RowNumber = rowNumber,
                FirstName = firstName,
                LastName = lastName,
                GuardianName = string.IsNullOrWhiteSpace(guardianName) ? null : guardianName,
                GuardianPhone = string.IsNullOrWhiteSpace(guardianPhone) ? null : guardianPhone,
                BirthDate = birthDate,
                Gender = gender
            });
        }

        return (validRows, readErrors, totalDataRows);
    }

    private async Task ImportStudentRowAsync(
    ImportStudentsRequest row,
    string defaultPassword,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(defaultPassword))
            throw new InvalidOperationException(ErrorCodes.StudentImport.DefaultPasswordNotConfigured);

        // ==========================================
        // 1. Check if the student already exists
        // ==========================================

        var studentExists =
            await _unitOfWork.Students.ExistsAsync(
                x =>
                    x.FirstName == row.FirstName &&
                    x.LastName == row.LastName &&
                    x.GuardianPhone == row.GuardianPhone &&
                    !x.IsDeleted,
                cancellationToken);

        if (studentExists)
        {
            throw new BadRequestException(ErrorCodes.StudentImport.AlreadyExists);
        }

        // ==========================================
        // 5. Create Identity User
        // ==========================================

        var nextSeq = await _unitOfWork.Students.GetNextStudentSequenceAsync(cancellationToken);
        var studentCode = $"STD-{DateTime.UtcNow.Year}-{nextSeq:D4}";

        var user = new ApplicationUser
        {
            UserName = studentCode,

            FirstName = row.FirstName,

            LastName = row.LastName,

            IsActive = true
        };

        var userResult =
            await _userManager.CreateAsync(
                user,
                defaultPassword);

        if (!userResult.Succeeded)
        {
            // take the first error code and throw a BadRequestException with a formatted message
            var firstErrorCode = userResult.Errors.First().Code;
            throw new BadRequestException($"IDENTITY_{firstErrorCode.ToUpper()}");
        }


        var roleResult = await _userManager.AddToRoleAsync(user, Roles.Student);
        if (!roleResult.Succeeded)
        {
            var firstErrorCode = roleResult.Errors.First().Code;
            throw new BadRequestException($"IDENTITY_{firstErrorCode.ToUpper()}");
        }

        // ==========================================
        // 2. Create Student
        // ==========================================


        var student = new Student
        {
            StudentCode = studentCode,
            FirstName = row.FirstName,
            LastName = row.LastName,
            GuardianName = row.GuardianName,
            GuardianPhone = row.GuardianPhone,
            BirthDate = row.BirthDate,
            Gender = row.Gender,
            IsActive = true,
            UserId = user.Id
        };

        await _unitOfWork.Students.AddAsync(
            student,
            cancellationToken);

    }

}