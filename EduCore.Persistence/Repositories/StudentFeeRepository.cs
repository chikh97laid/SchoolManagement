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

    public async Task<IEnumerable<StudentFee>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
                        .Include(x => x.Enrollment)
                            .ThenInclude(x => x.Student)

                        .Include(x => x.Enrollment)
                            .ThenInclude(x => x.Group)
                        .Where(x => !x.IsDeleted)
                        .AsNoTracking()
                        .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentFee>> GetByEnrollmentAsync(int enrollmentId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
                        .Include(x => x.Enrollment)
                            .ThenInclude(x => x.Student)

                        .Include(x => x.Enrollment)
                            .ThenInclude(x => x.Group)
                        
                        .Where(x => x.EnrollmentId == enrollmentId && !x.IsDeleted)
                        .AsNoTracking()
                        .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentFee>> GetByStudentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(x => x.Enrollment)
                .ThenInclude(x => x.Student)

            .Include(x => x.Enrollment)
                .ThenInclude(x => x.Group)

            .Where(x => x.Enrollment.StudentId == studentId && !x.IsDeleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentFee?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(x => x.Enrollment)
                .ThenInclude(x => x.Student)
            .Include(x => x.Enrollment)
                .ThenInclude(x => x.Group)
            .Where(x => !x.IsDeleted)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}