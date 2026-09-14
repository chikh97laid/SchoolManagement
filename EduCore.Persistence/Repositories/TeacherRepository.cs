using EduCore.Domain.Entities;
using EduCore.Domain.Interfaces.Repositories;
using EduCore.Persistence.Context;

namespace EduCore.Persistence.Repositories
{
    public class TeacherRepository
        : GenericRepository<Teacher>, ITeacherRepository
    {
        public TeacherRepository(ApplicationDbContext context)
            : base(context)
        {
        }
    }
}