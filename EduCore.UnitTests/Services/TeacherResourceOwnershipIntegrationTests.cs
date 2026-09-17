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
using Xunit;

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

        await CleanupAsync(groupA.Id, groupB.Id, teacherA.Id, teacherB.Id);
        await CleanupUserAsync(teacherAUserId, teacherBUserId);
    }

    [Fact]
    public async Task TeacherA_GroupCollection_ContainsOnlyOwnGroups()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var teacherAUserId = Guid.NewGuid();
        var teacherBUserId = Guid.NewGuid();

        await using var setupContext = CreateContext(Guid.NewGuid());
        await setupContext.Database.MigrateAsync();

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

        await CleanupAsync(groupA1.Id, groupA2.Id, groupB1.Id, teacherA.Id, teacherB.Id);
        await CleanupUserAsync(teacherAUserId, teacherBUserId);
    }

    [Fact]
    public async Task TeacherA_CannotReadTeacherBEnrollment_OrTeacherBStudentFee()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var teacherAUserId = Guid.NewGuid();
        var teacherBUserId = Guid.NewGuid();

        await using var setupContext = CreateContext(Guid.NewGuid());
        await setupContext.Database.MigrateAsync();

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

        await CleanupAsync(groupB.Id, enrollmentB.Id, student.Id, teacherA.Id, teacherB.Id, feeB.Id);
        await CleanupUserAsync(teacherAUserId, teacherBUserId);
    }

    [Fact]
    public async Task DeletedResources_RemainInaccessible_ToTeacher()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var teacherUserId = Guid.NewGuid();

        await using var setupContext = CreateContext(Guid.NewGuid());
        await setupContext.Database.MigrateAsync();

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

        await CleanupAsync(group.Id, teacher.Id);
        await CleanupUserAsync(teacherUserId);
    }

    [Fact]
    public async Task AdminRequest_WithoutTeacherRelation_CanStillAccessResource()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var currentUserId = Guid.NewGuid();

        await using var setupContext = CreateContext(Guid.NewGuid());
        await setupContext.Database.MigrateAsync();

        var teacherUserId = Guid.NewGuid();
        await setupContext.Users.AddAsync(CreateUser(teacherUserId, $"teacher-admin-{suffix}"));
        await setupContext.SaveChangesAsync();

        var teacher = new Teacher
        {
            FirstName = "Other",
            LastName = "Teacher-" + suffix,
            UserId = teacherUserId,
            IsActive = true
        };

        var group = new Group
        {
            Name = "AdminAccess-" + suffix,
            StandardFee = 500,
            Teacher = teacher,
            IsActive = true
        };

        await setupContext.Teachers.AddAsync(teacher);
        await setupContext.Groups.AddAsync(group);
        await setupContext.SaveChangesAsync();

        var groupService = CreateGroupService(currentUserId);

        var result = await groupService.GetByIdAsync(group.Id);
        Assert.Equal(group.Id, result.Id);

        await CleanupAsync(group.Id, teacher.Id);
        await CleanupUserAsync(teacherUserId);
    }

    private static GroupService CreateGroupService(Guid currentUserId)
    {
        var mapper = new Mapper(new MapperConfiguration(
            configuration => configuration.AddProfile<GroupProfile>(),
            NullLoggerFactory.Instance));
        var context = CreateContext(currentUserId);
        var unitOfWork = new UnitOfWork(context);
        return new GroupService(unitOfWork, mapper, new TestCurrentUserService(currentUserId));
    }

    private static EnrollmentService CreateEnrollmentService(Guid currentUserId)
    {
        var mapper = new Mapper(new MapperConfiguration(
            configuration => configuration.AddProfile<EnrollmentProfile>(),
            NullLoggerFactory.Instance));
        var context = CreateContext(currentUserId);
        var unitOfWork = new UnitOfWork(context);
        return new EnrollmentService(unitOfWork, mapper, new TestCurrentUserService(currentUserId));
    }

    private static StudentFeeService CreateStudentFeeService(Guid currentUserId)
    {
        var mapper = new Mapper(new MapperConfiguration(
            configuration => configuration.AddProfile<StudentFeeProfile>(),
            NullLoggerFactory.Instance));
        var context = CreateContext(currentUserId);
        var unitOfWork = new UnitOfWork(context);
        return new StudentFeeService(unitOfWork, mapper, new TestCurrentUserService(currentUserId));
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

    private static async Task CleanupAsync(params int[] ids)
    {
        if (ids.Length == 0)
            return;

        var sortedIds = ids.Where(x => x > 0).Distinct().OrderByDescending(x => x).ToArray();
        if (sortedIds.Length == 0)
            return;

        await using var cleanupContext = CreateContext(Guid.NewGuid());

        foreach (var id in sortedIds)
        {
            var payments = await cleanupContext.Payments
                .Where(x => x.StudentFeeId == id)
                .ToListAsync();

            if (payments.Count > 0)
            {
                cleanupContext.Payments.RemoveRange(payments);
            }

            var studentFee = await cleanupContext.StudentFees.FirstOrDefaultAsync(x => x.Id == id);
            if (studentFee is not null)
            {
                cleanupContext.StudentFees.Remove(studentFee);
            }

            var enrollment = await cleanupContext.Enrollments.FirstOrDefaultAsync(x => x.Id == id);
            if (enrollment is not null)
            {
                cleanupContext.Enrollments.Remove(enrollment);
            }

            var group = await cleanupContext.Groups.FirstOrDefaultAsync(x => x.Id == id);
            if (group is not null)
            {
                cleanupContext.Groups.Remove(group);
            }

            var student = await cleanupContext.Students.FirstOrDefaultAsync(x => x.Id == id);
            if (student is not null)
            {
                cleanupContext.Students.Remove(student);
            }

            var teacher = await cleanupContext.Teachers.FirstOrDefaultAsync(x => x.Id == id);
            if (teacher is not null)
            {
                cleanupContext.Teachers.Remove(teacher);
            }
        }

        await cleanupContext.SaveChangesAsync();
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

    private sealed class TestCurrentUserService(Guid currentUserId) : ICurrentUserService
    {
        public Guid UserId => currentUserId;

        public string? UserName => "test-user";

        public bool IsAuthenticated => true;
    }
}
