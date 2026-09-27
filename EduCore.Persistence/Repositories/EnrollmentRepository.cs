using EduCore.Domain.Entities;
using EduCore.Domain.Interfaces.Repositories;
using EduCore.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EduCore.Persistence.Repositories
{
    public class EnrollmentRepository
        : GenericRepository<Enrollment>, IEnrollmentRepository
    {
        public EnrollmentRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<Enrollment>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Enrollments
                .Include(x => x.Student)
                    .ThenInclude(x => x.User)
                .Include(x => x.Group)
                    .ThenInclude(x => x.Teacher)
                        .ThenInclude(x => x.User)
                .AsNoTracking() 
                .ToListAsync(cancellationToken);
        }

        public async Task<Enrollment?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Enrollments
                .Include(x => x.Student)
                    .ThenInclude(x => x.User)
                .Include(x => x.Group)
                    .ThenInclude(x => x.Teacher)
                        .ThenInclude(x => x.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
    }
}