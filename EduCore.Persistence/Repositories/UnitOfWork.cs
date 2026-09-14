using EduCore.Domain.Interfaces.Repositories;
using EduCore.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace EduCore.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public IStudentRepository Students { get; }
        public ITeacherRepository Teachers { get; }
        public IGroupRepository Groups { get; }
        public IEnrollmentRepository Enrollments { get; }
        public IAttendanceRepository Attendances { get; }
        public IStudentFeeRepository StudentFees { get; }
        public IPaymentRepository Payments { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Students = new StudentRepository(_context);
            Teachers = new TeacherRepository(_context);
            Groups = new GroupRepository(_context);
            Enrollments = new EnrollmentRepository(_context);
            Attendances = new AttendanceRepository(_context);
            StudentFees = new StudentFeeRepository(_context);
            Payments = new PaymentRepository(_context);
        }

        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database
                .BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
                throw new InvalidOperationException(
                    "No active transaction.");

            await _transaction.CommitAsync(cancellationToken);

            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is null)
                return;

            await _transaction.RollbackAsync(cancellationToken);

            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}