using DTOs.ProductsDTOs;
using IBussinesLogic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Controllers;
using Microsoft.AspNetCore.Http;

[TestClass]
public class ProductControllerTests
{
    private Mock<IProductService> _productServiceMock = null!;
    private ProductController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _productServiceMock = new Mock<IProductService>();

        _controller = new ProductController(_productServiceMock.Object);
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

    private static IFormFile CreateFakeImage(string fileName = "foto.jpg", int sizeInKb = 100)
    {
        var bytes = new byte[sizeInKb * 1024];
        var stream = new MemoryStream(bytes);

        return new FormFile(stream, 0, stream.Length, "Images", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }

    #region CreateProductTests

    [TestMethod]
    public void CreateProduct_ValidDto_ReturnsCreated()
    {
        var dto = CreateValidCreateDto();
        var token = SetupAuthorizationHeader();

        var expected = new ProductSummaryDto
        {
            Code = dto.Code,
            Name = dto.Name
        };

        _productServiceMock
            .Setup(s => s.CreateProduct(dto, token))
            .Returns(expected);

        var result = _controller.CreateProduct(dto);

        var createdResult = result as ObjectResult;

        Assert.IsNotNull(createdResult);
        Assert.AreEqual(201, createdResult.StatusCode);
        Assert.AreEqual(expected, createdResult.Value);
    }

    [TestMethod]
    public void CreateProduct_CallsService()
    {
        var dto = CreateValidCreateDto();
        var token = SetupAuthorizationHeader();

        _productServiceMock
            .Setup(s => s.CreateProduct(dto, token))
            .Returns(new ProductSummaryDto());

        _controller.CreateProduct(dto);

        _productServiceMock.Verify(
            s => s.CreateProduct(dto, token),
            Times.Once);
    }

    [TestMethod]
    public void CreateProduct_WhenAuthorizationHeaderIsMissing_ThrowsUnauthorizedAccessException()
    {
        var dto = CreateValidCreateDto();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        Assert.ThrowsException<UnauthorizedAccessException>(
            () => _controller.CreateProduct(dto));
    }

    [TestMethod]
    public void CreateProduct_WhenAuthorizationHeaderIsInvalid_ThrowsUnauthorizedAccessException()
    {
        var dto = CreateValidCreateDto();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        _controller.Request.Headers["Authorization"] = "token-invalido";

        Assert.ThrowsException<UnauthorizedAccessException>(
            () => _controller.CreateProduct(dto));
    }

    #endregion

    #region UpdateProductTests

    [TestMethod]
    public void UpdateProduct_ValidDto_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var token = SetupAuthorizationHeader();
        var dto = CreateValidUpdateDto();

        _productServiceMock
            .Setup(s => s.UpdateProduct(id, dto, token))
            .Returns(new ProductSummaryDto());

        var result = _controller.UpdateProduct(id, dto);

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
    }

    [TestMethod]
    public void UpdateProduct_CallsService()
    {
        var id = Guid.NewGuid();
        var token = SetupAuthorizationHeader();
        var dto = CreateValidUpdateDto();

        _productServiceMock
            .Setup(s => s.UpdateProduct(id, dto, token))
            .Returns(new ProductSummaryDto());

        _controller.UpdateProduct(id, dto);

        _productServiceMock.Verify(
            s => s.UpdateProduct(id, dto, token),
            Times.Once);
    }

    #endregion

    #region SearchProductsTests

    [TestMethod]
    public void SearchProducts_WithResults_ReturnsOk()
    {
        var dto = CreateValidSearchDto();

        var products = new List<ProductSummaryDto>
        {
            new ProductSummaryDto
            {
                Code = "P1",
                Name = "Burger"
            }
        };

        _productServiceMock
            .Setup(s => s.SearchProducts(dto))
            .Returns(products);

        var result = _controller.SearchProducts(dto);

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(products, okResult.Value);
    }

    [TestMethod]
    public void SearchProducts_CallsService()
    {
        var dto = CreateValidSearchDto();

        _productServiceMock
            .Setup(s => s.SearchProducts(dto))
            .Returns(new List<ProductSummaryDto>());

        _controller.SearchProducts(dto);

        _productServiceMock.Verify(
            s => s.SearchProducts(dto),
            Times.Once);
    }

    #endregion

    #region ChangeActivationTests

    [TestMethod]
    public void ChangeProductActivation_ValidId_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var token = SetupAuthorizationHeader();

        _productServiceMock
            .Setup(s => s.ChangeProductActivation(id, token))
            .Returns(new ProductSummaryDto());

        var result = _controller.ChangeProductActivation(id);

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
    }

    [TestMethod]
    public void ChangeProductActivation_CallsService()
    {
        var id = Guid.NewGuid();
        var token = SetupAuthorizationHeader();

        _productServiceMock
            .Setup(s => s.ChangeProductActivation(id, token))
            .Returns(new ProductSummaryDto());

        _controller.ChangeProductActivation(id);

        _productServiceMock.Verify(
            s => s.ChangeProductActivation(id, token),
            Times.Once);
    }

    #endregion



    private CreateProductRequestDto CreateValidCreateDto()
    {
        return new CreateProductRequestDto
        {
            Code = "PROD123",
            Name = "Producto Test Nombre Largo",
            Description = "Descripción válida suficientemente larga para pasar validaciones",
            Images = new List<IFormFile>
            {
                CreateFakeImage("img1.jpg")
            },
            CommercialLineName = "Minutas clasicas",
            CategoryName = "Fritos",
            Price = 100
        };
    }

    private UpdateProductRequestDto CreateValidUpdateDto()
    {
        return new UpdateProductRequestDto
        {
            Name = "Producto Test Nombre Largo",
            Description = "Descripción válida suficientemente larga para pasar validaciones",
            Price = 1000,
            CommercialLineName = "Minutas clasicas",
            CategoryName = "Fritos",
            Images = new List<IFormFile>
            {
                CreateFakeImage("img2.jpg")
            }
        };
    }

    private SearchProductRequestDto CreateValidSearchDto()
    {
        return new SearchProductRequestDto
        {
            Name = "Producto Test",
            CommercialLineId = Guid.Empty,
            CategoryId = Guid.Empty
        };
    }
}