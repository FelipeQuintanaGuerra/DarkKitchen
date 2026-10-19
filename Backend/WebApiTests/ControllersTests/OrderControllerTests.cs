using Domain;
using Domain.Enums;
using DTOs.OrderDTOs;
using IBussinesLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Controllers;

[TestClass]
public class OrderControllerTests
{
    private Mock<IOrderService> _orderServiceMock = null!;
    private OrderController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _orderServiceMock = new Mock<IOrderService>();
        _controller = new OrderController(_orderServiceMock.Object);
    }

    #region CreateOrderTests

    [TestMethod]
    public void CreateOrder_ValidDto_ReturnsCreated()
    {
        var dto = new CreateOrderRequestDto
        {
            CustomerId = Guid.NewGuid(),
            ShippingTypeId = Guid.NewGuid(),
            Street = "Calle 123",
            DoorNumber = "456",
            Apartment = "A",
            Lines = new List<CreateOrderLineRequestDto>
            {
                new CreateOrderLineRequestDto { ProductId = Guid.NewGuid(), Quantity = 2 }
            }
        };

        var expectedResponse = new CreateOrderResponseDto
        {
            OrderNumber = Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            Subtotal = 100,
            ShippingCost = 50,
            Total = 183
        };

        _orderServiceMock
            .Setup(s => s.CreateOrder(dto))
            .Returns(expectedResponse);

        var result = _controller.CreateOrder(dto);

        var createdResult = result as ObjectResult;

        Assert.IsNotNull(createdResult);
        Assert.AreEqual(201, createdResult.StatusCode);
        Assert.AreEqual(expectedResponse, createdResult.Value);
    }

    [TestMethod]
    public void CreateOrder_CallsService()
    {
        var dto = new CreateOrderRequestDto
        {
            CustomerId = Guid.NewGuid(),
            ShippingTypeId = Guid.NewGuid(),
            Street = "Calle 123",
            DoorNumber = "456",
            Apartment = "A",
            Lines = new List<CreateOrderLineRequestDto>
            {
                new CreateOrderLineRequestDto { ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        _orderServiceMock
            .Setup(s => s.CreateOrder(dto))
            .Returns(new CreateOrderResponseDto());

        _controller.CreateOrder(dto);

        _orderServiceMock.Verify(s => s.CreateOrder(dto), Times.Once);
    }

    #endregion

    #region GetOrderByIdTests

    [TestMethod]
    public void GetOrderById_WhenOrderExists_ReturnsOk()
    {
        var orderId = Guid.NewGuid();

        var expectedOrder = new OrderDetailResponseDto
        {
            OrderNumber = orderId,
            ShippingType = "Express",
            Street = "Calle 123",
            DoorNumber = "456",
            Apartment = "A",
            Total = 100
        };

        _orderServiceMock
            .Setup(s => s.GetOrderDetail(orderId))
            .Returns(expectedOrder);

        var result = _controller.GetOrderById(orderId);

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(expectedOrder, okResult.Value);
    }

    [TestMethod]
    public void GetOrderById_CallsService()
    {
        var orderId = Guid.NewGuid();

        _orderServiceMock
            .Setup(s => s.GetOrderDetail(orderId))
            .Returns(new OrderDetailResponseDto());

        _controller.GetOrderById(orderId);

        _orderServiceMock.Verify(s => s.GetOrderDetail(orderId), Times.Once);
    }

    #endregion

    [TestMethod]
    public void ExecuteAction_WhenOrderExists_ReturnsOk()
    {
        var orderId = Guid.NewGuid();
        var token = Guid.NewGuid();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        _controller.Request.Headers["Authorization"] = token.ToString();

        var request = new OrderActionRequestDto
        {
            Action = OrderAction.Prepare
        };

        var expectedResponse = new OrderStatusChangeResponseDto
        {
            OrderNumber = orderId,
            Status = OrderStatus.Prepared,
            ChangedAt = DateTime.Now
        };

        _orderServiceMock
            .Setup(s => s.ExecuteAction(orderId, request.Action, token))
            .Returns(expectedResponse);

        var result = _controller.ExecuteAction(orderId, request);

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(expectedResponse, okResult.Value);
    }
    [TestMethod]
    public void ExecuteAction_CallsService()
    {
        var orderId = Guid.NewGuid();
        var token = Guid.NewGuid();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        _controller.Request.Headers["Authorization"] = token.ToString();

        var request = new OrderActionRequestDto
        {
            Action = OrderAction.Cancel
        };

        _orderServiceMock
            .Setup(s => s.ExecuteAction(orderId, request.Action, token))
            .Returns(new OrderStatusChangeResponseDto());

        _controller.ExecuteAction(orderId, request);

        _orderServiceMock.Verify(
            s => s.ExecuteAction(orderId, request.Action, token),
            Times.Once);
    }

    #region GetCustomerOrdersTests

    [TestMethod]
    public void GetCustomerOrders_WithValidFilters_ReturnsOk()
    {
        var customerId = Guid.NewGuid();

        var dto = new SearchCustomerOrdersRequestDto
        {
            CustomerId = customerId
        };

        var expectedList = new List<OrderSummaryDto>
        {
            new OrderSummaryDto
            {
                OrderNumber = Guid.NewGuid(),
                CustomerId = customerId,
                Total = 100
            }
        };

        _orderServiceMock
            .Setup(s => s.GetCustomerOrders(dto))
            .Returns(expectedList);

        var result = _controller.GetCustomerOrders(dto);

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(expectedList, okResult.Value);
    }

    #endregion

    #region GetFilteredOrdersTests

    [TestMethod]
    public void GetFilteredOrders_WithValidFilters_ReturnsOk()
    {
        var dto = new SearchFilteredOrdersRequestDto
        {
            From = new DateTime(2026, 1, 1),
            To = new DateTime(2026, 12, 31)
        };

        var expectedList = new List<OrderSummaryDto>
        {
            new OrderSummaryDto
            {
                OrderNumber = Guid.NewGuid(),
                Total = 200
            }
        };

        _orderServiceMock
            .Setup(s => s.GetFilteredOrders(dto))
            .Returns(expectedList);

        var result = _controller.GetFilteredOrders(dto);

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(expectedList, okResult.Value);
    }

    #endregion

    [TestMethod]
    public void ExecuteAction_WhenAuthorizationHeaderIsMissing_ReturnsUnauthorized()
    {
        var orderId = Guid.NewGuid();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var request = new OrderActionRequestDto
        {
            Action = OrderAction.Prepare
        };

        var result = _controller.ExecuteAction(orderId, request);

        var unauthorizedResult = result as UnauthorizedResult;

        Assert.IsNotNull(unauthorizedResult);
        Assert.AreEqual(401, unauthorizedResult.StatusCode);

        _orderServiceMock.Verify(
            s => s.ExecuteAction(It.IsAny<Guid>(), It.IsAny<OrderAction>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [TestMethod]
    public void ExecuteAction_WhenAuthorizationHeaderIsInvalid_ReturnsUnauthorized()
    {
        var orderId = Guid.NewGuid();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        _controller.Request.Headers["Authorization"] = "token-invalido";

        var request = new OrderActionRequestDto
        {
            Action = OrderAction.Cancel
        };

        var result = _controller.ExecuteAction(orderId, request);

        var unauthorizedResult = result as UnauthorizedResult;

        Assert.IsNotNull(unauthorizedResult);
        Assert.AreEqual(401, unauthorizedResult.StatusCode);

        _orderServiceMock.Verify(
            s => s.ExecuteAction(It.IsAny<Guid>(), It.IsAny<OrderAction>(), It.IsAny<Guid>()),
            Times.Never);
    }
}