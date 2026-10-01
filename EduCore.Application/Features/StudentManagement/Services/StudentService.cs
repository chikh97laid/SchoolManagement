using AutoMapper;
using ClosedXML.Excel;
using EduCore.Application.Common;
using EduCore.Application.Common.Exceptions;
using EduCore.Application.Common.Interfaces;
using EduCore.Application.Common.Pagination;
using EduCore.Application.Features.StudentManagement.DTOs;
using EduCore.Application.Features.StudentManagement.DTOs.Student;
using EduCore.Application.Features.StudentManagement.Interfaces;
using EduCore.Domain.Entities;
using EduCore.Domain.Entities.Enums;
using EduCore.Domain.Identity;
using EduCore.Domain.Interfaces.Repositories;
using EduCore.Shared.Constants;
using FluentValidation;
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
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<ImportStudentsRequest> _importStudentsValidator;

    public StudentService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IOptions<StudentImportSettings> settings,
        ICurrentUserService currentUserService,
        IValidator<ImportStudentsRequest> importStudentsValidator)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _settings = settings.Value;
        _currentUserService = currentUserService;
        _importStudentsValidator = importStudentsValidator;
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
        if (_currentUserService.IsAdmin)
        {
            var adminStudent = await _unitOfWork.Students.FirstOrDefaultAsync(
                x => x.Id == studentId,
                false,
                cancellationToken,
                x => x.User);

            if (adminStudent is null)
                throw new NotFoundException(ErrorCodes.Student.NotFound);

            return _mapper.Map<StudentResponse>(adminStudent);
        }


        if (_currentUserService.IsStudent)
        {
            var currentStudent = await _unitOfWork.Students.FirstOrDefaultAsync(
                x => x.UserId == _currentUserService.UserId,
                false,
                cancellationToken,
                x => x.User);

            if (currentStudent is null || currentStudent.Id != studentId)
                throw new NotFoundException(ErrorCodes.Student.NotFound);

            return _mapper.Map<StudentResponse>(currentStudent);
        }

        if (_currentUserService.IsTeacher)
        {
            var teacher = await _unitOfWork.Teachers.FirstOrDefaultAsync(
                x => x.UserId == _currentUserService.UserId,
                false,
                cancellationToken);

            if (teacher is null)
                throw new NotFoundException(ErrorCodes.Student.NotFound);

            var student = await _unitOfWork.Students.FirstOrDefaultAsync(
                x => x.Id == studentId,
                false,
                cancellationToken,
                x => x.User);

            if (student is null)
                throw new NotFoundException(ErrorCodes.Student.NotFound);

            var isStudentInTeacherScope = await _unitOfWork.Enrollments.ExistsAsync(
                x => x.StudentId == student.Id
                    && x.Group.TeacherId == teacher.Id,
                cancellationToken);

            if (!isStudentInTeacherScope)
                throw new NotFoundException(ErrorCodes.Student.NotFound);

            return _mapper.Map<StudentResponse>(student);
        }

        throw new NotFoundException(ErrorCodes.Student.NotFound);
    }

    public async Task<PagedResult<StudentResponse>> GetStudentsAsync(
        PaginationParameters parameters,
        CancellationToken cancellationToken = default)
    {
        if (_currentUserService.IsAdmin)
        {
            var (students, totalCount) = await _unitOfWork.Students.GetPagedAsync(
                x => true,
                x => x.Id,
                parameters.PageNumber,
                parameters.PageSize,
                false,
                cancellationToken,
                x => x.User);

            return new PagedResult<StudentResponse>
            {
                Items = _mapper.Map<List<StudentResponse>>(students),
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalCount = totalCount
            };
        }

        if (_currentUserService.IsTeacher)
        {
            var teacher = await _unitOfWork.Teachers.FirstOrDefaultAsync(
                x => x.UserId == _currentUserService.UserId,
                false,
                cancellationToken);

            if (teacher is null)
                return new PagedResult<StudentResponse>
                {
                    PageNumber = parameters.PageNumber,
                    PageSize = parameters.PageSize
                };

            var (students, totalCount) = await _unitOfWork.Students.GetPagedAsync(
                x => x.Enrollments.Any(e => e.Group.TeacherId == teacher.Id),
                x => x.Id,
                parameters.PageNumber,
                parameters.PageSize,
                false,
                cancellationToken,
                x => x.User);

            return new PagedResult<StudentResponse>
            {
                Items = _mapper.Map<List<StudentResponse>>(students),
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalCount = totalCount
            };
        }

        if (_currentUserService.IsStudent)
            throw new NotFoundException(ErrorCodes.Student.NotFound);

        throw new NotFoundException(ErrorCodes.Student.NotFound);
    }


    public async Task<StudentResponse> UpdateStudentAsync(
    int studentId,
    UpdateStudentRequest request,
    CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Students.FirstOrDefaultAsync(
            x => x.Id == studentId,
            true,
            cancellationToken,
            x => x.User);

        if (student is null)
            throw new NotFoundException(ErrorCodes.Student.NotFound);

        _mapper.Map(request, student);

        if (student.User is not null)
        {
            student.User.FirstName = request.FirstName;
            student.User.LastName = request.LastName;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<StudentResponse>(student);
    }

    public async Task DeactivateStudentAccountAsync(
    int studentId,
    CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Students.FirstOrDefaultAsync(
            x => x.Id == studentId,
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

    public async Task SoftDeleteStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Students.FirstOrDefaultAsync(
            x => x.Id == studentId,
            trackChanges: true,
            cancellationToken: cancellationToken,
            x => x.User);

        if (student is null)
            throw new NotFoundException(ErrorCodes.Student.NotFound);

        student.IsActive = false;
        student.IsDeleted = true;
        student.DeletedAt = DateTime.UtcNow;
        student.DeletedBy = _currentUserService.IsAuthenticated
            ? _currentUserService.UserId
            : null;

        if (student.User is not null)
            student.User.IsActive = false;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }


    public async Task<ImportStudentsResponse> ImportStudentsFromExcelAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        if (fileStream is null || !fileStream.CanRead)
            throw new BadRequestException(ErrorCodes.StudentImport.EmptyFile);

        MemoryStream? bufferedStream = null;
        var workbookStream = fileStream;
        if (fileStream.CanSeek)
        {
            var remainingLength = fileStream.Length - fileStream.Position;
            if (remainingLength > StudentImportSettings.MaxFileSizeBytes)
                throw new BadRequestException(ErrorCodes.StudentImport.FileTooLarge);

            if (remainingLength <= 0)
                throw new BadRequestException(ErrorCodes.StudentImport.EmptyFile);
        }
        else
        {
            bufferedStream = await BufferWithinLimitAsync(
                fileStream,
                cancellationToken);
            workbookStream = bufferedStream;
        }

        List<ImportStudentsRequest> parsedRows;
        List<StudentImportError> rowErrors;
        int totalDataRows;
        try
        {
            (parsedRows, rowErrors, totalDataRows) = ReadExcelRows(workbookStream);
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is not OutOfMemoryException &&
            exception is not OperationCanceledException)
        {
            throw new BadRequestException(ErrorCodes.StudentImport.InvalidExcelFile);
        }
        finally
        {
            bufferedStream?.Dispose();
        }

        if (totalDataRows == 0)
            throw new BadRequestException(ErrorCodes.StudentImport.NoDataRows);

        var validRows = new List<ImportStudentsRequest>();
        foreach (var row in parsedRows)
        {
            var validation = await _importStudentsValidator.ValidateAsync(
                row,
                cancellationToken);

            if (validation.IsValid)
            {
                validRows.Add(row);
                continue;
            }

            rowErrors.AddRange(validation.Errors.Select(error => new StudentImportError
            {
                RowNumber = row.RowNumber,
                Error = string.IsNullOrWhiteSpace(error.ErrorCode)
                    ? error.ErrorMessage
                    : error.ErrorCode
            }));
        }

        var uniqueRows = await RemoveDuplicateImportRowsAsync(
            validRows,
            rowErrors,
            cancellationToken);

        var response = new ImportStudentsResponse
        {
            TotalRows = totalDataRows,
            FailedRows = rowErrors.Select(error => error.RowNumber).Distinct().Count(),
            SuccessfulRows = uniqueRows.Count,
            Errors = rowErrors
        };

        if (uniqueRows.Count == 0)
            return response;

        if (string.IsNullOrWhiteSpace(_settings.DefaultPassword))
            throw new InvalidOperationException(ErrorCodes.StudentImport.DefaultPasswordNotConfigured);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var row in uniqueRows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await ImportStudentRowAsync(row, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return response;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    private static async Task<MemoryStream> BufferWithinLimitAsync(
        Stream input,
        CancellationToken cancellationToken)
    {
        var output = new MemoryStream();
        var buffer = new byte[81920];

        try
        {
            int bytesRead;
            while ((bytesRead = await input.ReadAsync(buffer.AsMemory(), cancellationToken)) > 0)
            {
                if (output.Length + bytesRead > StudentImportSettings.MaxFileSizeBytes)
                    throw new BadRequestException(ErrorCodes.StudentImport.FileTooLarge);

                await output.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            }

            if (output.Length == 0)
                throw new BadRequestException(ErrorCodes.StudentImport.EmptyFile);

            output.Position = 0;
            return output;
        }
        catch
        {
            await output.DisposeAsync();
            throw;
        }
    }

    private async Task<List<ImportStudentsRequest>> RemoveDuplicateImportRowsAsync(
        IReadOnlyCollection<ImportStudentsRequest> rows,
        List<StudentImportError> errors,
        CancellationToken cancellationToken)
    {
        var candidatePhones = rows
            .Select(row => NormalizePhone(row.GuardianPhone))
            .Where(phone => phone is not null)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var candidateBirthDates = rows
            .Where(row => row.BirthDate.HasValue)
            .Select(row => row.BirthDate!.Value)
            .Distinct()
            .ToArray();

        IEnumerable<Student> existingStudents = [];
        if (candidatePhones.Length > 0 || candidateBirthDates.Length > 0)
        {
            existingStudents = await _unitOfWork.Students.FindAsync(
                student =>
                    (student.GuardianPhone != null && candidatePhones.Contains(student.GuardianPhone)) ||
                    (student.BirthDate.HasValue && candidateBirthDates.Contains(student.BirthDate.Value)),
                trackChanges: false,
                cancellationToken: cancellationToken,
                student => student.User);
        }

        var existingKeys = new HashSet<StudentImportDuplicateKey>();
        foreach (var student in existingStudents)
        {
            if (student.User is null)
                continue;

            var key = CreateDuplicateKey(
                student.User.FirstName,
                student.User.LastName,
                student.GuardianPhone,
                student.BirthDate,
                student.Gender);

            if (key.HasDiscriminator)
                existingKeys.Add(key);
        }

        var acceptedRows = new List<ImportStudentsRequest>(rows.Count);
        foreach (var row in rows)
        {
            var key = CreateDuplicateKey(
                row.FirstName,
                row.LastName,
                row.GuardianPhone,
                row.BirthDate,
                row.Gender);

            if (key.HasDiscriminator && !existingKeys.Add(key))
            {
                errors.Add(new StudentImportError
                {
                    RowNumber = row.RowNumber,
                    Error = ErrorCodes.StudentImport.AlreadyExists
                });
                continue;
            }

            acceptedRows.Add(row);
        }

        return acceptedRows;
    }

    private static StudentImportDuplicateKey CreateDuplicateKey(
        string firstName,
        string lastName,
        string? guardianPhone,
        DateOnly? birthDate,
        Gender gender)
    {
        return new StudentImportDuplicateKey(
            NormalizeName(firstName),
            NormalizeName(lastName),
            NormalizePhone(guardianPhone),
            birthDate,
            gender);
    }

    private static string NormalizeName(string value)
    {
        return string.Join(' ', value.Trim().Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();
    }

    private static string? NormalizePhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length == 0 ? null : digits;
    }

    private readonly record struct StudentImportDuplicateKey(
        string FirstName,
        string LastName,
        string? GuardianPhone,
        DateOnly? BirthDate,
        Gender Gender)
    {
        public bool HasDiscriminator => GuardianPhone is not null || BirthDate.HasValue;
    }

    private static (List<ImportStudentsRequest> ValidRows, List<StudentImportError> ReadErrors, int TotalDataRows) ReadExcelRows(Stream fileStream)
    {
        var validRows = new List<ImportStudentsRequest>();
        var readErrors = new List<StudentImportError>();

        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(fileStream);
        }
        catch (Exception exception) when (
            exception is not OutOfMemoryException &&
            exception is not OperationCanceledException)
        {
            throw new BadRequestException(ErrorCodes.StudentImport.InvalidExcelFile);
        }

        using var workbookToDispose = workbook;
        var worksheet = workbook.Worksheets.FirstOrDefault();

        if (worksheet is null)
            return (validRows, readErrors, 0);

        var usedRows = worksheet.RowsUsed().ToList();
        if (usedRows.Count <= 1)
            return (validRows, readErrors, 0);

        int totalDataRows = usedRows.Count - 1;
        if (totalDataRows > StudentImportSettings.MaxRows)
            throw new BadRequestException(ErrorCodes.StudentImport.TooManyRows);

        string[] allowedDateFormats = {
            "yyyy-MM-dd", "yyyy/MM/dd",
            "MM/dd/yyyy", "M/d/yyyy",
            "dd/MM/yyyy", "d/M/yyyy"
        };

        var headerRow = usedRows[0];

        ValidateHeaders(headerRow);

        for (int i = 1; i < usedRows.Count; i++)
        {
            var row = usedRows[i];
            var rowNumber = row.RowNumber();

            var firstName = row.Cell(2).GetString().Trim();
            var lastName = row.Cell(3).GetString().Trim();
            var guardianName = row.Cell(4).GetString().Trim();
            var guardianPhone = row.Cell(5).GetString().Trim();
            var genderText = row.Cell(7).GetString().Trim();

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
            var gender = Enum.TryParse<Gender>(genderText, true, out var parsedGender) &&
                         Enum.IsDefined(parsedGender)
                ? parsedGender
                : (Gender)(-1);

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
        CancellationToken cancellationToken)
    {
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
                _settings.DefaultPassword);

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

    private static void ValidateHeaders(IXLRow headerRow)
    {
        var expectedHeaders = new[]
        {
            "ID",
            "FirstName",
            "LastName",
            "GuardianName",
            "GuardianPhone",
            "BirthDate",
            "Gender"
        };

        for (int i = 0; i < expectedHeaders.Length; i++)
        {
            var actualHeader = headerRow
                .Cell(i + 1)
                .GetString()
                .Trim();

            if (!string.Equals(
                    actualHeader,
                    expectedHeaders[i],
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException(
                    ErrorCodes.StudentImport.InvalidHeaders);
            }
        }
    }


}