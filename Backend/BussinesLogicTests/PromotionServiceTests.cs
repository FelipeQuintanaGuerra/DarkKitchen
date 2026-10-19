using BusinessLogic;
using DTOs.PromotionsDTOs;
using IRepository;
using Moq;
using IBussinesLogic;
using Domain;
using System.Linq.Expressions;
using DTOs.AuditDTOs;

namespace BussinesLogicTests;

[TestClass]
public class PromotionServiceTests
{
    private Mock<IPromotionRepository> _mockPromotionRepo = null!;
    private Mock<IGenericRepository<CommercialLine>> _mockCommercialLineRepo = null!;
    private Mock<IGenericRepository<Product>> _mockProductRepo = null!;
    private Mock<ISessionRepository> _mockSessionRepository = null!;
    private Mock<IAuditLoggerService> _mockAuditLoggerService = null!;
    private IPromotionService _service = null!;

    private CommercialLine _line = null!;
    private Category _category = null!;
    private Product _product1 = null!;
    private Product _product2 = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockPromotionRepo = new Mock<IPromotionRepository>(MockBehavior.Strict);
        _mockCommercialLineRepo = new Mock<IGenericRepository<CommercialLine>>(MockBehavior.Strict);
        _mockProductRepo = new Mock<IGenericRepository<Product>>(MockBehavior.Strict);

        _mockSessionRepository = new Mock<ISessionRepository>(MockBehavior.Strict);
        _mockAuditLoggerService = new Mock<IAuditLoggerService>(MockBehavior.Strict);

        _service = new PromotionService(
            _mockPromotionRepo.Object,
            _mockCommercialLineRepo.Object,
            _mockSessionRepository.Object,
            _mockProductRepo.Object,
            _mockAuditLoggerService.Object
        );

        _line = new CommercialLine("Minutas clasicas");
        _category = new Category("Fritos");

        _product1 = new Product(
            "PROD1",
            "Producto uno nombre",
            "Descripcion larga y valida del producto",
            _line,
            _category,
            new List<ProductPicture> { new ProductPicture("foto.jpg", 100) }, 100);

