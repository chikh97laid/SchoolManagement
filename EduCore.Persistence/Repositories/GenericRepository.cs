using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using EduCore.Persistence.Context;
using EduCore.Domain.Interfaces.Repositories;

namespace EduCore.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;

        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync( bool trackChanges = false, 
            CancellationToken cancellationToken = default, params Expression<Func<T, object?>>[] includes)
        {
            IQueryable<T> query = trackChanges
                ? _dbSet
                : _dbSet.AsNoTracking();

            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object?>>[] includes)
        {
            IQueryable<T> query = trackChanges
                ? _dbSet
                : _dbSet.AsNoTracking();

            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            bool trackChanges = false,
            CancellationToken cancellationToken = default,
            params Expression<Func<T, object?>>[] includes) 
        {
            IQueryable<T> query = trackChanges
                ? _dbSet
                : _dbSet.AsNoTracking();

            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }   
            }

            return await query.Where(predicate).ToListAsync(cancellationToken);
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public async Task<bool> ExistsAsync(
            Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }
    }
}