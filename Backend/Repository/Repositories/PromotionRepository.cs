using System.Linq.Expressions;
using Domain;
using IRepository;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public class PromotionRepository : GenericRepository<Promotion>, IPromotionRepository
{
    public PromotionRepository(DbContext context) : base(context)
    {
    }

    public override Promotion? GetBy(Expression<Func<Promotion, bool>> predicate)
    {
        return _context.Set<Promotion>()
            .Include(p => p.Products)
            .Include(p => p.CommercialLine)
            .FirstOrDefault(predicate);
    }

    public override List<Promotion> GetAllBy(Expression<Func<Promotion, bool>> predicate, int page = 1, int pageSize = 20)
    {
        if (page < 1)
            throw new ArgumentException("La página debe ser mayor o igual a 1.");

        if (pageSize < 1)
            throw new ArgumentException("El tamaño de página debe ser mayor o igual a 1.");

        return _context.Set<Promotion>()
            .Include(p => p.Products)
            .Include(p => p.CommercialLine)
            .Where(predicate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }
}
