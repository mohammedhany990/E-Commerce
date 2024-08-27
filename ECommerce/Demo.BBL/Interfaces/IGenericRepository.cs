using System.Linq.Expressions;

namespace Demo.BBL.Interfaces
{
    public interface IGenericRepository<T>
    {
        Task AddAsync(T item);
        void Update(T item);
        void Delete(T item);
        void DeleteRange(IEnumerable<T> items);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetWithFilterAsync(Expression<Func<T, bool>> filter=null, string? includeProperty = null, bool tracked = false);
        public IEnumerable<T> GetAllWithFilter(Expression<Func<T, bool>>?filter=null, string? includeProperty = null);

    }
}
