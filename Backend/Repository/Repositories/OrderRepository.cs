using System.Linq.Expressions;
using Domain;
using Microsoft.EntityFrameworkCore;
using IRepository;

namespace Repository;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(DbContext context) : base(context)
    {
    }

    public override Order? GetBy(Expression<Func<Order, bool>> predicate)
    {
        var order = _context.Set<Order>()
            .Include(o => o.Customer)
            .Include(o => o.ShippingType)
            .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
                    .ThenInclude(p => p.Pictures)
            .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
                    .ThenInclude(p => p.CommercialLine)
            .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefault(predicate);

        return order == null ? null : RestoreOrder(order);
    }

    public override List<Order> GetAllBy(Expression<Func<Order, bool>> predicate, int page = 1, int pageSize = 20)
    {
        if (page < 1)
            throw new ArgumentException("La página debe ser mayor o igual a 1.");
        if (pageSize < 1)
            throw new ArgumentException("El tamaño de página debe ser mayor o igual a 1.");

        return _context.Set<Order>()
            .Include(o => o.Customer)
            .Include(o => o.ShippingType)
            .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
                    .ThenInclude(p => p.Pictures)
            .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
                    .ThenInclude(p => p.CommercialLine)
            .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
                    .ThenInclude(p => p.Category)
            .Where(predicate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsEnumerable()
            .Select(RestoreOrder)
            .ToList();
    }

    public List<Order> GetAllUnpagedBy(Expression<Func<Order, bool>> predicate)
    {
        return _context.Set<Order>()
            .Include(o => o.Customer)
            .Include(o => o.ShippingType)
            .Include(o => o.Lines)
            .ThenInclude(l => l.Product)
            .ThenInclude(p => p.Pictures)
            .Include(o => o.Lines)
            .ThenInclude(l => l.Product)
            .ThenInclude(p => p.CommercialLine)
            .Include(o => o.Lines)
            .ThenInclude(l => l.Product)
            .ThenInclude(p => p.Category)
            .Where(predicate)
            .AsEnumerable()
            .Select(RestoreOrder)
            .ToList();
    }

    private static Order RestoreOrder(Order order)
    {
        order.RestoreState(order.Status);
        return order;
    }
}
