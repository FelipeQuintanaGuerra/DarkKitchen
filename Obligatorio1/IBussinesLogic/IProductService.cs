using DTOs.ProductsDTOs;

namespace IBussinesLogic;

public interface IProductService
{
    ProductSummaryDto CreateProduct(CreateProductRequestDto request, Guid token);
    ProductSummaryDto UpdateProduct(Guid id, UpdateProductRequestDto request, Guid token);
    List<ProductSummaryDto> SearchProducts(SearchProductRequestDto request);
    ProductSummaryDto GetProductById(Guid id);
    ProductSummaryDto ChangeProductActivation(Guid id, Guid token);
}