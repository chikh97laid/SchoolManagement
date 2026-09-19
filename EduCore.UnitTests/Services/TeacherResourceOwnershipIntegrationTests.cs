using AutoMapper;
using EduCore.Application.Common.Exceptions;
using EduCore.Application.Common.Interfaces;
using EduCore.Application.Features.Enrollments.Mapping;
using EduCore.Application.Features.Enrollments.Services;
using EduCore.Application.Features.Groups.Mapping;
using EduCore.Application.Features.Groups.Services;
using EduCore.Application.Features.StudentFees.Mapping;
using EduCore.Application.Features.StudentFees.Services;
using EduCore.Domain.Entities;
using EduCore.Domain.Entities.Enums;
using EduCore.Domain.Identity;
using EduCore.Persistence.Context;
using EduCore.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace EduCore.UnitTests.Services;

public class TeacherResourceOwnershipIntegrationTests
{
    [Fact]
    public async Task TeacherA_CanAccessOwnGroup_AndCannotAccessTeacherBGroup()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var teacherAUserId = Guid.NewGuid();
        var teacherBUserId = Guid.NewGuid();

        await using var setupContext = CreateContext(Guid.NewGuid());
        await setupContext.Database.MigrateAsync();

        try
        {
            await setupContext.Users.AddAsync(CreateUser(teacherAUserId, $"teacher-a-{suffix}"));
            await setupContext.Users.AddAsync(CreateUser(teacherBUserId, $"teacher-b-{suffix}"));
            await setupContext.SaveChangesAsync();

            var teacherA = new Teacher
            {
                FirstName = "Teacher",
                LastName = "A-" + suffix,
                UserId = teacherAUserId,
                IsActive = true
            };

            var teacherB = new Teacher
            {
                FirstName = "Teacher",
                LastName = "B-" + suffix,
                UserId = teacherBUserId,
                IsActive = true
            };

            var groupA = new Group
            {
                Name = "TeacherA-Group-" + suffix,
                StandardFee = 1000,
                Teacher = teacherA,
                IsActive = true
            };

            var groupB = new Group
            {
                Name = "TeacherB-Group-" + suffix,
                StandardFee = 1500,
                Teacher = teacherB,
                IsActive = true
            };

            await setupContext.Teachers.AddRangeAsync(teacherA, teacherB);
            await setupContext.Groups.AddRangeAsync(groupA, groupB);
            await setupContext.SaveChangesAsync();

            var groupService = CreateGroupService(teacherAUserId);

            var ownGroup = await groupService.GetByIdAsync(groupA.Id);
            Assert.Equal(groupA.Id, ownGroup.Id);

            await Assert.ThrowsAsync<NotFoundException>(() => groupService.GetByIdAsync(groupB.Id));

            var teacherGroups = (await groupService.GetAllAsync()).ToList();
            Assert.Contains(teacherGroups, x => x.Id == groupA.Id);
            Assert.DoesNotContain(teacherGroups, x => x.Id == groupB.Id);
        }
        finally
        {
            await CleanupUserAsync(teacherAUserId, teacherBUserId);
        }
    }

    [Fact]
    public async Task TeacherA_GroupCollection_ContainsOnlyOwnGroups()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var teacherAUserId = Guid.NewGuid();
        var teacherBUserId = Guid.NewGuid();

        await using var setupContext = CreateContext(Guid.NewGuid());
        await setupContext.Database.MigrateAsync();

        try
        {
            await setupContext.Users.AddAsync(CreateUser(teacherAUserId, $"teacher-a-collection-{suffix}"));
            await setupContext.Users.AddAsync(CreateUser(teacherBUserId, $"teacher-b-collection-{suffix}"));
            await setupContext.SaveChangesAsync();

            var teacherA = new Teacher
            {
                FirstName = "Teacher",
                LastName = "A-Collection-" + suffix,
                UserId = teacherAUserId,
                IsActive = true
            };

            var teacherB = new Teacher
            {
                FirstName = "Teacher",
                LastName = "B-Collection-" + suffix,
                UserId = teacherBUserId,
                IsActive = true
            };

            var groupA1 = new Group
            {
                Name = "TeacherA-Collection-1-" + suffix,
                StandardFee = 900,
                Teacher = teacherA,
                IsActive = true
            };

            var groupA2 = new Group
            {
                Name = "TeacherA-Collection-2-" + suffix,
                StandardFee = 1100,
                Teacher = teacherA,
                IsActive = true
            };

            var groupB1 = new Group
            {
                Name = "TeacherB-Collection-1-" + suffix,
                StandardFee = 1300,
                Teacher = teacherB,
                IsActive = true
            };

            await setupContext.Teachers.AddRangeAsync(teacherA, teacherB);
            await setupContext.Groups.AddRangeAsync(groupA1, groupA2, groupB1);
            await setupContext.SaveChangesAsync();

            var groupService = CreateGroupService(teacherAUserId);
            var results = (await groupService.GetAllAsync()).ToList();

            Assert.Equal(2, results.Count);
            Assert.Contains(results, x => x.Id == groupA1.Id);
            Assert.Contains(results, x => x.Id == groupA2.Id);
            Assert.DoesNotContain(results, x => x.Id == groupB1.Id);
            Assert.All(results, group => Assert.Equal(teacherA.Id, group.TeacherId));
        }
        finally
        {
            await CleanupUserAsync(teacherAUserId, teacherBUserId);
        }
    }

    [Fact]
    public async Task TeacherA_CannotReadTeacherBEnrollment_OrTeacherBStudentFee()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var teacherAUserId = Guid.NewGuid();
        var teacherBUserId = Guid.NewGuid();

        await using var setupContext = CreateContext(Guid.NewGuid());
        await setupContext.Database.MigrateAsync();

        try
        {
            await setupContext.Users.AddAsync(CreateUser(teacherAUserId, $"teacher-a-enroll-{suffix}"));
            await setupContext.Users.AddAsync(CreateUser(teacherBUserId, $"teacher-b-enroll-{suffix}"));
            await setupContext.SaveChangesAsync();

            var teacherA = new Teacher
            {
                FirstName = "Teacher",
                LastName = "A-" + suffix,
                UserId = teacherAUserId,
                IsActive = true
            };

            var teacherB = new Teacher
            {
                FirstName = "Teacher",
                LastName = "B-" + suffix,
                UserId = teacherBUserId,
                IsActive = true
            };

            var student = new Student
            {
                StudentCode = "OWNERSHIP-" + suffix,
                FirstName = "Student",
                LastName = suffix,
                Gender = Gender.Male,
                IsActive = true
            };

            var groupB = new Group
            {
                Name = "TeacherB-Enroll-" + suffix,
                StandardFee = 1200,
                Teacher = teacherB,
                IsActive = true
            };

            var enrollmentB = new Enrollment
            {
                Student = student,
                Group = groupB,
                IsActive = true
            };

            var feeB = new StudentFee
            {
                Enrollment = enrollmentB,
                Title = "Ownership Fee " + suffix,
                TotalAmount = 500,
                PaidAmount = 0,
                Status = StudentFeeStatus.Pending
            };

            await setupContext.Teachers.AddRangeAsync(teacherA, teacherB);
            await setupContext.Students.AddAsync(student);
            await setupContext.Groups.AddAsync(groupB);
            await setupContext.Enrollments.AddAsync(enrollmentB);
            await setupContext.StudentFees.AddAsync(feeB);
            await setupContext.SaveChangesAsync();

            var enrollmentService = CreateEnrollmentService(teacherAUserId);
            var feeService = CreateStudentFeeService(teacherAUserId);

            await Assert.ThrowsAsync<NotFoundException>(() => enrollmentService.GetByIdAsync(enrollmentB.Id));
            await Assert.ThrowsAsync<NotFoundException>(() => feeService.GetByIdAsync(feeB.Id));
        }
        finally
        {
            await CleanupUserAsync(teacherAUserId, teacherBUserId);
        }
    }

    [Fact]
    public async Task DeletedResources_RemainInaccessible_ToTeacher()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var teacherUserId = Guid.NewGuid();

        await using var setupContext = CreateContext(Guid.NewGuid());
        await setupContext.Database.MigrateAsync();

        try
        {
            await setupContext.Users.AddAsync(CreateUser(teacherUserId, $"teacher-deleted-{suffix}"));
            await setupContext.SaveChangesAsync();

            var teacher = new Teacher
            {
                FirstName = "Teacher",
                LastName = "Deleted-" + suffix,
                UserId = teacherUserId,
                IsActive = true
            };

            var group = new Group
            {
                Name = "Deleted-Group-" + suffix,
                StandardFee = 900,
                Teacher = teacher,
                IsActive = true
            };

            await setupContext.Teachers.AddAsync(teacher);
            await setupContext.Groups.AddAsync(group);
            await setupContext.SaveChangesAsync();

            group.IsDeleted = true;
            await setupContext.SaveChangesAsync();

            var groupService = CreateGroupService(teacherUserId);
            await Assert.ThrowsAsync<NotFoundException>(() => groupService.GetByIdAsync(group.Id));
        }
        finally
        {
            await CleanupUserAsync(teacherUserId);
        }
    }

    [Fact]
    public async Task AdminUser_CanReadSharedGroup_WhenAdminFlagIsSet()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var adminUserId = Guid.NewGuid();

        await using var setupContext = CreateContext(Guid.NewGuid());
        await setupContext.Database.MigrateAsync();

        try
        {
            await setupContext.Users.AddAsync(CreateUser(adminUserId, $"admin-shared-{suffix}"));
            await setupContext.SaveChangesAsync();

            var teacher = new Teacher
            {
                FirstName = "Other",
                LastName = "Teacher-" + suffix,
                UserId = adminUserId,
                IsActive = true
            };

            var group = new Group
            {
                Name = "AdminSharedAccess-" + suffix,
                StandardFee = 750,
                Teacher = teacher,
                IsActive = true
            };

            await setupContext.Teachers.AddAsync(teacher);
            await setupContext.Groups.AddAsync(group);
            await setupContext.SaveChangesAsync();

            var groupService = CreateGroupService(adminUserId, isAdmin: true);
            var result = await groupService.GetByIdAsync(group.Id);

            Assert.Equal(group.Id, result.Id);
            Assert.Equal(group.TeacherId, result.TeacherId);
        }
        finally
        {
            await CleanupUserAsync(adminUserId);
        }
    }

    [Fact]
    public async Task NonAdminUser_WithoutTeacherProfile_CannotReadOtherTeacherGroup()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var missingTeacherUserId = Guid.NewGuid();
        var otherTeacherUserId = Guid.NewGuid();

        await using var setupContext = CreateContext(Guid.NewGuid());
        await setupContext.Database.MigrateAsync();

        try
        {
            await setupContext.Users.AddAsync(CreateUser(missingTeacherUserId, $"teacher-missing-regression-{suffix}"));
            await setupContext.Users.AddAsync(CreateUser(otherTeacherUserId, $"teacher-other-regression-{suffix}"));
            await setupContext.SaveChangesAsync();

            var teacher = new Teacher
            {
                FirstName = "Teacher",
                LastName = "Other-" + suffix,
                UserId = otherTeacherUserId,
                IsActive = true
            };

            var group = new Group
            {
                Name = "Other-Teacher-Group-" + suffix,
                StandardFee = 600,
                Teacher = teacher,
                IsActive = true
            };

            await setupContext.Teachers.AddAsync(teacher);
            await setupContext.Groups.AddAsync(group);
            await setupContext.SaveChangesAsync();

            var groupService = CreateGroupService(missingTeacherUserId);

            await Assert.ThrowsAsync<NotFoundException>(() => groupService.GetByIdAsync(group.Id));
            Assert.Empty(await groupService.GetAllAsync());
        }
        finally
        {
            await CleanupUserAsync(missingTeacherUserId, otherTeacherUserId);
        }
    }

    [Fact]
    public async Task TeacherWithoutTeacherProfile_CannotReadGroupById_AndCollection()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var missingTeacherUserId = Guid.NewGuid();
        var otherTeacherUserId = Guid.NewGuid();

        await using var setupContext = CreateContext(Guid.NewGuid());
        await setupContext.Database.MigrateAsync();

        try
        {
            await setupContext.Users.AddAsync(CreateUser(missingTeacherUserId, $"teacher-missing-{suffix}"));
            await setupContext.Users.AddAsync(CreateUser(otherTeacherUserId, $"teacher-other-{suffix}"));
            await setupContext.SaveChangesAsync();

            var teacher = new Teacher
            {
                FirstName = "Teacher",
                LastName = "Other-" + suffix,
                UserId = otherTeacherUserId,
                IsActive = true
            };

            var group = new Group
            {
                Name = "Outside-Teacher-Group-" + suffix,
                StandardFee = 500,
                Teacher = teacher,
                IsActive = true
            };

            await setupContext.Teachers.AddAsync(teacher);
            await setupContext.Groups.AddAsync(group);
            await setupContext.SaveChangesAsync();

            var groupService = CreateGroupService(missingTeacherUserId);

            await Assert.ThrowsAsync<NotFoundException>(() => groupService.GetByIdAsync(group.Id));
            Assert.Empty(await groupService.GetAllAsync());
        }
        finally
        {
            await CleanupUserAsync(missingTeacherUserId, otherTeacherUserId);
        }
    }

    [Fact]
    public async Task TeacherWithoutTeacherProfile_CannotReadEnrollment_OrStudentFee()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var missingTeacherUserId = Guid.NewGuid();
        var otherTeacherUserId = Guid.NewGuid();

        await using var setupContext = CreateContext(Guid.NewGuid());
        await setupContext.Database.MigrateAsync();

        try
        {
            await setupContext.Users.AddAsync(CreateUser(missingTeacherUserId, $"teacher-missing-enroll-{suffix}"));
            await setupContext.Users.AddAsync(CreateUser(otherTeacherUserId, $"teacher-other-enroll-{suffix}"));
            await setupContext.SaveChangesAsync();

            var teacher = new Teacher
            {
                FirstName = "Teacher",
                LastName = "Other-" + suffix,
                UserId = otherTeacherUserId,
                IsActive = true
            };

            var student = new Student
            {
                StudentCode = "NO-TEACHER-" + suffix,
                FirstName = "Student",
                LastName = suffix,
                Gender = Gender.Male,
                IsActive = true
            };

            var group = new Group
            {
                Name = "Outside-Teacher-Enrollment-" + suffix,
                StandardFee = 750,
                Teacher = teacher,
                IsActive = true
            };

            var enrollment = new Enrollment
            {
                Student = student,
                Group = group,
                IsActive = true
            };

            var fee = new StudentFee
            {
                Enrollment = enrollment,
                Title = "Unowned Fee " + suffix,
                TotalAmount = 250,
                PaidAmount = 0,
                Status = StudentFeeStatus.Pending
            };

            await setupContext.Teachers.AddAsync(teacher);
            await setupContext.Students.AddAsync(student);
            await setupContext.Groups.AddAsync(group);
            await setupContext.Enrollments.AddAsync(enrollment);
            await setupContext.StudentFees.AddAsync(fee);
            await setupContext.SaveChangesAsync();

            var enrollmentService = CreateEnrollmentService(missingTeacherUserId);
            var feeService = CreateStudentFeeService(missingTeacherUserId);

            await Assert.ThrowsAsync<NotFoundException>(() => enrollmentService.GetByIdAsync(enrollment.Id));
            await Assert.ThrowsAsync<NotFoundException>(() => feeService.GetByIdAsync(fee.Id));
        }
        finally
        {
            await CleanupUserAsync(missingTeacherUserId, otherTeacherUserId);
        }
    }

    private static GroupService CreateGroupService(Guid currentUserId, bool isAdmin = false)
    {
        var mapper = new Mapper(new MapperConfiguration(
            configuration => configuration.AddProfile<GroupProfile>(),
            NullLoggerFactory.Instance));
        var context = CreateContext(currentUserId);
        var unitOfWork = new UnitOfWork(context);
        return new GroupService(unitOfWork, mapper, new TestCurrentUserService(currentUserId, isAdmin));
    }

    private static EnrollmentService CreateEnrollmentService(Guid currentUserId, bool isAdmin = false)
    {
        var mapper = new Mapper(new MapperConfiguration(
            configuration => configuration.AddProfile<EnrollmentProfile>(),
            NullLoggerFactory.Instance));
        var context = CreateContext(currentUserId);
        var unitOfWork = new UnitOfWork(context);
        return new EnrollmentService(unitOfWork, mapper, new TestCurrentUserService(currentUserId, isAdmin));
    }

    private static StudentFeeService CreateStudentFeeService(Guid currentUserId, bool isAdmin = false)
    {
        var mapper = new Mapper(new MapperConfiguration(
            configuration => configuration.AddProfile<StudentFeeProfile>(),
            NullLoggerFactory.Instance));
        var context = CreateContext(currentUserId);
        var unitOfWork = new UnitOfWork(context);
        return new StudentFeeService(unitOfWork, mapper, new TestCurrentUserService(currentUserId, isAdmin));
    }

    private static ApplicationDbContext CreateContext(Guid currentUserId)
    {
        var connectionString = Environment.GetEnvironmentVariable(
                "ConnectionStrings__DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings__DefaultConnection must be set to run the PostgreSQL integration test.");

        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionStringBuilder.ConnectionString)
            .Options;

        return new ApplicationDbContext(options, new TestCurrentUserService(currentUserId));
    }

    private static ApplicationUser CreateUser(Guid userId, string userName)
    {
        return new ApplicationUser
        {
            Id = userId,
            UserName = userName,
            NormalizedUserName = userName,
            Email = $"{userName}@example.com",
            NormalizedEmail = $"{userName}@example.com",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            FirstName = userName,
            LastName = "Test",
            IsActive = true
        };
    }

    private static async Task CleanupUserAsync(params Guid[] userIds)
    {
        if (userIds.Length == 0)
            return;

        await using var cleanupContext = CreateContext(Guid.NewGuid());

        var distinctUserIds = userIds.Distinct().ToArray();
        var teacherIds = await cleanupContext.Teachers
            .Where(x => x.UserId.HasValue && distinctUserIds.Contains(x.UserId.Value))
            .Select(x => x.Id)
            .ToListAsync();

        if (teacherIds.Count > 0)
        {
            var groupIds = await cleanupContext.Groups
                .Where(x => teacherIds.Contains(x.TeacherId))
                .Select(x => x.Id)
                .ToListAsync();

            if (groupIds.Count > 0)
            {
                var enrollmentIds = await cleanupContext.Enrollments
                    .Where(x => groupIds.Contains(x.GroupId))
                    .Select(x => x.Id)
                    .ToListAsync();

                if (enrollmentIds.Count > 0)
                {
                    var studentIds = await cleanupContext.Enrollments
                        .Where(x => enrollmentIds.Contains(x.Id))
                        .Select(x => x.StudentId)
                        .Distinct()
                        .ToListAsync();

                    var feeIds = await cleanupContext.StudentFees
                        .Where(x => enrollmentIds.Contains(x.EnrollmentId))
                        .Select(x => x.Id)
                        .ToListAsync();

                    if (feeIds.Count > 0)
                    {
                        var payments = await cleanupContext.Payments
                            .Where(x => feeIds.Contains(x.StudentFeeId))
                            .ToListAsync();

                        if (payments.Count > 0)
                        {
                            cleanupContext.Payments.RemoveRange(payments);
                        }

                        var studentFees = await cleanupContext.StudentFees
                            .Where(x => feeIds.Contains(x.Id))
                            .ToListAsync();

                        if (studentFees.Count > 0)
                        {
                            cleanupContext.StudentFees.RemoveRange(studentFees);
                        }
                    }

                    var enrollments = await cleanupContext.Enrollments
                        .Where(x => enrollmentIds.Contains(x.Id))
                        .ToListAsync();

                    if (enrollments.Count > 0)
                    {
                        cleanupContext.Enrollments.RemoveRange(enrollments);
                    }

                    if (studentIds.Count > 0)
                    {
                        var students = await cleanupContext.Students
                            .Where(x => studentIds.Contains(x.Id))
                            .ToListAsync();

                        if (students.Count > 0)
                        {
                            cleanupContext.Students.RemoveRange(students);
                        }
                    }
                }

                var groups = await cleanupContext.Groups
                    .Where(x => groupIds.Contains(x.Id))
                    .ToListAsync();

                if (groups.Count > 0)
                {
                    cleanupContext.Groups.RemoveRange(groups);
                }
            }

            var teachers = await cleanupContext.Teachers
                .Where(x => teacherIds.Contains(x.Id))
                .ToListAsync();

            if (teachers.Count > 0)
            {
                cleanupContext.Teachers.RemoveRange(teachers);
            }
        }

        var users = await cleanupContext.Users
            .Where(x => distinctUserIds.Contains(x.Id))
            .ToListAsync();

        if (users.Count > 0)
        {
            cleanupContext.Users.RemoveRange(users);
        }

        await cleanupContext.SaveChangesAsync();
    }

    private sealed class TestCurrentUserService(Guid currentUserId, bool isAdmin = false) : ICurrentUserService
    {
        public Guid UserId => currentUserId;

        public string? UserName => "test-user";

        public bool IsAuthenticated => true;

        public bool IsAdmin => isAdmin;
    }
}
