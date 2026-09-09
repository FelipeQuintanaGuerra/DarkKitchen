using Domain;
using DTOs.PromotionsDTOs;

namespace BusinessLogic.Mappers;

public static class PromotionMapper
{
    public static PromotionSummaryDto ToSummaryDto(Promotion promotion)
    {
        return new PromotionSummaryDto
        {
            Id = promotion.Id,
            Name = promotion.Name,
            StartDate = promotion.StartDate,
            EndDate = promotion.EndDate,
            Discount = promotion.Discount,
            CommercialLineName = promotion.CommercialLine.Name,
            ProductNames = promotion.Products.Select(p => p.Name).ToList()
        };
    }
}