using DTOs.CommercialLineDTOs;
using DTOs.PromotionsDTOs;
using IBussinesLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Controllers;

[TestClass]
public class PromotionControllerTests
{
    private Mock<IPromotionService> _promotionServiceMock = null!;
    private PromotionController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _promotionServiceMock = new Mock<IPromotionService>();
        _controller = new PromotionController(_promotionServiceMock.Object);
    }

    private Guid SetupAuthorizationHeader()
    {
        var token = Guid.NewGuid();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        _controller.Request.Headers["Authorization"] = token.ToString();

        return token;
    }

    [TestMethod]
    public void GetActivePromotions_ReturnsOk()
    {
        var dto = new SearchPromotionRequestDto();

        var promotions = new List<PromotionSummaryDto>
        {
            new PromotionSummaryDto
            {
                Name = "Promo",
                Discount = 10,
                ProductNames = new List<string> { "Product1" }
            }
        };

        _promotionServiceMock
            .Setup(s => s.GetActivePromotions(dto))
            .Returns(promotions);

        var result = _controller.GetActivePromotions(dto);

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(promotions, okResult.Value);
    }

    [TestMethod]
    public void CreatePromotion_ReturnsCreated()
    {
        var token = SetupAuthorizationHeader();

        var dto = new CreatePromotionRequestDto
        {
            Name = "New Promotion",
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(10),
            Discount = 20,
            CommercialLineId = Guid.NewGuid(),
            ProductIds = new List<Guid> { Guid.NewGuid() }
        };

        var expected = new PromotionSummaryDto
        {
            Name = dto.Name,
            Discount = dto.Discount,
            ProductNames = new List<string> { "Product1" }
        };

        _promotionServiceMock
            .Setup(s => s.CreatePromotion(dto, token))
            .Returns(expected);

        var result = _controller.CreatePromotion(dto);

        var createdResult = result as ObjectResult;

        Assert.IsNotNull(createdResult);
        Assert.AreEqual(201, createdResult.StatusCode);
        Assert.AreEqual(expected, createdResult.Value);
    }

    [TestMethod]
    public void CreatePromotion_WhenAuthorizationHeaderIsMissing_ThrowsUnauthorizedAccessException()
    {
        var dto = new CreatePromotionRequestDto
        {
            Name = "New Promotion",
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(10),
            Discount = 20,
            CommercialLineId = Guid.NewGuid(),
            ProductIds = new List<Guid> { Guid.NewGuid() }
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        Assert.ThrowsException<UnauthorizedAccessException>(
            () => _controller.CreatePromotion(dto));
    }

    [TestMethod]
    public void CreatePromotion_WhenAuthorizationHeaderIsInvalid_ThrowsUnauthorizedAccessException()
    {
        var dto = new CreatePromotionRequestDto
        {
            Name = "New Promotion",
            StartDate = DateTime.Now.AddDays(1),
            EndDate = DateTime.Now.AddDays(10),
            Discount = 20,
            CommercialLineId = Guid.NewGuid(),
            ProductIds = new List<Guid> { Guid.NewGuid() }
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        _controller.Request.Headers["Authorization"] = "token-invalido";

        Assert.ThrowsException<UnauthorizedAccessException>(
            () => _controller.CreatePromotion(dto));
    }

    [TestMethod]
    public void UpdatePromotion_ReturnsOk()
    {
        var token = SetupAuthorizationHeader();

        var id = Guid.NewGuid();
        var dto = new UpdatePromotionRequestDto
        {
            Name = "Updated Promotion",
            StartDate = DateTime.Now.AddDays(2),
            EndDate = DateTime.Now.AddDays(15),
            Discount = 25
        };

        var expected = new PromotionSummaryDto
        {
            Name = dto.Name,
            Discount = dto.Discount,
            ProductNames = new List<string> { "Product1", "Product2" }
        };

        _promotionServiceMock
            .Setup(s => s.UpdatePromotion(id, dto, token))
            .Returns(expected);

        var result = _controller.UpdatePromotion(id, dto);

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(expected, okResult.Value);
    }

    [TestMethod]
    public void AddProductToPromotion_ReturnsNoContent()
    {
        var token = SetupAuthorizationHeader();

        var promotionId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _promotionServiceMock
            .Setup(s => s.AssociateProduct(promotionId, productId, token))
            .Returns(new PromotionSummaryDto());

        var dto = new AddProductToPromotionDto { ProductId = productId };
        var result = _controller.AddProductToPromotion(promotionId, dto);

        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }

    [TestMethod]
    public void RemoveProductFromPromotion_ReturnsNoContent()
    {
        var token = SetupAuthorizationHeader();

        var promotionId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _promotionServiceMock
            .Setup(s => s.RemoveProduct(promotionId, productId, token))
            .Returns(new PromotionSummaryDto());

        var result = _controller.RemoveProductFromPromotion(promotionId, productId);

        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }

    [TestMethod]
    public void GetCommercialLines_ReturnsOk()
    {
        var lines = new List<CommercialLineDto>
        {
            new CommercialLineDto { Id = Guid.NewGuid(), Name = "Minutas"
            }
        };

        _promotionServiceMock
            .Setup(s => s.GetCommercialLines())
            .Returns(lines);

        var result = _controller.GetCommercialLines();

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(lines, okResult.Value);
    }
}