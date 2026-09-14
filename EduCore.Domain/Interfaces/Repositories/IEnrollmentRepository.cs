
using EduCore.Domain.Entities;

namespace EduCore.Domain.Interfaces.Repositories
{
    public interface IEnrollmentRepository
        : IGenericRepository<Enrollment>
    {
        Task<Enrollment?> GetWithDetailsAsync(
        int id,
        CancellationToken cancellationToken = default);

        Task<IEnumerable<Enrollment>> GetAllWithDetailsAsync(
        CancellationToken cancellationToken = default);
    }
}