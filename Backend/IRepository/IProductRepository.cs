using Domain;

namespace IRepository;

public interface IProductRepository : IGenericRepository<Product>
{
    void ReplacePictures(Guid productId, List<ProductPicture> newPictures);
}