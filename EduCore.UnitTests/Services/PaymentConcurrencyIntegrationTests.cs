using AutoMapper;
using EduCore.Application.Common;
using EduCore.Application.Common.Exceptions;
using EduCore.Application.Common.Interfaces;
using EduCore.Application.Features.Payments.DTOs;
using EduCore.Application.Features.Payments.Mapping;
using EduCore.Application.Features.Payments.Services;
using EduCore.Domain.Entities;
using EduCore.Domain.Entities.Enums;
using EduCore.Persistence.Context;
using EduCore.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Xunit;

namespace EduCore.UnitTests.Services;

public class PaymentConcurrencyIntegrationTests
{
    [Fact]
    public async Task CreateAsync_ConcurrentPayments_ShouldLockFeeAndRejectSecondOverpayment()
    {
        var feeId = 0;
        var enrollmentId = 0;
        var groupId = 0;
        var studentId = 0;
        var teacherId = 0;
        Task<PaymentResponse>? firstTask = null;
        Task<PaymentResponse>? secondTask = null;

        try
        {
            await using var setupContext = CreateContext();
            await setupContext.Database.MigrateAsync();

            var suffix = Guid.NewGuid().ToString("N")[..12];
            var teacher = new Teacher
            {
                FirstName = "Concurrency",
                LastName = suffix
            };
            var group = new Group
            {
                Name = $"Concurrency-{suffix}",
                StandardFee = 3000,
                Teacher = teacher
            };
            var student = new Student
            {
                StudentCode = $"CONC-{suffix}",
                FirstName = "Concurrency",
                LastName = suffix,
                Gender = Gender.Male
            };
            var enrollment = new Enrollment
            {
                Student = student,
                Group = group
            };
            var fee = new StudentFee
            {
                Enrollment = enrollment,
                Title = "Concurrency fee",
                TotalAmount = 3000,
                PaidAmount = 1000,
                Status = StudentFeeStatus.PartiallyPaid
            };

            setupContext.StudentFees.Add(fee);
            await setupContext.SaveChangesAsync();

            feeId = fee.Id;
            enrollmentId = enrollment.Id;
            groupId = group.Id;
            studentId = student.Id;
            teacherId = teacher.Id;

            setupContext.Payments.Add(new Payment
            {
                StudentFeeId = fee.Id,
                Amount = 1000,
                PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                PaymentMethod = PaymentMethod.Cash,
                Status = PaymentStatus.Completed
            });
            await setupContext.SaveChangesAsync();

            var existingPaymentId = setupContext.Payments
                .Where(x => x.StudentFeeId == feeId && x.Amount == 1000)
                .Select(x => x.Id)
                .Single();

            await using var blockingContext = CreateContext();
            await using var blockingTransaction =
                await blockingContext.Database.BeginTransactionAsync();

            await blockingContext.StudentFees
                .FromSqlInterpolated($"""
                    SELECT *
                    FROM "StudentFees"
                    WHERE "Id" = {feeId}
                    FOR UPDATE
                    """)
                .AsNoTracking()
                .SingleAsync();

            var firstStarted = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var secondStarted = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            var firstApplicationName = $"EduCorePaymentTest-{suffix}-1";
            var secondApplicationName = $"EduCorePaymentTest-{suffix}-2";
            firstTask = RunCreateAsync(
                feeId,
                firstStarted,
                firstApplicationName,
                CancellationToken.None);
            secondTask = RunCreateAsync(
                feeId,
                secondStarted,
                secondApplicationName,
                CancellationToken.None);

            await Task.WhenAll(
                firstStarted.Task.WaitAsync(TimeSpan.FromSeconds(10)),
                secondStarted.Task.WaitAsync(TimeSpan.FromSeconds(10)));
            await AssertWorkersWaitingForLockAsync(
                firstApplicationName,
                secondApplicationName);

            await blockingTransaction.CommitAsync();

            var results = await Task.WhenAll(
                    CaptureResultAsync(firstTask),
                    CaptureResultAsync(secondTask))
                .WaitAsync(TimeSpan.FromSeconds(30));

            Assert.Equal(1, results.Count(x => x.Succeeded));
            var failedResult = Assert.Single(results.Where(x => !x.Succeeded));
            var exception = Assert.IsType<BadRequestException>(failedResult.Exception);
            Assert.Equal(ErrorCodes.Payment.InvalidAmount, exception.Message);

            await using var verificationContext = CreateContext();
            var finalFee = await verificationContext.StudentFees
                .SingleAsync(x => x.Id == feeId);
            var payments = await verificationContext.Payments
                .Where(x => x.StudentFeeId == feeId && !x.IsDeleted)
                .ToListAsync();

            Assert.Equal(2500, finalFee.PaidAmount);
            Assert.Equal(StudentFeeStatus.PartiallyPaid, finalFee.Status);
            Assert.Equal(2, payments.Count);
            Assert.Equal(finalFee.PaidAmount, payments.Sum(x => x.Amount));
            Assert.Contains(payments, x => x.Id == existingPaymentId && x.Amount == 1000);
            Assert.Single(payments, x => x.Amount == 1500);
        }
        finally
        {
            if (firstTask is not null)
                await CaptureResultAsync(firstTask);

            if (secondTask is not null)
                await CaptureResultAsync(secondTask);

            await CleanupAsync(
                feeId,
                enrollmentId,
                groupId,
                studentId,
                teacherId);
        }
    }

