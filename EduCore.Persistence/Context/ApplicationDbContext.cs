using EduCore.Application.Common.Interfaces;
using EduCore.Domain.Common;
using EduCore.Domain.Entities;
using EduCore.Domain.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace EduCore.Persistence.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{

    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<StudentFee> StudentFees => Set<StudentFee>();
    public DbSet<Payment> Payments => Set<Payment>();


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasSequence<int>("StudentCodeSequence", schema: "dbo")
            .StartsAt(1)
            .IncrementsBy(1);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global soft-delete filters
        builder.Entity<Student>()
            .HasQueryFilter(x => !x.IsDeleted);

        builder.Entity<Teacher>()
            .HasQueryFilter(x => !x.IsDeleted);

        builder.Entity<Group>()
            .HasQueryFilter(x => !x.IsDeleted);

        builder.Entity<Enrollment>()
            .HasQueryFilter(x => !x.IsDeleted);

        builder.Entity<Attendance>()
            .HasQueryFilter(x => !x.IsDeleted);

        builder.Entity<StudentFee>()
            .HasQueryFilter(x => !x.IsDeleted);

        builder.Entity<Payment>()
            .HasQueryFilter(x => !x.IsDeleted);

    }


    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyAuditing();

        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditing()
    {
        var entries = ChangeTracker
            .Entries<BaseEntity>();

        var now = DateTime.UtcNow;

        var userId = _currentUserService.IsAuthenticated
            ? _currentUserService.UserId
            : (Guid?)null;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:

                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.IsDeleted = false;

                    break;

                case EntityState.Modified:

                    entry.Entity.LastModifiedAt = now;
                    entry.Entity.LastModifiedBy = userId;

                    break;

                case EntityState.Deleted:

                    // Soft Delete
                    entry.State = EntityState.Modified;

                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    entry.Entity.DeletedBy = userId;

                    break;
            }
        }
    }
}