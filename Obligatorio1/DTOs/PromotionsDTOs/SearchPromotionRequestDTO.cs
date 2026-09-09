namespace DTOs.PromotionsDTOs;

public class SearchPromotionRequestDto
{
    public DateTime? Date { get; set; }
    public Guid? CommercialLineId { get; set; }
    public Guid? ProductId { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}