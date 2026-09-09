using System.Linq.Expressions;

namespace IRepository;

public interface IGenericRepository<T> where T : class
{
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
    T? GetBy(Expression<Func<T, bool>> predicate);
    List<T> GetAllBy(Expression<Func<T, bool>> predicate, int page = 1, int pageSize = 20);
}