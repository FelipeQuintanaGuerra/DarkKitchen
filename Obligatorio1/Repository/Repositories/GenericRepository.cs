using System.Linq.Expressions;
using IRepository;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly DbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual void Add(T entity)
    {
        _dbSet.Add(entity);
        _context.SaveChanges();
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
        _context.SaveChanges();
    }

    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
        _context.SaveChanges();
    }

    public virtual T? GetBy(Expression<Func<T, bool>> predicate)
    {
        return _dbSet.FirstOrDefault(predicate);
    }

    public virtual List<T> GetAllBy(Expression<Func<T, bool>> predicate, int page = 1, int pageSize = 20)
    {
        if (page < 1)
            throw new ArgumentException("La página debe ser mayor o igual a 1.");

        if (pageSize < 1)
            throw new ArgumentException("El tamaño de página debe ser mayor o igual a 1.");

        return _dbSet
            .Where(predicate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }
}