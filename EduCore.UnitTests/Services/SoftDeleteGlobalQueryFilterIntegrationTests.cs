using AutoMapper;
using EduCore.Application.Common.Exceptions;
using EduCore.Application.Common.Interfaces;
using EduCore.Application.Common.Pagination;
using EduCore.Application.Features.Enrollments.Mapping;
using EduCore.Application.Features.Enrollments.Services;
using EduCore.Application.Features.Groups.Mapping;
using EduCore.Application.Features.Groups.DTOs;
using EduCore.Application.Features.Groups.Services;
using EduCore.Application.Features.Payments.Mapping;
using EduCore.Application.Features.Payments.Services;
using EduCore.Domain.Entities;
using EduCore.Domain.Entities.Enums;
using EduCore.Domain.Identity;
using EduCore.Persistence.Context;
using EduCore.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace EduCore.UnitTests.Services
{
    public class SoftDeleteGlobalQueryFilterIntegrationTests
    {
        [Fact]
        public async Task Group_SoftDelete_HidesEntityFromGetById()
        {
            var scenario = await CreateScenarioAsync();

            try
            {
                await Assert.ThrowsAsync<NotFoundException>(
                    async () =>
                    {
                        await using var serviceContext = CreateContext();
                        var service = CreateGroupService(serviceContext);
                        await service.SoftDeleteAsync(scenario.GroupId);
                        await service.GetByIdAsync(scenario.GroupId);
                    });
            }
            finally
            {
                await CleanupAsync(scenario);
            }
        }

        [Fact]
        public async Task DeletedGroup_MakesEnrollmentInaccessible()
        {
            var scenario = await CreateScenarioAsync();

            try
            {
                await using (var setupContext = CreateContext())
                {
                    var group = await setupContext.Groups
                        .SingleAsync(x => x.Id == scenario.GroupId);
                    group.IsDeleted = true;
                    group.DeletedAt = DateTime.UtcNow;
                    await setupContext.SaveChangesAsync();
                }

                await using var serviceContext = CreateContext();
                var service = CreateEnrollmentService(serviceContext);
                await Assert.ThrowsAsync<NotFoundException>(
                    () => service.GetByIdAsync(scenario.EnrollmentId));
            }
            finally
            {
                await CleanupAsync(scenario);
            }
        }

        [Fact]
        public async Task Group_SoftDelete_RemovesEntityFromCollection()
        {
            var scenario = await CreateScenarioAsync();

            try
            {
                await using var serviceContext = CreateContext();
                var service = CreateGroupService(serviceContext);
                await service.SoftDeleteAsync(scenario.GroupId);

                await using var readContext = CreateContext();
                var groups = await CreateGroupService(readContext)
                    .GetAllAsync(new PaginationParameters());
                Assert.DoesNotContain(groups.Items, group => group.Id == scenario.GroupId);
            }
            finally
            {
                await CleanupAsync(scenario);
            }
        }

        [Fact]
        public async Task Group_Create_MapsTeacherNameFromLoadedIdentityUser()
        {
            var scenario = await CreateScenarioAsync();
            var createdGroupId = 0;

            try
            {
                await using var serviceContext = CreateContext();
                var createdGroup = await CreateGroupService(serviceContext).CreateAsync(
                    new CreateGroupRequest
                    {
                        Name = $"Created-Group-{Guid.NewGuid():N}",
                        StandardFee = 1200,
                        TeacherId = scenario.TeacherId
                    });
                createdGroupId = createdGroup.Id;

                Assert.Equal("Test User", createdGroup.TeacherName);
            }
            finally
            {
                if (createdGroupId != 0)
                {
                    await using var cleanupContext = CreateContext();
                    await cleanupContext.Database.ExecuteSqlInterpolatedAsync(
                        $"DELETE FROM \"Groups\" WHERE \"Id\" = {createdGroupId}");
                }

                await CleanupAsync(scenario);
            }
        }

        [Fact]
        public async Task Payment_SoftDelete_ReversesFeeAndHidesPayment()
        {
            var scenario = await CreateScenarioAsync(includePayment: true);

            try
            {
                await using (var serviceContext = CreateContext())
                {
                    var service = CreatePaymentService(serviceContext);
                    await service.SoftDeleteAsync(scenario.PaymentId);
                }

                await using var verificationContext = CreateContext();
                var fee = await verificationContext.StudentFees
                    .SingleAsync(x => x.Id == scenario.StudentFeeId);
                var visiblePayment = await verificationContext.Payments
                    .FirstOrDefaultAsync(x => x.Id == scenario.PaymentId);
                var historicalPayment = await verificationContext.Payments
                    .IgnoreQueryFilters()
                    .SingleAsync(x => x.Id == scenario.PaymentId);

                Assert.Equal(0, fee.PaidAmount);
                Assert.Null(visiblePayment);
                Assert.True(historicalPayment.IsDeleted);
                Assert.Equal(PaymentStatus.Cancelled, historicalPayment.Status);
            }
            finally
            {
                await CleanupAsync(scenario);
            }
        }

        [Fact]
        public async Task Payment_SoftDelete_WhenAlreadyCancelled_LeavesPaidAmountUnchanged()
        {
            var scenario = await CreateScenarioAsync(
                includePayment: true,
                targetPaymentAlreadyCancelled: true);

            try
            {
                await using (var serviceContext = CreateContext())
                {
                    var service = CreatePaymentService(serviceContext);
                    await service.SoftDeleteAsync(scenario.PaymentId);
                }

                await using var verificationContext = CreateContext();
                var fee = await verificationContext.StudentFees
                    .SingleAsync(x => x.Id == scenario.StudentFeeId);

                Assert.Equal(400, fee.PaidAmount);
            }
            finally
            {
                await CleanupAsync(scenario);
            }
        }

        private static async Task<TestScenario> CreateScenarioAsync(
            bool includePayment = false,
            bool targetPaymentAlreadyCancelled = false)
        {
            var suffix = Guid.NewGuid().ToString("N")[..10];
            var teacherUserId = Guid.NewGuid();
            var studentUserId = Guid.NewGuid();

            await using var context = CreateContext();
            await context.Database.MigrateAsync();

            context.Users.AddRange(
                CreateUser(teacherUserId, $"teacher-{suffix}"),
                CreateUser(studentUserId, $"student-{suffix}"));
            await context.SaveChangesAsync();

            var teacher = new Teacher
            {
                UserId = teacherUserId,
                HireDate = DateTime.UtcNow,
                IsActive = true
            };
            var student = new Student
            {
                UserId = studentUserId,
                StudentCode = $"STU-{suffix}",
                Gender = Gender.Male,
                IsActive = true
            };
            var group = new Group
            {
                Name = $"Test-Group-{suffix}",
                StandardFee = 1000,
                Teacher = teacher,
                IsActive = true
            };
            var enrollment = new Enrollment
            {
                Student = student,
                Group = group,
                IsActive = true
            };

            context.Enrollments.Add(enrollment);

            var studentFeeId = 0;
            var paymentId = 0;
            if (includePayment)
            {
                var fee = new StudentFee
                {
                    Enrollment = enrollment,
                    Title = "Test fee",
                    TotalAmount = 1000,
                    PaidAmount = 400,
                    Status = StudentFeeStatus.PartiallyPaid
                };
                var completedPayment = new Payment
                {
                    StudentFee = fee,
                    Amount = 400,
                    PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    PaymentMethod = PaymentMethod.Cash,
                    Status = PaymentStatus.Completed
                };
                var targetPayment = targetPaymentAlreadyCancelled
                    ? new Payment
                    {
                        StudentFee = fee,
                        Amount = 300,
                        PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        PaymentMethod = PaymentMethod.Cash,
                        Status = PaymentStatus.Cancelled
                    }
                    : completedPayment;

                context.StudentFees.Add(fee);
                context.Payments.Add(completedPayment);
                if (targetPaymentAlreadyCancelled)
                    context.Payments.Add(targetPayment);

                await context.SaveChangesAsync();
                studentFeeId = fee.Id;
                paymentId = targetPayment.Id;
            }
            else
            {
                await context.SaveChangesAsync();
            }

            return new TestScenario(
                teacher.Id,
                student.Id,
                group.Id,
                enrollment.Id,
                studentFeeId,
                paymentId,
                teacherUserId,
                studentUserId);
        }

        private static ApplicationDbContext CreateContext()
        {
            var connectionString = Environment.GetEnvironmentVariable(
                    "ConnectionStrings__DefaultConnection")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings__DefaultConnection must be set to run the PostgreSQL integration test.");

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(connectionStringBuilder.ConnectionString)
                .Options;

            return new ApplicationDbContext(options, new TestCurrentUserService());
        }

        private static ApplicationUser CreateUser(Guid id, string userName)
        {
            return new ApplicationUser
            {
                Id = id,
                UserName = userName,
                NormalizedUserName = userName.ToUpperInvariant(),
                Email = $"{userName}@example.com",
                NormalizedEmail = $"{userName}@example.com".ToUpperInvariant(),
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                FirstName = "Test",
                LastName = "User",
                IsActive = true
            };
        }

        private static async Task CleanupAsync(
            TestScenario scenario)
        {
            await using var context = CreateContext();
            if (scenario.StudentFeeId != 0)
            {
                await context.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM \"Payments\" WHERE \"StudentFeeId\" = {scenario.StudentFeeId}");
                await context.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM \"StudentFees\" WHERE \"Id\" = {scenario.StudentFeeId}");
            }
            if (scenario.EnrollmentId != 0)
            {
                await context.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM \"Attendances\" WHERE \"EnrollmentId\" = {scenario.EnrollmentId}");
                await context.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM \"Enrollments\" WHERE \"Id\" = {scenario.EnrollmentId}");
            }
            if (scenario.GroupId != 0)
                await context.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM \"Groups\" WHERE \"Id\" = {scenario.GroupId}");
            if (scenario.StudentId != 0)
                await context.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM \"Students\" WHERE \"Id\" = {scenario.StudentId}");
            if (scenario.TeacherId != 0)
                await context.Database.ExecuteSqlInterpolatedAsync(
                    $"DELETE FROM \"Teachers\" WHERE \"Id\" = {scenario.TeacherId}");

            await context.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM \"AspNetUsers\" WHERE \"Id\" = ANY(ARRAY[{scenario.TeacherUserId}, {scenario.StudentUserId}])");
        }

        private static GroupService CreateGroupService(ApplicationDbContext context)
        {
            var mapper = new Mapper(new MapperConfiguration(
                configuration => configuration.AddProfile<GroupProfile>(),
                NullLoggerFactory.Instance));
            return new GroupService(
                new UnitOfWork(context),
                mapper,
                new TestCurrentUserService(isAdmin: true));
        }

        private static EnrollmentService CreateEnrollmentService(ApplicationDbContext context)
        {
            var mapper = new Mapper(new MapperConfiguration(
                configuration => configuration.AddProfile<EnrollmentProfile>(),
                NullLoggerFactory.Instance));
            return new EnrollmentService(
                new UnitOfWork(context),
                mapper,
                new TestCurrentUserService(isAdmin: true));
        }

        private static PaymentService CreatePaymentService(ApplicationDbContext context)
        {
            var mapper = new Mapper(new MapperConfiguration(
                configuration => configuration.AddProfile<PaymentProfile>(),
                NullLoggerFactory.Instance));
            return new PaymentService(
                new UnitOfWork(context),
                mapper,
                new TestCurrentUserService(isAdmin: true));
        }

        private sealed record TestScenario(
            int TeacherId,
            int StudentId,
            int GroupId,
            int EnrollmentId,
            int StudentFeeId,
            int PaymentId,
            Guid TeacherUserId,
            Guid StudentUserId);

        private sealed class TestCurrentUserService(bool isAdmin = false) : ICurrentUserService
        {
            public Guid UserId => Guid.Empty;
            public string? UserName => null;
            public bool IsAuthenticated => false;
            public bool IsAdmin => isAdmin;
            public bool IsStudent => false;
            public bool IsTeacher => false;
        }
    }
}
