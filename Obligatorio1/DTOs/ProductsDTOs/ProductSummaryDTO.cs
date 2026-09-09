namespace DTOs.ProductsDTOs;

public class ProductSummaryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string CommercialLineName { get; set; }
    public string CategoryName { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public bool IsActive { get; set; }
}
