using AutoMapper;
using ClosedXML.Excel;
using EduCore.Application.Common;
using EduCore.Application.Common.Exceptions;
using EduCore.Application.Common.Interfaces;
using EduCore.Application.Features.StudentManagement.DTOs.Student;
using EduCore.Application.Features.StudentManagement.Services;
using EduCore.Application.Features.StudentManagement.Validators;
using EduCore.Domain.Entities;
using EduCore.Domain.Entities.Enums;
using EduCore.Domain.Identity;
using EduCore.Domain.Interfaces.Repositories;
using EduCore.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Linq.Expressions;

namespace EduCore.UnitTests.Services;

public class StudentImportTests
{
    [Fact]
    public async Task ImportStudentsFromExcelAsync_RejectsEmptyFile()
    {
        using var stream = new MemoryStream();
        var harness = CreateHarness();

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => harness.Service.ImportStudentsFromExcelAsync(stream));

        Assert.Equal(ErrorCodes.StudentImport.EmptyFile, exception.Message);
    }

    [Fact]
    public async Task ImportStudentsFromExcelAsync_RejectsFileLargerThanLimit()
    {
        using var stream = new MemoryStream(
            new byte[StudentImportSettings.MaxFileSizeBytes + 1]);
        var harness = CreateHarness();

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => harness.Service.ImportStudentsFromExcelAsync(stream));

        Assert.Equal(ErrorCodes.StudentImport.FileTooLarge, exception.Message);
    }

    [Fact]
    public async Task ImportStudentsFromExcelAsync_RejectsTooManyRows()
    {
        var rows = Enumerable.Range(0, StudentImportSettings.MaxRows + 1)
            .Select(index => ValidRow($"First{index}", $"Last{index}", $"05{index:D8}", $"2000-01-{index % 28 + 1:D2}"));
        using var stream = CreateWorkbook(rows);
        var harness = CreateHarness();

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => harness.Service.ImportStudentsFromExcelAsync(stream));

        Assert.Equal(ErrorCodes.StudentImport.TooManyRows, exception.Message);
        harness.UnitOfWork.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ImportStudentsFromExcelAsync_ReportsInvalidGender()
    {
        using var stream = CreateWorkbook([ValidRow(gender: "999")]);
        var harness = CreateHarness();

        var response = await harness.Service.ImportStudentsFromExcelAsync(stream);

        Assert.Equal(0, response.SuccessfulRows);
        Assert.Equal(1, response.FailedRows);
        Assert.Contains(response.Errors, error => error.Error == ErrorCodes.Student.GenderInvalid);
    }

    [Fact]
    public async Task ImportStudentsFromExcelAsync_ReportsInvalidPhoneNumber()
    {
        using var stream = CreateWorkbook([ValidRow(guardianPhone: "not-a-phone")]);
        var harness = CreateHarness();

        var response = await harness.Service.ImportStudentsFromExcelAsync(stream);

        Assert.Equal(0, response.SuccessfulRows);
        Assert.Equal(1, response.FailedRows);
        Assert.Contains(response.Errors, error => error.Error == ErrorCodes.Student.GuardianPhoneInvalid);
    }

    [Fact]
    public async Task ImportStudentsFromExcelAsync_ReportsInvalidDate()
    {
        using var stream = CreateWorkbook([ValidRow()], firstBirthDateOverride: "not-a-date");
        var harness = CreateHarness();

        var response = await harness.Service.ImportStudentsFromExcelAsync(stream);

        Assert.Equal(0, response.SuccessfulRows);
        Assert.Equal(1, response.FailedRows);
        Assert.Contains(response.Errors, error => error.Error == ErrorCodes.StudentImport.InvalidBirthDate);
    }

    [Fact]
    public async Task ImportStudentsFromExcelAsync_RejectsInvalidHeaders()
    {
        using var stream = CreateWorkbook([ValidRow()], firstHeader: "WrongColumn");
        var harness = CreateHarness();

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => harness.Service.ImportStudentsFromExcelAsync(stream));

        Assert.Equal(ErrorCodes.StudentImport.InvalidHeaders, exception.Message);
    }

    [Fact]
    public async Task ImportStudentsFromExcelAsync_RejectsDuplicateStudentAlreadyInDatabase()
    {
        var duplicate = ValidRow();
        using var stream = CreateWorkbook([duplicate]);
        var harness = CreateHarness([ExistingStudent(duplicate)]);

        var response = await harness.Service.ImportStudentsFromExcelAsync(stream);

        Assert.Equal(1, response.TotalRows);
        Assert.Equal(0, response.SuccessfulRows);
        Assert.Equal(1, response.FailedRows);
        Assert.Contains(response.Errors, error =>
            error.RowNumber == 2 && error.Error == ErrorCodes.StudentImport.AlreadyExists);
        harness.UnitOfWork.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ImportStudentsFromExcelAsync_MixedBatchImportsUniqueRowsAndReportsDuplicates()
    {
        var duplicate = ValidRow("Mina", "Hassan", "0551234567", "2012-03-04");
        var unique = ValidRow("Sara", "Omar", "0569876543", "2014-07-08");
        using var stream = CreateWorkbook([duplicate, unique]);
        var harness = CreateHarness([ExistingStudent(duplicate)]);

        var response = await harness.Service.ImportStudentsFromExcelAsync(stream);

        Assert.Equal(2, response.TotalRows);
        Assert.Equal(1, response.SuccessfulRows);
        Assert.Equal(1, response.FailedRows);
        Assert.Contains(response.Errors, error =>
            error.RowNumber == 2 && error.Error == ErrorCodes.StudentImport.AlreadyExists);
        harness.StudentRepository.Verify(
            x => x.AddAsync(It.Is<Student>(student => student.UserId != Guid.Empty), It.IsAny<CancellationToken>()),
            Times.Once);
        harness.UnitOfWork.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ImportStudentsFromExcelAsync_FirstOccurrenceWinsForDuplicatesWithinFile()
    {
        var row = ValidRow("Mina", "Hassan", "0551234567", "2012-03-04");
        using var stream = CreateWorkbook([row, row]);
        var harness = CreateHarness();

        var response = await harness.Service.ImportStudentsFromExcelAsync(stream);

        Assert.Equal(2, response.TotalRows);
        Assert.Equal(1, response.SuccessfulRows);
        Assert.Equal(1, response.FailedRows);
        Assert.Contains(response.Errors, error =>
            error.RowNumber == 3 && error.Error == ErrorCodes.StudentImport.AlreadyExists);
        harness.StudentRepository.Verify(
            x => x.AddAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ImportStudentsFromExcelAsync_PersistsSuccessfulImport()
    {
        using var stream = CreateWorkbook([ValidRow()]);
        var harness = CreateHarness();

        var response = await harness.Service.ImportStudentsFromExcelAsync(stream);

        Assert.Equal(1, response.TotalRows);
        Assert.Equal(1, response.SuccessfulRows);
        Assert.Equal(0, response.FailedRows);
        harness.UserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Temporary-123"), Times.Once);
        harness.UserManager.Verify(
            x => x.CreateAsync(It.Is<ApplicationUser>(user => user.MustChangePassword), "Temporary-123"),
            Times.Once);
        harness.UserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), Roles.Student), Times.Once);
        harness.StudentRepository.Verify(x => x.AddAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Once);
        harness.UnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        harness.UnitOfWork.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        harness.UnitOfWork.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ImportStudentsFromExcelAsync_IdentityFailureRollsBackWholeBatch()
    {
        using var stream = CreateWorkbook([ValidRow()]);
        var harness = CreateHarness();
        harness.UserManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "CreateFailed" }));

        await Assert.ThrowsAsync<BadRequestException>(
            () => harness.Service.ImportStudentsFromExcelAsync(stream));

        harness.UnitOfWork.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        harness.UnitOfWork.Verify(x => x.RollbackTransactionAsync(CancellationToken.None), Times.Once);
        harness.UnitOfWork.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        harness.StudentRepository.Verify(x => x.AddAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ImportStudentsFromExcelAsync_CancellationRollsBackWholeBatch()
    {
        using var stream = CreateWorkbook([ValidRow()]);
        using var cancellation = new CancellationTokenSource();
        var harness = CreateHarness();
        harness.UserManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Returns(() =>
            {
                cancellation.Cancel();
                return Task.FromResult(IdentityResult.Success);
            });
        harness.UnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns<CancellationToken>(token => Task.FromCanceled<int>(token));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => harness.Service.ImportStudentsFromExcelAsync(stream, cancellation.Token));

        harness.UnitOfWork.Verify(x => x.RollbackTransactionAsync(CancellationToken.None), Times.Once);
        harness.UnitOfWork.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ImportStudentsFromExcelAsync_RejectsMalformedExcelFile()
    {
        using var stream = new MemoryStream([0x01, 0x02, 0x03, 0x04]);
        var harness = CreateHarness();

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => harness.Service.ImportStudentsFromExcelAsync(stream));

        Assert.Equal(ErrorCodes.StudentImport.InvalidExcelFile, exception.Message);
    }

    private static TestHarness CreateHarness(IEnumerable<Student>? existingStudents = null)
    {
        var userStore = Mock.Of<IUserStore<ApplicationUser>>();
        var userManager = new Mock<UserManager<ApplicationUser>>(
            userStore,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            NullLogger<UserManager<ApplicationUser>>.Instance);

        userManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>((user, _) => user.Id = Guid.NewGuid());
        userManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), Roles.Student))
            .ReturnsAsync(IdentityResult.Success);

        var studentRepository = new Mock<IStudentRepository>();
        studentRepository
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Student, bool>>>(),
                false,
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Student, object?>>[]>()))
            .ReturnsAsync(existingStudents ?? []);
        studentRepository
            .Setup(x => x.GetNextStudentSequenceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        studentRepository
            .Setup(x => x.AddAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(x => x.Students).Returns(studentRepository.Object);
        unitOfWork.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        unitOfWork.Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        unitOfWork.Setup(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new StudentService(
            userManager.Object,
            unitOfWork.Object,
            Mock.Of<IMapper>(),
            Options.Create(new StudentImportSettings { DefaultPassword = "Temporary-123" }),
            Mock.Of<ICurrentUserService>(),
            new ImportStudentsRequestValidator());

        return new TestHarness(service, unitOfWork, userManager, studentRepository);
    }

    private static Student ExistingStudent(ImportStudentsRequest row)
    {
        return new Student
        {
            StudentCode = "EXISTING-STUDENT",
            GuardianPhone = row.GuardianPhone,
            BirthDate = row.BirthDate,
            Gender = row.Gender,
            User = new ApplicationUser
            {
                FirstName = $" {row.FirstName.ToLowerInvariant()} ",
                LastName = row.LastName.ToUpperInvariant()
            }
        };
    }

    private static ImportStudentsRequest ValidRow(
        string firstName = "Mina",
        string lastName = "Hassan",
        string guardianPhone = "0551234567",
        string birthDate = "2012-03-04",
        string gender = "Female")
    {
        return new ImportStudentsRequest
        {
            FirstName = firstName,
            LastName = lastName,
            GuardianPhone = guardianPhone,
            BirthDate = DateOnly.ParseExact(birthDate, "yyyy-MM-dd"),
            Gender = Enum.Parse<Gender>(gender)
        };
    }

    private static MemoryStream CreateWorkbook(
        IEnumerable<ImportStudentsRequest> rows,
        string firstHeader = "ID",
        string? firstBirthDateOverride = null)
    {
        var stream = new MemoryStream();
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.AddWorksheet("Students");
            SetHeader(worksheet, firstHeader);
            var rowNumber = 2;
            foreach (var row in rows)
            {
                worksheet.Cell(rowNumber, 2).Value = row.FirstName;
                worksheet.Cell(rowNumber, 3).Value = row.LastName;
                worksheet.Cell(rowNumber, 4).Value = "Guardian";
                worksheet.Cell(rowNumber, 5).Value = row.GuardianPhone ?? string.Empty;
                worksheet.Cell(rowNumber, 6).Value = rowNumber == 2 && firstBirthDateOverride is not null
                    ? firstBirthDateOverride
                    : row.BirthDate?.ToString("yyyy-MM-dd") ?? string.Empty;
                worksheet.Cell(rowNumber, 7).Value = row.Gender.ToString();
                rowNumber++;
            }

            workbook.SaveAs(stream);
        }

        stream.Position = 0;
        return stream;
    }

    private static void SetHeader(IXLWorksheet worksheet, string firstHeader)
    {
        var headers = new[]
        {
            firstHeader,
            "FirstName",
            "LastName",
            "GuardianName",
            "GuardianPhone",
            "BirthDate",
            "Gender"
        };

        for (var column = 0; column < headers.Length; column++)
            worksheet.Cell(1, column + 1).Value = headers[column];
    }

    private sealed record TestHarness(
        StudentService Service,
        Mock<IUnitOfWork> UnitOfWork,
        Mock<UserManager<ApplicationUser>> UserManager,
        Mock<IStudentRepository> StudentRepository);
}
