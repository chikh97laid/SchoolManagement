using EduCore.Domain.Entities;
using EduCore.Domain.Interfaces.Repositories;
using EduCore.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Persistence.Repositories;
public class StudentFeeRepository
    : GenericRepository<StudentFee>,
      IStudentFeeRepository
{
    public StudentFeeRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    public async Task<StudentFee?> GetForUpdateAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var fees = await _context.StudentFees
            .FromSqlInterpolated($"""
                SELECT *
                FROM "StudentFees"
                WHERE "Id" = {id}
                AND NOT "IsDeleted"
                FOR UPDATE
                """)
            .AsTracking()
            .ToListAsync(cancellationToken);

        return fees.SingleOrDefault();
    }

    public async Task<IEnumerable<StudentFee>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
                        .Include(x => x.Enrollment)
                            .ThenInclude(x => x.Student)
                                .ThenInclude(x => x.User)
                        .Include(x => x.Enrollment)
                            .ThenInclude(x => x.Group)
                        .AsNoTracking()
                        .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentFee>> GetByEnrollmentAsync(int enrollmentId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
                        .Include(x => x.Enrollment)
                            .ThenInclude(x => x.Student)
                                .ThenInclude(x => x.User)
                        .Include(x => x.Enrollment)
                            .ThenInclude(x => x.Group)
                        .Where(x => x.EnrollmentId == enrollmentId)
                        .AsNoTracking()
                        .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentFee>> GetByStudentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(x => x.Enrollment)
                .ThenInclude(x => x.Student)
                    .ThenInclude(x => x.User)
            .Include(x => x.Enrollment)
                .ThenInclude(x => x.Group)
            .Where(x => x.Enrollment.StudentId == studentId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentFee?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(x => x.Enrollment)
                .ThenInclude(x => x.Student)
                    .ThenInclude(x => x.User)
            .Include(x => x.Enrollment)
                .ThenInclude(x => x.Group)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}