        _product2 = new Product(
            "PROD2",
            "Producto dos nombre",
            "Descripcion larga y valida del producto",
            _line,
            _category,
            new List<ProductPicture> { new ProductPicture("foto.jpg", 100) }, 100);
    }

    private void SetupPromotionAdd()
    {
        _mockPromotionRepo
            .Setup(r => r.Add(It.IsAny<Promotion>()));
    }

    private void SetupPromotionUpdate()
    {
        _mockPromotionRepo
            .Setup(r => r.Update(It.IsAny<Promotion>()));
    }

    private Guid SetupValidSession()
    {
        var token = Guid.NewGuid();

        var user = new User(
            "Admin",
            "Test",
            "admin@test.com",
            "099123456",
            "Password1!aaaaaa",
            Domain.Enums.Role.Admin
        );

        var session = new Session(user);

        _mockSessionRepository
            .Setup(r => r.GetByToken(It.IsAny<Guid>()))
            .Returns(session);

        return token;
    }

    private void SetupAuditLog()
    {
        _mockAuditLoggerService
            .Setup(a => a.LogAsync(It.IsAny<CreateLogDto>()))
            .Returns(Task.CompletedTask);
    }

    [TestMethod]
    public void CreatePromotion_NullRequest_ThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(
            () => _service.CreatePromotion(null!, Guid.NewGuid())
        );
    }

    [TestMethod]
    public void CreatePromotion_NonExistingCommercialLine_ThrowsKeyNotFoundException()
    {
        var request = new CreatePromotionRequestDto
        {
            Name = "Promo1",
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(10),
            Discount = 15,
            CommercialLineId = Guid.NewGuid(),
            ProductIds = new List<Guid>()
        };

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns((CommercialLine?)null);

        Assert.ThrowsException<KeyNotFoundException>(
            () => _service.CreatePromotion(request, Guid.NewGuid())
        );
    }

    [TestMethod]
    public void CreatePromotion_ValidRequest_SavesAndReturnsDto()
    {
        var request = new CreatePromotionRequestDto
        {
            Name = "Promo1",
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(10),
            Discount = 15,
            CommercialLineId = _line.Id,
            ProductIds = new List<Guid> { _product1.Id }
        };

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(_line);

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(_product1);

        _mockPromotionRepo
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<Promotion, bool>>>(), 1, 20))
            .Returns(new List<Promotion>());

        SetupPromotionAdd();

        var token = SetupValidSession();
        SetupAuditLog();

        var result = _service.CreatePromotion(request, token);

        _mockPromotionRepo.Verify(r => r.Add(It.IsAny<Promotion>()), Times.Once);
        Assert.AreEqual("Promo1", result.Name);
        Assert.AreEqual(15, result.Discount);
        Assert.IsTrue(result.ProductNames.Contains("Producto uno nombre"));
    }

    [TestMethod]
    public void CreatePromotion_ValidRequest_AuditsPromotionCreation()
    {
        var request = new CreatePromotionRequestDto
        {
            Name = "Promo1",
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(10),
            Discount = 15,
            CommercialLineId = _line.Id,
            ProductIds = new List<Guid> { _product1.Id }
        };

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(_line);

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(_product1);

        _mockPromotionRepo
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<Promotion, bool>>>(), 1, 20))
            .Returns(new List<Promotion>());

        SetupPromotionAdd();

        var token = SetupValidSession();
        SetupAuditLog();

        _service.CreatePromotion(request, token);

        _mockAuditLoggerService.Verify(a => a.LogAsync(
                It.Is<CreateLogDto>(dto =>
                    dto.Username == "admin@test.com" &&
                    dto.Action == "Alta" &&
                    dto.EntityName == "PROMOCION" &&
                    dto.EntityDisplayName == request.Name &&
                    dto.EntityId != Guid.Empty)),
            Times.Once);
    }

    [TestMethod]
    public void CreatePromotion_ProductFromDifferentCommercialLine_ThrowsArgumentException()
    {
        var otherLine = new CommercialLine("Otra linea");

        var productOtherLine = new Product(
            "OTH11",
            "Producto otra linea",
            "Descripcion larga y valida del producto",
            otherLine,
            _category,
            new List<ProductPicture> { new ProductPicture("foto.jpg", 100) },
            100
        );

        var request = new CreatePromotionRequestDto
        {
            Name = "Promo1",
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(10),
            Discount = 15,
            CommercialLineId = _line.Id,
            ProductIds = new List<Guid> { productOtherLine.Id }
        };

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(_line);

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(productOtherLine);

        _mockPromotionRepo
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<Promotion, bool>>>(), 1, 20))
            .Returns(new List<Promotion>());

        Assert.ThrowsException<ArgumentException>(
            () => _service.CreatePromotion(request, Guid.NewGuid())
        );
    }

    [TestMethod]
    public void CreatePromotion_WhenSessionDoesNotExist_ThrowsUnauthorizedAccessException()
    {
        var request = new CreatePromotionRequestDto
        {
            Name = "Promo1",
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(10),
            Discount = 15,
            CommercialLineId = _line.Id,
            ProductIds = new List<Guid> { _product1.Id }
        };

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(_line);

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(_product1);

        _mockPromotionRepo
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<Promotion, bool>>>(), 1, 20))
            .Returns(new List<Promotion>());

        SetupPromotionAdd();

        _mockSessionRepository
            .Setup(r => r.GetByToken(It.IsAny<Guid>()))
            .Returns((Session?)null);

        Assert.ThrowsException<UnauthorizedAccessException>(
            () => _service.CreatePromotion(request, Guid.NewGuid()));
    }

    [TestMethod]
    public void UpdatePromotion_NullRequest_ThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(
            () => _service.UpdatePromotion(Guid.NewGuid(), null!, Guid.NewGuid())
        );
    }

    [TestMethod]
    public void UpdatePromotion_NonExistingPromotion_ThrowsKeyNotFoundException()
    {
        _mockPromotionRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Promotion, bool>>>()))
            .Returns((Promotion?)null);

        var request = new UpdatePromotionRequestDto
        {
            Name = "PromoUpd",
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(10),
            Discount = 10
        };

        Assert.ThrowsException<KeyNotFoundException>(
            () => _service.UpdatePromotion(Guid.NewGuid(), request, Guid.NewGuid())
        );
    }

    [TestMethod]
    public void UpdatePromotion_ValidRequest_UpdatesAndReturnsDto()
    {
        var existingPromo = new Promotion(
            "PromoOrig",
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(10),
            10,
            _line,
            new List<Product>()
        );

        _mockPromotionRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Promotion, bool>>>()))
            .Returns(existingPromo);

        var request = new UpdatePromotionRequestDto
        {
            Name = "PromoUpdt",
            StartDate = DateTime.Now.AddDays(2),
            EndDate = DateTime.Now.AddDays(15),
            Discount = 25
        };
        SetupPromotionUpdate();
        var token = SetupValidSession();
        SetupAuditLog();

        var result = _service.UpdatePromotion(existingPromo.Id, request, token);

        _mockPromotionRepo.Verify(r => r.Update(existingPromo), Times.Once);
        Assert.AreEqual("PromoUpdt", result.Name);
        Assert.AreEqual(25, result.Discount);
    }

    [TestMethod]
    public void UpdatePromotion_ValidRequest_AuditsPromotionModification()
    {
        var existingPromo = new Promotion(
            "PromoOrig",
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(10),
            10,
            _line,
            new List<Product>()
        );

        _mockPromotionRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Promotion, bool>>>()))
            .Returns(existingPromo);

        var request = new UpdatePromotionRequestDto
        {
            Name = "PromoUpdt",
            StartDate = DateTime.Now.AddDays(2),
            EndDate = DateTime.Now.AddDays(15),
            Discount = 25
        };

        SetupPromotionUpdate();

        var token = SetupValidSession();
        SetupAuditLog();

        _service.UpdatePromotion(existingPromo.Id, request, token);

        _mockAuditLoggerService.Verify(a => a.LogAsync(
                It.Is<CreateLogDto>(dto =>
                    dto.Username == "admin@test.com" &&
                    dto.Action == "Modificación" &&
                    dto.EntityName == "PROMOCION" &&
                    dto.EntityId == existingPromo.Id &&
                    dto.EntityDisplayName == request.Name)),
            Times.Once);
    }

    [TestMethod]
    public void UpdatePromotion_InvalidName_ThrowsArgumentException()
    {
        var existingPromo = new Promotion(
            "PromoOrig",
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(10),
            10,
            _line,
            new List<Product>()
        );

        _mockPromotionRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Promotion, bool>>>()))
            .Returns(existingPromo);

        var request = new UpdatePromotionRequestDto
        {
            Name = "AB",
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(10),
            Discount = 10
        };

        Assert.ThrowsException<ArgumentException>(
            () => _service.UpdatePromotion(existingPromo.Id, request, Guid.NewGuid())
        );
    }

    [TestMethod]
    public void UpdatePromotion_InvalidDates_ThrowsArgumentException()
    {
        var existingPromo = new Promotion(
            "PromoOrig",
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(10),
            10,
            _line,
            new List<Product>()
        );

        _mockPromotionRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Promotion, bool>>>()))
            .Returns(existingPromo);

        var request = new UpdatePromotionRequestDto
        {
            Name = "PromoUpdt",
            StartDate = DateTime.Now.AddDays(10),
            EndDate = DateTime.Now.AddDays(1),
            Discount = 10
        };

        Assert.ThrowsException<ArgumentException>(
            () => _service.UpdatePromotion(existingPromo.Id, request, Guid.NewGuid())
        );
    }

    [TestMethod]
    public void UpdatePromotion_InvalidDiscount_ThrowsArgumentException()
    {
        var existingPromo = new Promotion(
            "PromoOrig",
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(10),
            10,
            _line,
            new List<Product>()
        );

        _mockPromotionRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Promotion, bool>>>()))
            .Returns(existingPromo);

        var request = new UpdatePromotionRequestDto
        {
            Name = "PromoUpdt",
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(10),
            Discount = 0
        };

        Assert.ThrowsException<ArgumentException>(
            () => _service.UpdatePromotion(existingPromo.Id, request, Guid.NewGuid())
        );
    }

    [TestMethod]
    public void AssociateProduct_NonExistingPromotion_ThrowsKeyNotFoundException()
    {
        _mockPromotionRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Promotion, bool>>>()))
            .Returns((Promotion?)null);

        Assert.ThrowsException<KeyNotFoundException>(
            () => _service.AssociateProduct(Guid.NewGuid(), _product1.Id, Guid.NewGuid())
        );
    }

    [TestMethod]
    public void AssociateProduct_NonExistingProduct_ThrowsKeyNotFoundException()
    {
        var promo = new Promotion(
            "PromoBase",
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(20),
            10,
            _line,
            new List<Product>()
        );

        _mockPromotionRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Promotion, bool>>>()))
            .Returns(promo);

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns((Product?)null);

        Assert.ThrowsException<KeyNotFoundException>(
            () => _service.AssociateProduct(Guid.NewGuid(), _product1.Id, Guid.NewGuid())
        );
    }

    [TestMethod]
    public void AssociateProduct_ValidProductAndPromotion_AddsProductAndReturnsDto()
    {
        var promo = new Promotion(
            "PromoBase",
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(20),
            10,
            _line,
            new List<Product>()
        );

        _mockPromotionRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Promotion, bool>>>()))
            .Returns(promo);

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(_product1);

        _mockPromotionRepo
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<Promotion, bool>>>(), 1, 20))
            .Returns(new List<Promotion>());

        SetupPromotionUpdate();

        var token = SetupValidSession();
        SetupAuditLog();

        var result = _service.AssociateProduct(promo.Id, _product1.Id, token);

        _mockPromotionRepo.Verify(r => r.Update(promo), Times.Once);
        Assert.IsTrue(result.ProductNames.Contains("Producto uno nombre"));
    }

    [TestMethod]
    public void AssociateProduct_ValidRequest_AuditsPromotionModification()
    {
        var promo = new Promotion(
            "PromoBase",
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(20),
            10,
            _line,
            new List<Product>()
        );

        _mockPromotionRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Promotion, bool>>>()))
            .Returns(promo);

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(_product1);

        _mockPromotionRepo
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<Promotion, bool>>>(), 1, 20))
            .Returns(new List<Promotion>());

        SetupPromotionUpdate();

        var token = SetupValidSession();
        SetupAuditLog();

        _service.AssociateProduct(promo.Id, _product1.Id, token);

        _mockAuditLoggerService.Verify(a => a.LogAsync(
                It.Is<CreateLogDto>(dto =>
                    dto.Username == "admin@test.com" &&
                    dto.Action == "Asociación" &&
                    dto.EntityName == "PROMOCION" &&
                    dto.EntityId == promo.Id &&
                    dto.EntityDisplayName == promo.Name)),
            Times.Once);
    }

    [TestMethod]
    public void RemoveProduct_NonExistingPromotion_ThrowsKeyNotFoundException()
    {
        _mockPromotionRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Promotion, bool>>>()))
            .Returns((Promotion?)null);

        Assert.ThrowsException<KeyNotFoundException>(
            () => _service.RemoveProduct(Guid.NewGuid(), _product1.Id, Guid.NewGuid())
        );
    }

    [TestMethod]
    public void RemoveProduct_NonExistingProduct_ThrowsKeyNotFoundException()
    {
        var promo = new Promotion(
            "PromoBase",
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(20),
            10,
            _line,
            new List<Product>()
        );

        _mockPromotionRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Promotion, bool>>>()))
            .Returns(promo);

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns((Product?)null);

        Assert.ThrowsException<KeyNotFoundException>(
            () => _service.RemoveProduct(promo.Id, Guid.NewGuid(), Guid.NewGuid())
        );
    }

    [TestMethod]
    public void RemoveProduct_ValidProductInPromotion_RemovesAndReturnsDto()
    {
        var promo = new Promotion(
            "PromoBase",
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(20),
            10,
            _line,
            new List<Product> { _product1 }
        );

        _mockPromotionRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Promotion, bool>>>()))
            .Returns(promo);

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(_product1);

        SetupPromotionUpdate();

        var token = SetupValidSession();
        SetupAuditLog();

        var result = _service.RemoveProduct(promo.Id, _product1.Id, token);

        _mockPromotionRepo.Verify(r => r.Update(promo), Times.Once);
        Assert.IsFalse(result.ProductNames.Contains("Producto uno nombre"));
    }

    [TestMethod]
    public void RemoveProduct_ValidRequest_AuditsPromotionModification()
    {
        var promo = new Promotion(
            "PromoBase",
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(20),
            10,
            _line,
            new List<Product> { _product1 }
        );

        _mockPromotionRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Promotion, bool>>>()))
            .Returns(promo);

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(_product1);

        SetupPromotionUpdate();

        var token = SetupValidSession();
        SetupAuditLog();

        _service.RemoveProduct(promo.Id, _product1.Id, token);

        _mockAuditLoggerService.Verify(a => a.LogAsync(
                It.Is<CreateLogDto>(dto =>
                    dto.Username == "admin@test.com" &&
                    dto.Action == "Desasociación" &&
                    dto.EntityName == "PROMOCION" &&
                    dto.EntityId == promo.Id &&
                    dto.EntityDisplayName == promo.Name)),
            Times.Once);
    }

    [TestMethod]
    public void GetActivePromotions_ReturnsFilteredList()
    {
        var promo = new Promotion(
            "PromoActv",
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(20),
            10,
            _line,
            new List<Product> { _product1 }
        );

        _mockPromotionRepo
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<Promotion, bool>>>(), 1, 20))
            .Returns(new List<Promotion> { promo });

        var request = new SearchPromotionRequestDto
        {
            Date = DateTime.Now.AddDays(5),
            CommercialLineId = null,
            ProductId = null
        };

        var result = _service.GetActivePromotions(request);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("PromoActv", result[0].Name);
    }

    [TestMethod]
    public void GetBestPromotionForProduct_WithoutPromotions_ReturnsNull()
    {
        Guid productId = _product1.Id;
        DateTime date = DateTime.Now;

        _mockPromotionRepo
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<Promotion, bool>>>(), 1, 20))
            .Returns(new List<Promotion>());

        var result = _service.GetBestPromotionForProduct(productId, date);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetBestPromotionForProduct_WithOneValidPromotion_ReturnsPromotion()
    {
        DateTime startDate = DateTime.Now.AddDays(1);
        DateTime endDate = DateTime.Now.AddDays(10);
        DateTime queryDate = DateTime.Now.AddDays(5);

        var promotion = new Promotion(
            "Promo",
            startDate,
            endDate,
            15,
            _line,
            new List<Product> { _product1 }
        );

        _mockPromotionRepo
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<Promotion, bool>>>(), 1, 20))
            .Returns(new List<Promotion> { promotion });

        var result = _service.GetBestPromotionForProduct(_product1.Id, queryDate);

        Assert.IsNotNull(result);
        Assert.AreEqual(15, result.Discount);
    }

    [TestMethod]
    public void GetBestPromotionForProduct_WithMultiplePromotions_ReturnsHighestDiscount()
    {
        DateTime queryDate = DateTime.Now.AddDays(5);

        var promo1 = new Promotion(
            "Promo1",
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(10),
            10,
            _line,
            new List<Product> { _product1 }
        );

        var promo2 = new Promotion(
            "Promo2",
            DateTime.Now.AddDays(2),
            DateTime.Now.AddDays(8),
            25,
            _line,
            new List<Product> { _product1 }
        );

        _mockPromotionRepo
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<Promotion, bool>>>(), 1, 20))
            .Returns(new List<Promotion> { promo1, promo2 });

        var result = _service.GetBestPromotionForProduct(_product1.Id, queryDate);

        Assert.IsNotNull(result);
        Assert.AreEqual(25, result.Discount);
    }

    [TestMethod]
    public void GetBestPromotionForProduct_WithPromotionsForOtherProducts_ReturnsNull()
    {
        DateTime queryDate = DateTime.Now.AddDays(5);

        var promotion = new Promotion(
            "Promo",
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(10),
            20,
            _line,
            new List<Product> { _product2 }
        );

        var promotions = new List<Promotion> { promotion };

        _mockPromotionRepo
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<Promotion, bool>>>(), 1, 20))
            .Returns((Expression<Func<Promotion, bool>> predicate,
                    int page,
                    int pageSize) =>
                new List<Promotion> { promotion }
                    .AsQueryable()
                    .Where(predicate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList());

        var result = _service.GetBestPromotionForProduct(_product1.Id, queryDate);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetBestPromotionForProduct_WithInactivePromotion_ReturnsNull()
    {
        DateTime startDate = DateTime.Now.AddDays(10);
        DateTime endDate = DateTime.Now.AddDays(20);
        DateTime queryDate = DateTime.Now.AddDays(5);

        var promotion = new Promotion(
            "Promo",
            startDate,
            endDate,
            30,
            _line,
            new List<Product> { _product1 }
        );

        _mockPromotionRepo
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<Promotion, bool>>>(), 1, 20))
            .Returns((Expression<Func<Promotion, bool>> predicate,
                    int page,
                    int pageSize) =>
                new List<Promotion> { promotion }
                    .AsQueryable()
                    .Where(predicate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList());

        var result = _service.GetBestPromotionForProduct(_product1.Id, queryDate);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetActivePromotions_WhenPaginationIsProvided_UsesCorrectPageAndPageSize()
    {
        var request = new SearchPromotionRequestDto
        {
            Page = 2,
            PageSize = 5
        };

        _mockPromotionRepo
            .Setup(r => r.GetAllBy(
                It.IsAny<Expression<Func<Promotion, bool>>>(),
                2,
                5))
            .Returns(new List<Promotion>());

        _service.GetActivePromotions(request);

        _mockPromotionRepo.Verify(r => r.GetAllBy(
                It.IsAny<Expression<Func<Promotion, bool>>>(),
                2,
                5),
            Times.Once);
    }

    [TestMethod]
    public void GetCommercialLines_ReturnsAllLines()
    {
        var lines = new List<CommercialLine> { _line, new CommercialLine("Bebidas") };

        _mockCommercialLineRepo
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<CommercialLine, bool>>>(),
                                   It.IsAny<int>(), It.IsAny<int>()))
            .Returns(lines);

        var result = _service.GetCommercialLines().ToList();

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(_line.Id, result[0].Id);
        Assert.AreEqual(_line.Name, result[0].Name);
    }

}
