namespace DTOs.ProductsDTOs;

public class SearchProductRequestDto
{
    public string? Name { get; set; }
    public Guid? CommercialLineId { get; set; }
    public Guid? CategoryId { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}