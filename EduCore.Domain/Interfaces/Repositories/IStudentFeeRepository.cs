using EduCore.Domain.Entities;

namespace EduCore.Domain.Interfaces.Repositories;
public interface IStudentFeeRepository
    : IGenericRepository<StudentFee>
{
    Task<StudentFee?> GetForUpdateAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<StudentFee?> GetWithDetailsAsync(
    int id,
    CancellationToken cancellationToken = default);

    Task<IEnumerable<StudentFee>> GetAllWithDetailsAsync(
        CancellationToken cancellationToken = default);

    Task<IEnumerable<StudentFee>> GetByStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<StudentFee>> GetByEnrollmentAsync(
        int enrollmentId,
        CancellationToken cancellationToken = default);
}