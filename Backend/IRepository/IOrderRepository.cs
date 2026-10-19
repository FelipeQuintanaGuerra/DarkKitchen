using System.Linq.Expressions;
using Domain;

namespace IRepository;

public interface IOrderRepository : IGenericRepository<Order>
{
    List<Order> GetAllUnpagedBy(Expression<Func<Order, bool>> predicate);
}