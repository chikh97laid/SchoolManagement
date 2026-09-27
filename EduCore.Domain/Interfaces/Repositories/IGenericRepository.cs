using System.Linq.Expressions;

namespace EduCore.Domain.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object?>>[] includes);

        Task<IEnumerable<T>> GetAllAsync(
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object?>>[] includes);

        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync<TKey>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TKey>> orderBy,
            int pageNumber,
            int pageSize,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object?>>[] includes);



        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, bool trackChanges = false,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object?>>[] includes);    
        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        void Update(T entity);

        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    }
}