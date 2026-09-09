using Domain;
using DTOs.CommercialLineDTOs;
using DTOs.PromotionsDTOs;

namespace IBussinesLogic;

public interface IPromotionService
{
    PromotionSummaryDto CreatePromotion(CreatePromotionRequestDto request, Guid token);
    PromotionSummaryDto UpdatePromotion(Guid id, UpdatePromotionRequestDto request, Guid token);
    PromotionSummaryDto AssociateProduct(Guid promotionId, Guid productId, Guid token);
    PromotionSummaryDto RemoveProduct(Guid promotionId, Guid productId, Guid token);
    List<PromotionSummaryDto> GetActivePromotions(SearchPromotionRequestDto request);
    Promotion? GetBestPromotionForProduct(Guid productId, DateTime date);
    IEnumerable<CommercialLineDto> GetCommercialLines();

}