namespace DTOs.PromotionsDTOs;

public class UpdatePromotionRequestDto
{
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Discount { get; set; }
}