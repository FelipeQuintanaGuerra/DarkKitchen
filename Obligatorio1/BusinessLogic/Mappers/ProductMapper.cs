using Domain;
using DTOs.ProductsDTOs;

namespace BusinessLogic.Mappers;

public static class ProductMapper
{
    public static ProductSummaryDto ToSummaryDto(Product product)
    {
        return new ProductSummaryDto
        {
            Id = product.Id,
            Code = product.Code,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CommercialLineName = product.CommercialLine.Name,
            CategoryName = product.Category.Name,
            ImageUrls = product.Pictures
                .Select(p => BuildImageUrl(p.FileName))
                .ToList(),
            IsActive = product.IsActive
        };
    }

    private static string BuildImageUrl(string fileName)
    {
        return $"/images/products/{fileName}";
    }
}