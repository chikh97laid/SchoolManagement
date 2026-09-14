
namespace EduCore.Domain.Interfaces.Repositories
{
    public interface IUnitOfWork 
    {
        IStudentRepository Students { get; }
        ITeacherRepository Teachers { get; }
        IGroupRepository Groups { get; }
        IEnrollmentRepository Enrollments { get; }
        IAttendanceRepository Attendances { get; }
        IStudentFeeRepository StudentFees { get; }
        IPaymentRepository Payments { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    }
}
