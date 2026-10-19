using DTOs.ShippingTypeDTOs;
using IBussinesLogic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Controllers;

namespace WebApiTests;

[TestClass]
public class ShippingTypeControllerTests
{
    private Mock<IShippingTypeService> _shippingTypeServiceMock = null!;
    private ShippingTypeController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _shippingTypeServiceMock = new Mock<IShippingTypeService>();
        _controller = new ShippingTypeController(_shippingTypeServiceMock.Object);
    }

    [TestMethod]
    public void Create_ValidDto_ReturnsCreated()
    {
        var request = new CreateShippingTypeRequestDto
        {
            Name = "Envío en el día",
            Cost = 200
        };

        var expectedResponse = new ShippingTypeResponseDto
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Cost = request.Cost
        };

        _shippingTypeServiceMock
            .Setup(s => s.Create(request))
            .Returns(expectedResponse);

        var result = _controller.Create(request);

        var createdResult = result as ObjectResult;

        Assert.IsNotNull(createdResult);
        Assert.AreEqual(201, createdResult.StatusCode);
        Assert.AreEqual(expectedResponse, createdResult.Value);
    }

    [TestMethod]
    public void Create_ValidDto_CallsService()
    {
        var request = new CreateShippingTypeRequestDto
        {
            Name = "Envío en el día",
            Cost = 200
        };

        _shippingTypeServiceMock
            .Setup(s => s.Create(request))
            .Returns(new ShippingTypeResponseDto());

        _controller.Create(request);

        _shippingTypeServiceMock.Verify(s => s.Create(request), Times.Once);
    }

    [TestMethod]
    public void Update_ValidDto_ReturnsOk()
    {
        var id = Guid.NewGuid();

        var request = new UpdateShippingTypeRequestDto
        {
            Name = "Envío premium",
            Cost = 400
        };

        var expectedResponse = new ShippingTypeResponseDto
        {
            Id = id,
            Name = request.Name,
            Cost = request.Cost
        };

        _shippingTypeServiceMock
            .Setup(s => s.Update(id, request))
            .Returns(expectedResponse);

        var result = _controller.Update(id, request);

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(expectedResponse, okResult.Value);
    }

    [TestMethod]
    public void Update_ValidDto_CallsService()
    {
        var id = Guid.NewGuid();

        var request = new UpdateShippingTypeRequestDto
        {
            Name = "Envío premium",
            Cost = 400
        };

        _shippingTypeServiceMock
            .Setup(s => s.Update(id, request))
            .Returns(new ShippingTypeResponseDto());

        _controller.Update(id, request);

        _shippingTypeServiceMock.Verify(s => s.Update(id, request), Times.Once);
    }

    [TestMethod]
    public void GetAll_ReturnsOk()
    {
        var expectedResponse = new List<ShippingTypeResponseDto>
        {
            new ShippingTypeResponseDto
            {
                Id = Guid.NewGuid(),
                Name = "Express",
                Cost = 250
            },
            new ShippingTypeResponseDto
            {
                Id = Guid.NewGuid(),
                Name = "24 horas",
                Cost = 180
            }
        };

        _shippingTypeServiceMock
            .Setup(s => s.GetAll())
            .Returns(expectedResponse);

        var result = _controller.GetAll();

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(expectedResponse, okResult.Value);
    }

    [TestMethod]
    public void GetAll_CallsService()
    {
        _shippingTypeServiceMock
            .Setup(s => s.GetAll())
            .Returns(new List<ShippingTypeResponseDto>());

        _controller.GetAll();

        _shippingTypeServiceMock.Verify(s => s.GetAll(), Times.Once);
    }

    [TestMethod]
    public void Delete_ValidId_ReturnsNoContent()
    {
        var id = Guid.NewGuid();

        _shippingTypeServiceMock.Setup(s => s.Delete(id));

        var result = _controller.Delete(id);

        var noContentResult = result as NoContentResult;

        Assert.IsNotNull(noContentResult);
        Assert.AreEqual(204, noContentResult.StatusCode);
    }

    [TestMethod]
    public void Delete_ValidId_CallsService()
    {
        var id = Guid.NewGuid();

        _shippingTypeServiceMock.Setup(s => s.Delete(id));

        _controller.Delete(id);

        _shippingTypeServiceMock.Verify(s => s.Delete(id), Times.Once);
    }
}