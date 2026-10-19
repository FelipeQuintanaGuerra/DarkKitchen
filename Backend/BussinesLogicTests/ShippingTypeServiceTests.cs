using BusinessLogic;
using Domain;
using Domain.Enums;
using DTOs.ShippingTypeDTOs;
using IBussinesLogic;
using IRepository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BussinesLogicTests;

[TestClass]
public class ShippingTypeServiceTests
{
    private Mock<IGenericRepository<ShippingType>> _shippingTypeRepository = null!;
    private Mock<IGenericRepository<Order>> _orderRepository = null!;
    private IShippingTypeService _shippingTypeService = null!;

    [TestInitialize]
    public void Setup()
    {
        _shippingTypeRepository = new Mock<IGenericRepository<ShippingType>>(MockBehavior.Strict);
        _orderRepository = new Mock<IGenericRepository<Order>>(MockBehavior.Strict);
        _shippingTypeService = new ShippingTypeService(_shippingTypeRepository.Object, _orderRepository.Object);
    }

    private void SetupNoExistingShippingType()
    {
        _shippingTypeRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<ShippingType, bool>>>()))
            .Returns((ShippingType?)null);
    }

    private void SetupShippingTypeAdd()
    {
        _shippingTypeRepository
            .Setup(r => r.Add(It.IsAny<ShippingType>()));
    }

    private void SetupShippingTypeUpdate()
    {
        _shippingTypeRepository
            .Setup(r => r.Update(It.IsAny<ShippingType>()));
    }

    [TestMethod]
    public void Create_ShouldAddShippingTypeToRepository()
    {
        var request = new CreateShippingTypeRequestDto
        {
            Name = "Envío en el día",
            Cost = 200
        };

        SetupNoExistingShippingType();
        SetupShippingTypeAdd();

        _shippingTypeService.Create(request);

        _shippingTypeRepository.Verify(r => r.Add(It.IsAny<ShippingType>()), Times.Once);
    }

    [TestMethod]
    public void Create_ShouldReturnCreatedShippingType()
    {
        var request = new CreateShippingTypeRequestDto
        {
            Name = "Envío express",
            Cost = 250
        };

        SetupNoExistingShippingType();
        SetupShippingTypeAdd();

        var result = _shippingTypeService.Create(request);

        Assert.AreEqual("Envío express", result.Name);
        Assert.AreEqual(250, result.Cost);
        Assert.AreNotEqual(Guid.Empty, result.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(ConflictException))]
    public void Create_WhenNameAlreadyExists_ShouldThrowException()
    {
        var request = new CreateShippingTypeRequestDto
        {
            Name = "Envío express",
            Cost = 250
        };

        _shippingTypeRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<ShippingType, bool>>>()))
            .Returns(new ShippingType("Envío express", 250));

        _shippingTypeService.Create(request);
    }

    [TestMethod]
    public void GetAll_ShouldReturnAllShippingTypes()
    {
        var shippingTypes = new List<ShippingType>
        {
            new ShippingType("Envío express", 250),
            new ShippingType("Envío en el día", 200)
        };

        _shippingTypeRepository
            .Setup(r => r.GetAllBy(
                It.IsAny<System.Linq.Expressions.Expression<Func<ShippingType, bool>>>(),
                1,
                100))
            .Returns(shippingTypes);

        var result = _shippingTypeService.GetAll();

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Envío express", result[0].Name);
        Assert.AreEqual("Envío en el día", result[1].Name);
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void Update_WhenShippingTypeDoesNotExist_ShouldThrowException()
    {
        var request = new UpdateShippingTypeRequestDto
        {
            Name = "Envío actualizado",
            Cost = 300
        };

        _shippingTypeRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<ShippingType, bool>>>()))
            .Returns((ShippingType?)null);

        _shippingTypeService.Update(Guid.NewGuid(), request);
    }

    [TestMethod]
    public void Update_ShouldModifyShippingType()
    {
        var shippingType = new ShippingType("Envío express", 250);

        var request = new UpdateShippingTypeRequestDto
        {
            Name = "Envío premium premium",
            Cost = 400
        };

        _shippingTypeRepository
            .SetupSequence(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<ShippingType, bool>>>()))
            .Returns(shippingType)
            .Returns((ShippingType?)null);

        SetupShippingTypeUpdate();

        var result = _shippingTypeService.Update(shippingType.Id, request);

        Assert.AreEqual("Envío premium premium", result.Name);
        Assert.AreEqual(400, result.Cost);

        _shippingTypeRepository.Verify(r => r.Update(It.IsAny<ShippingType>()), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(ConflictException))]
    public void Update_WhenAnotherShippingTypeWithSameNameExists_ShouldThrowException()
    {
        var shippingType = new ShippingType("Envío express", 250);

        var request = new UpdateShippingTypeRequestDto
        {
            Name = "Envío premium",
            Cost = 400
        };

        _shippingTypeRepository
            .SetupSequence(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<ShippingType, bool>>>()))
            .Returns(shippingType)
            .Returns(new ShippingType("Envío premium", 500));

        _shippingTypeService.Update(shippingType.Id, request);
    }

    [TestMethod]
    public void Delete_ShouldRemoveShippingTypeFromRepository()
    {
        var shippingType = new ShippingType("Envío express", 250);

        _shippingTypeRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<ShippingType, bool>>>()))
            .Returns(shippingType);

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .Returns((Order?)null);

        _shippingTypeRepository
            .Setup(r => r.Delete(It.IsAny<ShippingType>()));

        _shippingTypeService.Delete(shippingType.Id);

        _shippingTypeRepository.Verify(r => r.Delete(It.IsAny<ShippingType>()), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void Delete_WhenShippingTypeDoesNotExist_ShouldThrowException()
    {
        _shippingTypeRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<ShippingType, bool>>>()))
            .Returns((ShippingType?)null);

        _shippingTypeService.Delete(Guid.NewGuid());
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Delete_WhenShippingTypeIsInUse_ShouldThrowException()
    {
        var shippingType = new ShippingType("Envío express", 250);

        _shippingTypeRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<ShippingType, bool>>>()))
            .Returns(shippingType);

        var order = new Order(
            Guid.NewGuid(),
            shippingType,
            new DeliveryAddress("Calle", "123", "")
        );

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .Returns(order);

        _shippingTypeService.Delete(shippingType.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Delete_WhenShippingTypeIsDefault_ShouldThrowException()
    {
        var shippingType = new ShippingType("Express", 250);
        shippingType.IsDefault = true;

        _shippingTypeRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<ShippingType, bool>>>()))
            .Returns(shippingType);

        _shippingTypeService.Delete(shippingType.Id);
    }
}