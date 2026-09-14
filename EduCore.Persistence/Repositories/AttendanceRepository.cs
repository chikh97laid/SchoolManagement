using EduCore.Domain.Entities;
using EduCore.Domain.Interfaces.Repositories;
using EduCore.Persistence.Context;



namespace EduCore.Persistence.Repositories
{
    public class AttendanceRepository
        : GenericRepository<Attendance>, IAttendanceRepository
    {
        public AttendanceRepository(ApplicationDbContext context)
            : base(context)
        {
        }
    }
}