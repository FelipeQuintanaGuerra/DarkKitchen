using System.Linq.Expressions;
using Domain;
using IRepository;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(DbContext context) : base(context)
    {
    }

    public override Product? GetBy(Expression<Func<Product, bool>> predicate)
    {
        return _context.Set<Product>()
            .Include(p => p.Category)
            .Include(p => p.CommercialLine)
            .Include(p => p.Pictures)
            .FirstOrDefault(predicate);
    }

    public override List<Product> GetAllBy(Expression<Func<Product, bool>> predicate, int page = 1, int pageSize = 20)
    {
        if (page < 1)
            throw new ArgumentException("La página debe ser mayor o igual a 1.");

        if (pageSize < 1)
            throw new ArgumentException("El tamaño de página debe ser mayor o igual a 1.");

        return _context.Set<Product>()
            .Include(p => p.Category)
            .Include(p => p.CommercialLine)
            .Include(p => p.Pictures)
            .Where(predicate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public override void Update(Product product)
    {
        _context.SaveChanges();
    }

    public void ReplacePictures(Guid productId, List<ProductPicture> newPictures)
    {
        var trackedPictures = _context.ChangeTracker
            .Entries<ProductPicture>()
            .ToList();

        foreach (var entry in trackedPictures)
        {
            entry.State = EntityState.Detached;
        }

        var oldPictures = _context.Set<ProductPicture>()
            .Where(p => EF.Property<Guid>(p, "ProductId") == productId)
            .ToList();

        _context.Set<ProductPicture>().RemoveRange(oldPictures);
        _context.SaveChanges();

        foreach (var picture in newPictures)
        {
            _context.Set<ProductPicture>().Add(picture);
            _context.Entry(picture).Property("ProductId").CurrentValue = productId;
        }

        _context.SaveChanges();
    }
}
