using BusinessLogic.Mappers;
using Domain;
using DTOs.AuditDTOs;
using DTOs.ProductsDTOs;
using IBussinesLogic;
using IRepository;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic;

public class ProductService : IProductService
{
    private const int MaxImageSizeInBytes = 500 * 1024;

    private readonly IProductRepository _productRepository;
    private readonly IGenericRepository<CommercialLine> _commercialLineRepository;
    private readonly IGenericRepository<Category> _categoryRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IImageStorageService _imageStorageService;
    private readonly IAuditLoggerService _auditLoggerService;

    public ProductService(
        IProductRepository productRepository,
        IGenericRepository<CommercialLine> commercialLineRepository,
        IGenericRepository<Category> categoryRepository,
        ISessionRepository sessionRepository,
        IImageStorageService imageStorageService,
        IAuditLoggerService auditLoggerService)
    {
        _productRepository = productRepository;
        _commercialLineRepository = commercialLineRepository;
        _categoryRepository = categoryRepository;
        _sessionRepository = sessionRepository;
        _imageStorageService = imageStorageService;
        _auditLoggerService = auditLoggerService;
    }

    public ProductSummaryDto CreateProduct(CreateProductRequestDto request, Guid token)
    {
        ValidateCreateProductRequest(request);
        EnsureProductCodeIsAvailable(request.Code);

        var commercialLine = GetCommercialLineOrCreate(request.CommercialLineName);
        var category = GetCategoryOrCreate(request.CategoryName);
        var pictures = CreatePictures(request.Images);

        var product = CreateProductFromRequest(request, commercialLine, category, pictures);

        _productRepository.Add(product);

        var username = GetUsernameByToken(token);
        LogProductChange(username, "Alta", product);

        return ProductMapper.ToSummaryDto(product);
    }

    public ProductSummaryDto UpdateProduct(Guid id, UpdateProductRequestDto request, Guid token)
    {
        ValidateUpdateProductRequest(request);

        var product = GetProductOrThrow(id);
        var commercialLine = GetCommercialLineOrCreate(request.CommercialLineName);
        var category = GetCategoryOrCreate(request.CategoryName);

        UpdateProductFields(product, request, commercialLine, category);

        _productRepository.Update(product);

        if (request.Images != null && request.Images.Count > 0)
        {
            var pictures = CreatePictures(request.Images);
            _productRepository.ReplacePictures(product.Id, pictures);
            product.Pictures = pictures;
        }

        var username = GetUsernameByToken(token);
        LogProductChange(username, "Modificación", product);

        return ProductMapper.ToSummaryDto(product);
    }

    public List<ProductSummaryDto> SearchProducts(SearchProductRequestDto dto)
    {
        var products = _productRepository.GetAllBy(
            product =>
            (string.IsNullOrWhiteSpace(dto.Name) || product.Name.Contains(dto.Name)) &&
            ((string.IsNullOrWhiteSpace(dto.CommercialLineId.ToString()) || product.CommercialLine.Id == dto.CommercialLineId)) &&
            (string.IsNullOrWhiteSpace(dto.CategoryId.ToString()) || product.Category.Id == dto.CategoryId),
            dto.Page,
            dto.PageSize
        );

        return products.Select(ProductMapper.ToSummaryDto).ToList();
    }

    public ProductSummaryDto ChangeProductActivation(Guid id, Guid token)
    {
        var product = GetProductOrThrow(id);
        product.ChangeActivation();

        _productRepository.Update(product);

        var username = GetUsernameByToken(token);
        var action = product.IsActive ? "Modificación" : "Eliminación";
        LogProductChange(username, action, product);

        return ProductMapper.ToSummaryDto(product);
    }

    public ProductSummaryDto GetProductById(Guid id)
    {
        var product = GetProductOrThrow(id);
        return ProductMapper.ToSummaryDto(product);
    }

    private string GetUsernameByToken(Guid token)
    {
        var session = _sessionRepository.GetByToken(token);

        if (session == null)
            throw new UnauthorizedAccessException("Sesión inválida.");

        return session.User.Email;
    }

    private void LogProductChange(string username, string action, Product product)
    {
        _auditLoggerService.LogAsync(new CreateLogDto
        {
            Username = username,
            Action = action,
            EntityName = "PRODUCTO",
            EntityId = product.Id,
            EntityDisplayName = product.Name
        }).GetAwaiter().GetResult();
    }

    private Product GetProductOrThrow(Guid id)
    {
        var product = _productRepository.GetBy(x => x.Id == id);

        if (product == null)
            throw new KeyNotFoundException("No existe un producto con ese id.");

        return product;
    }

    private CommercialLine GetCommercialLineOrCreate(string name)
    {
        var commercialLine = _commercialLineRepository.GetBy(x => x.Name == name);

        if (commercialLine != null)
            return commercialLine;

        commercialLine = new CommercialLine(name);
        _commercialLineRepository.Add(commercialLine);

        return commercialLine;
    }

    private Category GetCategoryOrCreate(string name)
    {
        var category = _categoryRepository.GetBy(x => x.Name == name);

        if (category != null)
            return category;

        category = new Category(name);
        _categoryRepository.Add(category);

        return category;
    }

    private static void ValidateCreateProductRequest(CreateProductRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
    }

    private void EnsureProductCodeIsAvailable(string code)
    {
        var existing = _productRepository.GetBy(x => x.Code == code);

        if (existing != null)
            throw new ConflictException("Ya existe un producto con ese código.");
    }

    private List<ProductPicture> CreatePictures(List<IFormFile> images)
    {
        ValidateImages(images);

        var imageUrls = _imageStorageService.SaveProductImages(images);

        return imageUrls
            .Select((url, index) =>
                new ProductPicture(url, (int)(images[index].Length / 1024)))
            .ToList();
    }

    private static void ValidateImages(List<IFormFile> images)
    {
        if (images == null)
            throw new ArgumentNullException("La lista de imágenes no existe.");

        if (images.Count < 1)
            throw new ArgumentException("El producto debe tener al menos una imagen.");

        if (images.Count > 3)
            throw new ArgumentException("El producto puede tener hasta 3 imágenes.");

        foreach (var image in images)
        {
            if (image.Length > MaxImageSizeInBytes)
                throw new ArgumentException("La imagen no puede superar los 500 KB.");

            var extension = Path.GetExtension(image.FileName).ToLower();

            if (extension != ".jpg")
                throw new ArgumentException("La imagen debe estar en formato jpg.");
        }
    }

    private static Product CreateProductFromRequest(
        CreateProductRequestDto request,
        CommercialLine commercialLine,
        Category category,
        List<ProductPicture> pictures)
    {
        return new Product(
            request.Code,
            request.Name,
            request.Description,
            commercialLine,
            category,
            pictures,
            request.Price
        );
    }

    private static void ValidateUpdateProductRequest(UpdateProductRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
    }

    private static void UpdateProductFields(
        Product product,
        UpdateProductRequestDto request,
        CommercialLine commercialLine,
        Category category)
    {
        product.Update(request.Name, request.Description, request.Price, commercialLine, category);
    }
}
