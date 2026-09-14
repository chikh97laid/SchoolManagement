using EduCore.Domain.Entities;

namespace EduCore.Domain.Interfaces.Repositories
{
    public interface IStudentRepository : IGenericRepository<Student>
    {
        Task<int> GetNextStudentSequenceAsync(CancellationToken cancellationToken);
    }
}