    private async Task<PaymentResponse> RunCreateAsync(
        int feeId,
        TaskCompletionSource<bool> started,
        string applicationName,
        CancellationToken cancellationToken)
    {
        await using var context = CreateContext(applicationName);
        var unitOfWork = new UnitOfWork(context);
        var mapper = new Mapper(new MapperConfiguration(
            configuration => configuration.AddProfile<PaymentProfile>(),
            NullLoggerFactory.Instance));
        var service = new PaymentService(unitOfWork, mapper);
        var request = new CreatePaymentRequest
        {
            StudentFeeId = feeId,
            Amount = 1500,
            PaymentMethod = PaymentMethod.Cash
        };

        started.SetResult(true);
        return await service.CreateAsync(request, cancellationToken);
    }

    private static async Task<CreateResult> CaptureResultAsync(
        Task<PaymentResponse> task)
    {
        try
        {
            await task;
            return new CreateResult(true, null);
        }
        catch (Exception exception)
        {
            return new CreateResult(false, exception);
        }
    }

        private static async Task AssertWorkersWaitingForLockAsync(
            string firstApplicationName,
            string secondApplicationName)
        {
            await using var context = CreateContext();
            var deadline = DateTime.UtcNow.AddSeconds(10);

            while (DateTime.UtcNow < deadline)
            {
                var waitingWorkers = await context.Database.SqlQuery<int>($"""
                    SELECT COUNT(*)::int AS "Value"
                    FROM pg_stat_activity
                    WHERE application_name IN ({firstApplicationName}, {secondApplicationName})
                      AND wait_event_type = 'Lock'
                    """).SingleAsync();

                if (waitingWorkers == 2)
                    return;

                await Task.Delay(50);
            }

            throw new TimeoutException("Both payment workers did not reach the StudentFee row lock.");
        }

        private static async Task CleanupAsync(
            int feeId,
            int enrollmentId,
            int groupId,
            int studentId,
            int teacherId)
        {
            if (feeId == 0)
                return;

            await using var cleanupContext = CreateContext();
            await cleanupContext.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM \"Payments\" WHERE \"StudentFeeId\" = {feeId}");
            await cleanupContext.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM \"StudentFees\" WHERE \"Id\" = {feeId}");
            await cleanupContext.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM \"Enrollments\" WHERE \"Id\" = {enrollmentId}");
            await cleanupContext.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM \"Groups\" WHERE \"Id\" = {groupId}");
            await cleanupContext.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM \"Students\" WHERE \"Id\" = {studentId}");
            await cleanupContext.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM \"Teachers\" WHERE \"Id\" = {teacherId}");
        }

        private static ApplicationDbContext CreateContext(
            string? applicationName = null)
    {
            var connectionString = Environment.GetEnvironmentVariable(
                "ConnectionStrings__DefaultConnection")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings__DefaultConnection must be set to run the PostgreSQL integration test.");
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);

            if (applicationName is not null)
                connectionStringBuilder.ApplicationName = applicationName;

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(connectionStringBuilder.ConnectionString)
            .Options;

        return new ApplicationDbContext(options, new TestCurrentUserService());
    }

    private sealed record CreateResult(bool Succeeded, Exception? Exception);

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public Guid UserId => Guid.Empty;

        public string? UserName => null;

        public bool IsAuthenticated => false;
    }
}
