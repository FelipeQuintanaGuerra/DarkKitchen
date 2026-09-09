using Domain.Enums;
using DTOs.PromotionsDTOs;
using IBussinesLogic;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;

namespace WebApi.Controllers;

[ApiController]
[Route("api/promotions")]
public class PromotionController(IPromotionService promotionService) : ControllerBase
{
    [HttpGet]
    [AuthorizationFilter([Role.Admin, Role.Client])]
    public IActionResult GetActivePromotions([FromQuery] SearchPromotionRequestDto dto)
    {
        var promotions = promotionService.GetActivePromotions(dto);
        return Ok(promotions);
    }

    [HttpPost]
    [AuthorizationFilter([Role.Admin])]
    public IActionResult CreatePromotion([FromBody] CreatePromotionRequestDto dto)
    {
        var promotion = promotionService.CreatePromotion(dto, GetToken());
        return StatusCode(201, promotion);
    }

    [HttpPut("{id}")]
    [AuthorizationFilter([Role.Admin])]
    public IActionResult UpdatePromotion(Guid id, [FromBody] UpdatePromotionRequestDto dto)
    {
        var promotion = promotionService.UpdatePromotion(id, dto, GetToken());
        return Ok(promotion);
    }

    [HttpPost("{promotionId}/products")]
    [AuthorizationFilter([Role.Admin])]
    public IActionResult AddProductToPromotion(Guid promotionId, [FromBody] AddProductToPromotionDto dto)
    {
        promotionService.AssociateProduct(promotionId, dto.ProductId, GetToken());
        return NoContent();
    }

    [HttpDelete("{promotionId}/products")]
    [AuthorizationFilter([Role.Admin])]
    public IActionResult RemoveProductFromPromotion(Guid promotionId, [FromQuery] Guid productId)
    {
        promotionService.RemoveProduct(promotionId, productId, GetToken());
        return NoContent();
    }

    private Guid GetToken()
    {
        var tokenString = Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(tokenString) || !Guid.TryParse(tokenString, out var token))
            throw new UnauthorizedAccessException("Token inválido.");

        return token;
    }

    [HttpGet("commercial-lines")]
    [AuthorizationFilter([Role.Admin])]
    public IActionResult GetCommercialLines() =>
        Ok(promotionService.GetCommercialLines());
}