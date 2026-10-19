using BusinessLogic.Mappers;
using Domain;
using DTOs.AuditDTOs;
using DTOs.CommercialLineDTOs;
using DTOs.PromotionsDTOs;
using IBussinesLogic;
using IRepository;

namespace BusinessLogic;

public class PromotionService : IPromotionService
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IGenericRepository<CommercialLine> _commercialLineRepository;
    private readonly IGenericRepository<Product> _productRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IAuditLoggerService _auditLoggerService;

    public PromotionService(
        IPromotionRepository promotionRepository,
        IGenericRepository<CommercialLine> commercialLineRepository,
        ISessionRepository sessionRepository,
        IGenericRepository<Product> productRepository,
        IAuditLoggerService auditLoggerService)
    {
        _promotionRepository = promotionRepository;
        _commercialLineRepository = commercialLineRepository;
        _sessionRepository = sessionRepository;
        _productRepository = productRepository;
        _auditLoggerService = auditLoggerService;
    }

    public PromotionSummaryDto CreatePromotion(CreatePromotionRequestDto request, Guid token)
    {
        ValidateCreatePromotionRequest(request);

        var commercialLine = GetCommercialLineOrThrow(request.CommercialLineId);
        var products = GetProductsOrThrow(request.ProductIds);

        var promotion = BuildPromotion(request, commercialLine, products);

        _promotionRepository.Add(promotion);

        var username = GetUsernameByToken(token);
        LogPromotionChange(username, "Alta", promotion);

        return PromotionMapper.ToSummaryDto(promotion);
    }

    public PromotionSummaryDto UpdatePromotion(Guid id, UpdatePromotionRequestDto request, Guid token)
    {
        ValidateUpdatePromotionRequest(request);

        var promotion = GetPromotionOrThrow(id);

        UpdatePromotionFields(promotion, request);

        _promotionRepository.Update(promotion);

        var username = GetUsernameByToken(token);
        LogPromotionChange(username, "Modificación", promotion);

        return PromotionMapper.ToSummaryDto(promotion);
    }

    public PromotionSummaryDto AssociateProduct(Guid promotionId, Guid productId, Guid token)
    {
        var promotion = GetPromotionOrThrow(promotionId);
        var product = GetProductOrThrow(productId);

        promotion.AddProduct(product);
        _promotionRepository.Update(promotion);

        var username = GetUsernameByToken(token);
        LogPromotionChange(username, "Asociación", promotion);

        return PromotionMapper.ToSummaryDto(promotion);
    }

    public PromotionSummaryDto RemoveProduct(Guid promotionId, Guid productId, Guid token)
    {
        var promotion = GetPromotionOrThrow(promotionId);
        var product = GetProductOrThrow(productId);

        promotion.RemoveProduct(product);
        _promotionRepository.Update(promotion);

        var username = GetUsernameByToken(token);
        LogPromotionChange(username, "Desasociación", promotion);

        return PromotionMapper.ToSummaryDto(promotion);
    }

    public List<PromotionSummaryDto> GetActivePromotions(SearchPromotionRequestDto dto)
    {
        var promotions = _promotionRepository.GetAllBy(
            promotion =>
                (!dto.Date.HasValue || (promotion.StartDate <= dto.Date.Value && promotion.EndDate >= dto.Date.Value)) &&
                (!dto.CommercialLineId.HasValue || promotion.CommercialLine.Id == dto.CommercialLineId.Value) &&
                (!dto.ProductId.HasValue || promotion.Products.Any(product => product.Id == dto.ProductId.Value)),
            dto.Page,
            dto.PageSize
        );

        return promotions.Select(PromotionMapper.ToSummaryDto).ToList();
    }

    public Promotion? GetBestPromotionForProduct(Guid productId, DateTime date)
    {
        var validPromotions = _promotionRepository.GetAllBy(p =>
            p.Products.Any(prod => prod.Id == productId) &&
            p.StartDate <= date &&
            p.EndDate >= date
        );

        if (!validPromotions.Any())
            return null;

        return validPromotions
            .OrderByDescending(p => p.Discount)
            .First();
    }

    public IEnumerable<CommercialLineDto> GetCommercialLines() =>
        _commercialLineRepository
            .GetAllBy(cl => true, page: 1, pageSize: int.MaxValue)
            .Select(cl => new CommercialLineDto
            {
                Id = cl.Id,
                Name =
                cl.Name
            });

    private string GetUsernameByToken(Guid token)
    {
        var session = _sessionRepository.GetByToken(token);

        if (session == null)
            throw new UnauthorizedAccessException("Sesión inválida.");

        return session.User.Email;
    }

    private void LogPromotionChange(string username, string action, Promotion promotion)
    {
        _auditLoggerService.LogAsync(new CreateLogDto
        {
            Username = username,
            Action = action,
            EntityName = "PROMOCION",
            EntityId = promotion.Id,
            EntityDisplayName = promotion.Name
        }).GetAwaiter().GetResult();
    }

    private static void ValidateCreatePromotionRequest(CreatePromotionRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
    }

    private static void ValidateUpdatePromotionRequest(UpdatePromotionRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
    }

    private static void UpdatePromotionFields(Promotion promotion, UpdatePromotionRequestDto request)
    {
        promotion.Update(request.Name, request.StartDate, request.EndDate, request.Discount);
    }

    private List<Product> GetProductsOrThrow(IEnumerable<Guid> productIds)
    {
        return productIds
            .Select(GetProductOrThrow)
            .ToList();
    }

    private static Promotion BuildPromotion(
        CreatePromotionRequestDto request,
        CommercialLine commercialLine,
        List<Product> products)
    {
        return new Promotion(
            request.Name,
            request.StartDate,
            request.EndDate,
            request.Discount,
            commercialLine,
            products
        );
    }

    private CommercialLine GetCommercialLineOrThrow(Guid id)
    {
        var commercialLine = _commercialLineRepository.GetBy(x => x.Id == id);

        if (commercialLine == null)
            throw new KeyNotFoundException("No existe una línea comercial con ese id.");

        return commercialLine;
    }

    private Product GetProductOrThrow(Guid id)
    {
        var product = _productRepository.GetBy(x => x.Id == id);

        if (product == null)
            throw new KeyNotFoundException($"No existe un producto con id {id}.");

        return product;
    }

    private Promotion GetPromotionOrThrow(Guid id)
    {
        var promotion = _promotionRepository.GetBy(x => x.Id == id);

        if (promotion == null)
            throw new KeyNotFoundException("No existe una promoción con ese id.");

        return promotion;
    }
}