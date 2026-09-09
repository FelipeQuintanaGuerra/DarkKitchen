using Microsoft.AspNetCore.Http;

namespace DTOs.ProductsDTOs;

public class UpdateProductRequestDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string CommercialLineName { get; set; }
    public string CategoryName { get; set; }
    public List<IFormFile> Images { get; set; } = new();
}