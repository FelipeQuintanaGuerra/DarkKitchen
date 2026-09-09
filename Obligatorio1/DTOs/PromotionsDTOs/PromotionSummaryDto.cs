namespace DTOs.PromotionsDTOs;

public class PromotionSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Discount { get; set; }
    public string CommercialLineName { get; set; }
    public List<string> ProductNames { get; set; } = new();
}