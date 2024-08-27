using System.Linq.Expressions;
using Demo.BBL.Interfaces;
using Demo.DAL.Data.Contexts;
using Demo.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo.BBL.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ECommerceDbContext _dbContext;

        public GenericRepository(ECommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(T item)
        {
            await _dbContext.Set<T>().AddAsync(item);
        }

        public void Update(T item)
        {
            _dbContext.Set<T>().Update(item);

        }

        public void Delete(T item)
        {
            _dbContext.Set<T>().Remove(item);

        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            if (typeof(T) == typeof(Product))
            {
                return (IEnumerable<T>) await _dbContext.Products.Include(C => C.Category).ToListAsync();
            }
            return await _dbContext.Set<T>().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public Task<T?> GetWithFilterAsync(Expression<Func<T, bool>> filter, string? includeProperty = null,bool tracked = false)
        {
            IQueryable<T> query = _dbContext.Set<T>();
            query = tracked ? query.AsNoTracking() : query;
            query = query.Where(filter);
            if (!string.IsNullOrEmpty(includeProperty))
            {
                foreach (var property in
                         includeProperty.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }
            return query.FirstOrDefaultAsync();
        }

        public IEnumerable<T> GetAllWithFilter(Expression<Func<T, bool>>? filter, string ? includeProperty = null)
        {
            IQueryable<T> query = _dbContext.Set<T>();
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrEmpty(includeProperty))
            {
                foreach (var property in 
                         includeProperty.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }

            return query.ToList();
        }


        public void DeleteRange(IEnumerable<T> items)
        {
            _dbContext.Set<T>().RemoveRange(items);
        }
    }
}
