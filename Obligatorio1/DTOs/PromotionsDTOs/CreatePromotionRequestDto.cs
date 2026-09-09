namespace DTOs.PromotionsDTOs;

public class CreatePromotionRequestDto
{
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Discount { get; set; }
    public Guid CommercialLineId { get; set; }
    public List<Guid> ProductIds { get; set; } = new();
}