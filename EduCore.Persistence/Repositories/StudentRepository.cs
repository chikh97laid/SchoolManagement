using EduCore.Domain.Entities;
using EduCore.Domain.Interfaces.Repositories;
using EduCore.Persistence.Context;
using Microsoft.EntityFrameworkCore;


namespace EduCore.Persistence.Repositories
{
    public class StudentRepository
        : GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<int> GetNextStudentSequenceAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Database
            .SqlQueryRaw<int>("SELECT nextval('dbo.\"StudentCodeSequence\"') AS \"Value\"")
            .SingleAsync(cancellationToken);
        }

    }
}
