using System.Linq.Expressions;
using BusinessLogic;
using Domain;
using Domain.Enums;
using DTOs.OrderDTOs;
using IBussinesLogic;
using IRepository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BussinesLogicTests;

[TestClass]
public class OrderServiceTests
{
    private Mock<IGenericRepository<User>> _userRepository = null!;
    private Mock<IGenericRepository<ShippingType>> _shippingTypeRepository = null!;
    private Mock<IOrderRepository> _orderRepository = null!;
    private Mock<IProductRepository> _productRepository = null!;
    private Mock<IPromotionService> _promotionService = null!;
    private IOrderService _orderService = null!;
    private Mock<ISessionRepository> _sessionRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        _orderRepository = new Mock<IOrderRepository>(MockBehavior.Strict);
        _productRepository = new Mock<IProductRepository>(MockBehavior.Strict);
        _userRepository = new Mock<IGenericRepository<User>>(MockBehavior.Strict);
        _shippingTypeRepository = new Mock<IGenericRepository<ShippingType>>(MockBehavior.Strict);
        _promotionService = new Mock<IPromotionService>(MockBehavior.Strict);
        _sessionRepository = new Mock<ISessionRepository>(MockBehavior.Strict);

        _orderService = new OrderService(
            _orderRepository.Object,
            _userRepository.Object,
            _productRepository.Object,
            _promotionService.Object,
            _sessionRepository.Object,
            _shippingTypeRepository.Object
        );
    }

    private static ShippingType CreateShippingType()
    {
        return new ShippingType("Tipo de envío válido", 250);
    }

    private static Order CreateOrder()
    {
        return new Order(
            Guid.NewGuid(),
            CreateShippingType(),
            new DeliveryAddress("Rivera", "1234", "101"));
    }

    private static User CreateCustomer(Guid customerId)
    {
        var customer = new User(
            "Juan",
            "Perez",
            "test@test.com",
            "099123456",
            "PasswordSuper123!",
            Role.Client);

        customer.Id = customerId;

        return customer;
    }

    private static Product CreateProductForOrderLine()
    {
        var commercialLine = new CommercialLine("Linea test");
        var category = new Category("Categoria test");
        var pictures = new List<ProductPicture>
        {
            new ProductPicture("foto.jpg", 100)
        };

        return new Product(
            "COD123",
            "Producto test",
            "Descripcion valida del producto",
            commercialLine,
            category,
            pictures,
            100);
    }

    private static void SetCustomer(Order order, User customer)
    {
        typeof(Order)
            .GetProperty(nameof(Order.Customer))!
            .SetValue(order, customer);
    }

    private static void SetProduct(OrderLine line, Product product)
    {
        typeof(OrderLine)
            .GetProperty(nameof(OrderLine.Product))!
            .SetValue(line, product);
    }

    private static Order CreateOrderForMapper(
        Guid customerId,
        string street = "Rivera",
        string doorNumber = "1234",
        string apartment = "101",
        int quantity = 1,
        decimal unitPrice = 100,
        decimal discount = 0)
    {
        var order = new Order(
            customerId,
            CreateShippingType(),
            new DeliveryAddress(street, doorNumber, apartment));

        SetCustomer(order, CreateCustomer(customerId));

        var productId = Guid.NewGuid();
        var line = new OrderLine(productId, quantity, unitPrice, discount);
        SetProduct(line, CreateProductForOrderLine());

        order.AddLine(line);
        order.CalculateTotals();

        return order;
    }

    private void SetupValidShippingType()
    {
        _shippingTypeRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<ShippingType, bool>>>()))
            .Returns(CreateShippingType());
    }

    private void SetupOrderAdd()
    {
        _orderRepository
            .Setup(r => r.Add(It.IsAny<Order>()));
    }

    private void SetupOrderUpdate()
    {
        _orderRepository
            .Setup(r => r.Update(It.IsAny<Order>()));
    }

    private void SetupNoPromotion()
    {
        _promotionService
            .Setup(p => p.GetBestPromotionForProduct(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .Returns((Promotion?)null);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CreateOrder_WhenRequestHasNoLines_ShouldThrowException()
    {
        CreateOrderRequestDto request = new CreateOrderRequestDto
        {
            CustomerId = Guid.NewGuid(),
            ShippingTypeId = Guid.NewGuid(),
            Street = "Rivera",
            DoorNumber = "1234",
            Apartment = "101",
            Lines = new List<CreateOrderLineRequestDto>()
        };

        _orderService.CreateOrder(request);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void CreateOrder_WhenRequestIsNull_ShouldThrowArgumentNullException()
    {
        _orderService.CreateOrder(null!);
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void CreateOrder_WhenCustomerDoesNotExist_ShouldThrowException()
    {
        CreateOrderRequestDto request = new CreateOrderRequestDto
        {
            CustomerId = Guid.NewGuid(),
            ShippingTypeId = Guid.NewGuid(),
            Street = "Rivera",
            DoorNumber = "1234",
            Apartment = "101",
            Lines = new List<CreateOrderLineRequestDto>
            {
                new CreateOrderLineRequestDto
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1
                }
            }
        };

        _userRepository
            .Setup(repo => repo.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .Returns((User?)null);

        _orderService.CreateOrder(request);
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void CreateOrder_WhenProductDoesNotExist_ShouldThrowException()
    {
        var productId = Guid.NewGuid();

        CreateOrderRequestDto request = new CreateOrderRequestDto
        {
            CustomerId = Guid.NewGuid(),
            ShippingTypeId = Guid.NewGuid(),
            Street = "Rivera",
            DoorNumber = "1234",
            Apartment = "101",
            Lines = new List<CreateOrderLineRequestDto>
            {
                new CreateOrderLineRequestDto
                {
                    ProductId = productId,
                    Quantity = 1
                }
            }
        };

        _userRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .Returns(new User("Juan", "Perez", "test@test.com", "099123456", "PasswordSuper123!", Domain.Enums.Role.Client));

        _productRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .Returns((Product?)null);

        _orderService.CreateOrder(request);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void CreateOrder_WhenProductIsInactive_ShouldThrowException()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        CreateOrderRequestDto request = new CreateOrderRequestDto
        {
            CustomerId = customerId,
            ShippingTypeId = Guid.NewGuid(),
            Street = "Rivera",
            DoorNumber = "1234",
            Apartment = "101",
            Lines = new List<CreateOrderLineRequestDto>
            {
                new CreateOrderLineRequestDto
                {
                    ProductId = productId,
                    Quantity = 1
                }
            }
        };

        var customer = new User(
            "Juan",
            "Perez",
            "test@test.com",
            "099123456",
            "PasswordSuper123!",
            Domain.Enums.Role.Client
        );

        var commercialLine = new CommercialLine("Linea test");
        var category = new Category("Categoria test");
        var pictures = new List<ProductPicture>
        {
            new ProductPicture("foto.jpg", 100)
        };

        var product = new Product(
            "COD123",
            "Producto test",
            "Descripcion valida del producto",
            commercialLine,
            category,
            pictures,
            100
        );

        product.ChangeActivation();

        _userRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .Returns(customer);

        _productRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .Returns(product);

        _orderService.CreateOrder(request);
    }

    [TestMethod]
    public void ExecuteAction_WhenSessionDoesNotExist_ShouldThrowUnauthorizedAccessException()
    {
        var token = Guid.NewGuid();

        _sessionRepository
            .Setup(r => r.GetByToken(token))
            .Returns((Session?)null);

        Assert.ThrowsException<UnauthorizedAccessException>(() =>
            _orderService.ExecuteAction(
                Guid.NewGuid(),
                OrderAction.Prepare,
                token));
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void CreateOrder_WhenShippingTypeDoesNotExist_ShouldThrowException()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        CreateOrderRequestDto request = new CreateOrderRequestDto
        {
            CustomerId = customerId,
            ShippingTypeId = Guid.NewGuid(),
            Street = "Rivera",
            DoorNumber = "1234",
            Apartment = "101",
            Lines = new List<CreateOrderLineRequestDto>
            {
                new CreateOrderLineRequestDto
                {
                    ProductId = productId,
                    Quantity = 1
                }
            }
        };

        var customer = new User(
            "Juan",
            "Perez",
            "test@test.com",
            "099123456",
            "PasswordSuper123!",
            Domain.Enums.Role.Client
        );

        var commercialLine = new CommercialLine("Linea test");
        var category = new Category("Categoria test");
        var pictures = new List<ProductPicture>
        {
            new ProductPicture("foto.jpg", 100)
        };

        var product = new Product(
            "COD123",
            "Producto test",
            "Descripcion valida del producto",
            commercialLine,
            category,
            pictures,
            100
        );

        _userRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .Returns(customer);

        _productRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .Returns(product);

        _shippingTypeRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<ShippingType, bool>>>()))
            .Returns((ShippingType?)null);

        _orderService.CreateOrder(request);
    }

    [TestMethod]
    public void CreateOrder_ShouldReturnOrderCreatedInPendingStatus()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        CreateOrderRequestDto request = new CreateOrderRequestDto
        {
            CustomerId = customerId,
            ShippingTypeId = Guid.NewGuid(),
            Street = "Rivera",
            DoorNumber = "1234",
            Apartment = "101",
            Lines = new List<CreateOrderLineRequestDto>
            {
                new CreateOrderLineRequestDto
                {
                    ProductId = productId,
                    Quantity = 2
                }
            }
        };

        var customer = new User(
            "Juan",
            "Perez",
            "test@test.com",
            "099123456",
            "PasswordSuper123!",
            Domain.Enums.Role.Client
        );

        var commercialLine = new CommercialLine("Linea test");
        var category = new Category("Categoria test");
        var pictures = new List<ProductPicture>
        {
            new ProductPicture("foto.jpg", 100)
        };

        var product = new Product(
            "COD123",
            "Producto test",
            "Descripcion valida del producto",
            commercialLine,
            category,
            pictures,
            100
        );

        _userRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .Returns(customer);

        _productRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .Returns(product);

        SetupValidShippingType();
        SetupNoPromotion();
        SetupOrderAdd();

        var result = _orderService.CreateOrder(request);

        Assert.AreEqual(OrderStatus.Pending, result.Status);
    }

    [TestMethod]
    public void CreateOrder_ShouldAddOrderToRepository()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        CreateOrderRequestDto request = new CreateOrderRequestDto
        {
            CustomerId = customerId,
            ShippingTypeId = Guid.NewGuid(),
            Street = "Rivera",
            DoorNumber = "1234",
            Apartment = "101",
            Lines = new List<CreateOrderLineRequestDto>
            {
                new CreateOrderLineRequestDto
                {
                    ProductId = productId,
                    Quantity = 2
                }
            }
        };

        var customer = new User(
            "Juan",
            "Perez",
            "test@test.com",
            "099123456",
            "PasswordSuper123!",
            Domain.Enums.Role.Client
        );

        var commercialLine = new CommercialLine("Linea test");
        var category = new Category("Categoria test");
        var pictures = new List<ProductPicture>
        {
            new ProductPicture("foto.jpg", 100)
        };

        var product = new Product(
            "COD123",
            "Producto test",
            "Descripcion valida del producto",
            commercialLine,
            category,
            pictures,
            100
        );

        _userRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .Returns(customer);

        _productRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .Returns(product);

        SetupValidShippingType();
        SetupNoPromotion();
        SetupOrderAdd();

        _orderService.CreateOrder(request);

        _orderRepository.Verify(r => r.Add(It.IsAny<Order>()), Times.Once);
    }

    [TestMethod]
    public void CreateOrder_ShouldAddLinesToOrder()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        CreateOrderRequestDto request = new CreateOrderRequestDto
        {
            CustomerId = customerId,
            ShippingTypeId = Guid.NewGuid(),
            Street = "Rivera",
            DoorNumber = "1234",
            Apartment = "101",
            Lines = new List<CreateOrderLineRequestDto>
            {
                new CreateOrderLineRequestDto
                {
                    ProductId = productId,
                    Quantity = 2
                }
            }
        };

        var customer = new User(
            "Juan",
            "Perez",
            "test@test.com",
            "099123456",
            "PasswordSuper123!",
            Domain.Enums.Role.Client
        );

        var commercialLine = new CommercialLine("Linea test");
        var category = new Category("Categoria test");
        var pictures = new List<ProductPicture>
        {
            new ProductPicture("foto.jpg", 100)
        };

        var product = new Product(
            "COD123",
            "Producto test",
            "Descripcion valida del producto",
            commercialLine,
            category,
            pictures,
            100
        );

        _userRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .Returns(customer);

        _productRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .Returns(product);

        SetupValidShippingType();
        SetupNoPromotion();

        Order? capturedOrder = null;

        _orderRepository
            .Setup(r => r.Add(It.IsAny<Order>()))
            .Callback<Order>(o => capturedOrder = o);

        _orderService.CreateOrder(request);

        Assert.IsNotNull(capturedOrder);
        Assert.AreEqual(1, capturedOrder.Lines.Count);
    }

    [TestMethod]
    public void CreateOrder_ShouldCalculateTotals()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        CreateOrderRequestDto request = new CreateOrderRequestDto
        {
            CustomerId = customerId,
            ShippingTypeId = Guid.NewGuid(),
            Street = "Rivera",
            DoorNumber = "1234",
            Apartment = "101",
            Lines = new List<CreateOrderLineRequestDto>
            {
                new CreateOrderLineRequestDto
                {
                    ProductId = productId,
                    Quantity = 2
                }
            }
        };

        var customer = new User(
            "Juan",
            "Perez",
            "test@test.com",
            "099123456",
            "PasswordSuper123!",
            Domain.Enums.Role.Client
        );

        var commercialLine = new CommercialLine("Linea test");
        var category = new Category("Categoria test");
        var pictures = new List<ProductPicture>
        {
            new ProductPicture("foto.jpg", 100)
        };

        var product = new Product(
            "COD123",
            "Producto test",
            "Descripcion valida del producto",
            commercialLine,
            category,
            pictures,
            150
        );

        _userRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .Returns(customer);

        _productRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .Returns(product);

        SetupValidShippingType();
        SetupNoPromotion();
        SetupOrderAdd();

        var result = _orderService.CreateOrder(request);

        Assert.AreEqual(300, result.Subtotal);
        Assert.AreEqual(250, result.ShippingCost);
        Assert.AreEqual(671m, result.Total);
    }

    [TestMethod]
    public void CreateOrder_ShouldApplyPromotionDiscount()
    {
        var customerId = Guid.NewGuid();

        var customer = new User(
            "Juan",
            "Perez",
            "test@test.com",
            "099123456",
            "PasswordSuper123!",
            Domain.Enums.Role.Client
        );

        var commercialLine = new CommercialLine("Linea test");
        var category = new Category("Categoria test");
        var pictures = new List<ProductPicture>
        {
            new ProductPicture("foto.jpg", 100)
        };

        var product = new Product(
            "COD123",
            "Producto test",
            "Descripcion valida del producto",
            commercialLine,
            category,
            pictures,
            100
        );

        CreateOrderRequestDto request = new CreateOrderRequestDto
        {
            CustomerId = customerId,
            ShippingTypeId = Guid.NewGuid(),
            Street = "Rivera",
            DoorNumber = "1234",
            Apartment = "101",
            Lines = new List<CreateOrderLineRequestDto>
            {
                new CreateOrderLineRequestDto
                {
                    ProductId = product.Id,
                    Quantity = 2
                }
            }
        };

        var startDate = DateTime.Now.AddMinutes(1);
        var endDate = startDate.AddDays(5);

        var promotion = new Promotion(
            "Promo1",
            startDate,
            endDate,
            20,
            commercialLine,
            new List<Product> { product }
        );

        _userRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .Returns(customer);

        _productRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .Returns(product);

        _promotionService
            .Setup(p => p.GetBestPromotionForProduct(product.Id, It.IsAny<DateTime>()))
            .Returns(promotion);

        SetupValidShippingType();
        SetupOrderAdd();

        var result = _orderService.CreateOrder(request);

        Assert.AreEqual(160, result.Subtotal);
    }

    [TestMethod]
    public void CreateOrder_ShouldUseShippingTypeFromRepository()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var shippingType = new ShippingType("Envío personalizado", 375);

        CreateOrderRequestDto request = new CreateOrderRequestDto
        {
            CustomerId = customerId,
            ShippingTypeId = shippingType.Id,
            Street = "Rivera",
            DoorNumber = "1234",
            Apartment = "101",
            Lines = new List<CreateOrderLineRequestDto>
            {
                new CreateOrderLineRequestDto
                {
                    ProductId = productId,
                    Quantity = 1
                }
            }
        };

        var customer = new User(
            "Juan",
            "Perez",
            "test@test.com",
            "099123456",
            "PasswordSuper123!",
            Domain.Enums.Role.Client
        );

        var commercialLine = new CommercialLine("Linea test");
        var category = new Category("Categoria test");
        var pictures = new List<ProductPicture>
        {
            new ProductPicture("foto.jpg", 100)
        };

        var product = new Product(
            "COD123",
            "Producto test",
            "Descripcion valida del producto",
            commercialLine,
            category,
            pictures,
            100
        );

        _userRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .Returns(customer);

        _productRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .Returns(product);

        _shippingTypeRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<ShippingType, bool>>>()))
            .Returns(shippingType);

        SetupNoPromotion();

        Order? capturedOrder = null;

        _orderRepository
            .Setup(r => r.Add(It.IsAny<Order>()))
            .Callback<Order>(o => capturedOrder = o);

        _orderService.CreateOrder(request);

        Assert.IsNotNull(capturedOrder);
        Assert.AreEqual(shippingType.Id, capturedOrder.ShippingTypeId);
        Assert.AreEqual(375, capturedOrder.ShippingCost);
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void PrepareOrder_WhenOrderDoesNotExist_ShouldThrowException()
    {
        var orderNumber = Guid.NewGuid();

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .Returns((Order?)null);

        _orderService.PrepareOrder(orderNumber);
    }

    [TestMethod]
    public void PrepareOrder_ShouldChangeStatusToPrepared()
    {
        var order = CreateOrder();

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .Returns(order);

        SetupOrderUpdate();

        var result = _orderService.PrepareOrder(order.OrderNumber);

        Assert.AreEqual(OrderStatus.Prepared, result.Status);
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void CancelOrder_WhenOrderDoesNotExist_ShouldThrowException()
    {
        var orderNumber = Guid.NewGuid();

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .Returns((Order?)null);

        _orderService.CancelOrder(orderNumber);
    }

    [TestMethod]
    public void CancelOrder_ShouldChangeStatusToCancelled()
    {
        var order = CreateOrder();

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .Returns(order);

        SetupOrderUpdate();

        var result = _orderService.CancelOrder(order.OrderNumber);

        Assert.AreEqual(OrderStatus.Cancelled, result.Status);
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void MarkOrderAsInTransit_WhenOrderDoesNotExist_ShouldThrowException()
    {
        var orderNumber = Guid.NewGuid();

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .Returns((Order?)null);

        _orderService.MarkOrderAsInTransit(orderNumber);
    }

    [TestMethod]
    public void MarkOrderAsInTransit_ShouldChangeStatusToInTransit()
    {
        var order = CreateOrder();
        order.Prepare();

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .Returns(order);

        SetupOrderUpdate();

        var result = _orderService.MarkOrderAsInTransit(order.OrderNumber);

        Assert.AreEqual(OrderStatus.InTransit, result.Status);
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void DeliverOrder_WhenOrderDoesNotExist_ShouldThrowException()
    {
        var orderNumber = Guid.NewGuid();

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .Returns((Order?)null);

        _orderService.DeliverOrder(orderNumber);
    }

    [TestMethod]
    public void DeliverOrder_ShouldChangeStatusToDelivered()
    {
        var order = CreateOrder();
        order.Prepare();
        order.MarkAsInTransit();

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .Returns(order);

        SetupOrderUpdate();

        var result = _orderService.DeliverOrder(order.OrderNumber);

        Assert.AreEqual(OrderStatus.Delivered, result.Status);
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void MarkOrderAsNotDelivered_WhenOrderDoesNotExist_ShouldThrowException()
    {
        var orderNumber = Guid.NewGuid();

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .Returns((Order?)null);

        _orderService.MarkOrderAsNotDelivered(orderNumber);
    }

    [TestMethod]
    public void MarkOrderAsNotDelivered_ShouldChangeStatusToNotDelivered()
    {
        var order = CreateOrder();
        order.Prepare();
        order.MarkAsInTransit();

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .Returns(order);

        SetupOrderUpdate();

        var result = _orderService.MarkOrderAsNotDelivered(order.OrderNumber);

        Assert.AreEqual(OrderStatus.NotDelivered, result.Status);
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void GetOrderDetail_WhenOrderDoesNotExist_ShouldThrowException()
    {
        var orderNumber = Guid.NewGuid();

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .Returns((Order?)null);

        _orderService.GetOrderDetail(orderNumber);
    }

    [TestMethod]
    public void GetOrderDetail_ShouldReturnOrderDetail()
    {
        var order = CreateOrderForMapper(Guid.NewGuid(), quantity: 2);

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .Returns(order);

        var result = _orderService.GetOrderDetail(order.OrderNumber);

        Assert.AreEqual(order.OrderNumber, result.OrderNumber);
        Assert.AreEqual(order.CustomerId, result.CustomerId);
        Assert.AreEqual(order.Status, result.Status);
        Assert.AreEqual(order.Subtotal, result.Subtotal);
        Assert.AreEqual(order.ShippingCost, result.ShippingCost);
        Assert.AreEqual(order.Total, result.Total);
        Assert.AreEqual(order.DeliveryAddress.Street, result.Street);
        Assert.AreEqual(order.DeliveryAddress.DoorNumber, result.DoorNumber);
        Assert.AreEqual(order.DeliveryAddress.Apartment, result.Apartment);
        Assert.AreEqual(1, result.Lines.Count);
    }

    [TestMethod]
    public void GetCustomerOrders_ShouldReturnOnlyOrdersFromThatCustomer()
    {
        var customerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();

        var order1 = CreateOrderForMapper(customerId, "Rivera", "1234", "101");
        var order2 = CreateOrderForMapper(otherCustomerId, "18 de Julio", "2222", "202");

        _orderRepository
            .Setup(r => r.GetAllBy(
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
                1,
                20))
            .Returns(new List<Order> { order1 });

        var result = _orderService.GetCustomerOrders(new SearchCustomerOrdersRequestDto
        {
            CustomerId = customerId
        });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(order1.OrderNumber, result[0].OrderNumber);
        Assert.AreEqual(order1.CustomerId, result[0].CustomerId);
    }

    [TestMethod]
    public void GetCustomerOrders_WhenStatusIsProvided_ShouldFilterByStatus()
    {
        var customerId = Guid.NewGuid();

        var pendingOrder = CreateOrderForMapper(customerId, "Rivera", "1234", "101");

        var preparedOrder = CreateOrderForMapper(customerId, "18 de Julio", "2222", "202");
        preparedOrder.Prepare();

        var orders = new List<Order> { pendingOrder, preparedOrder };

        _orderRepository
            .Setup(r => r.GetAllBy(
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
                1,
                20))
            .Returns((System.Linq.Expressions.Expression<Func<Order, bool>> predicate,
                    int page,
                    int pageSize) =>
                orders
                    .Where(predicate.Compile())
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList());

        var result = _orderService.GetCustomerOrders(new SearchCustomerOrdersRequestDto
        {
            CustomerId = customerId,
            Status = "Prepared"
        });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(preparedOrder.OrderNumber, result[0].OrderNumber);
        Assert.AreEqual(OrderStatus.Prepared, result[0].Status);
    }

    [TestMethod]
    public void GetCustomerOrders_WhenFromDateIsProvided_ShouldFilterByCreatedAtFrom()
    {
        var customerId = Guid.NewGuid();

        var oldOrder = CreateOrderForMapper(customerId, "Rivera", "1234", "101");

        Thread.Sleep(20);

        var fromDate = DateTime.Now;

        Thread.Sleep(20);

        var newOrder = CreateOrderForMapper(customerId, "18 de Julio", "2222", "202");

        var orders = new List<Order> { oldOrder, newOrder };

        _orderRepository
            .Setup(r => r.GetAllBy(
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
                1,
                20))
            .Returns((System.Linq.Expressions.Expression<Func<Order, bool>> predicate,
                    int page,
                    int pageSize) =>
                orders
                    .Where(predicate.Compile())
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList());

        var result = _orderService.GetCustomerOrders(new SearchCustomerOrdersRequestDto
        {
            CustomerId = customerId,
            From = fromDate
        });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(newOrder.OrderNumber, result[0].OrderNumber);
    }

    [TestMethod]
    public void GetCustomerOrders_WhenToDateIsProvided_ShouldFilterByCreatedAtTo()
    {
        var customerId = Guid.NewGuid();

        var firstOrder = CreateOrderForMapper(customerId, "Rivera", "1234", "101");

        Thread.Sleep(20);

        var toDate = DateTime.Now;

        Thread.Sleep(20);

        var secondOrder = CreateOrderForMapper(customerId, "18 de Julio", "2222", "202");

        var orders = new List<Order> { firstOrder, secondOrder };

        _orderRepository
            .Setup(r => r.GetAllBy(
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
                1,
                20))
            .Returns((System.Linq.Expressions.Expression<Func<Order, bool>> predicate,
                    int page,
                    int pageSize) =>
                orders
                    .Where(predicate.Compile())
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList());

        var result = _orderService.GetCustomerOrders(new SearchCustomerOrdersRequestDto
        {
            CustomerId = customerId,
            To = toDate
        });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(firstOrder.OrderNumber, result[0].OrderNumber);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void GetCustomerOrders_WhenStatusIsInvalid_ShouldThrowException()
    {
        var customerId = Guid.NewGuid();

        _orderService.GetCustomerOrders(new SearchCustomerOrdersRequestDto
        {
            CustomerId = customerId,
            Status = "CualquierCosa"
        });
    }

    [TestMethod]
    public void GetFilteredOrders_ShouldReturnOrdersInDateRange()
    {
        var oldOrder = CreateOrderForMapper(Guid.NewGuid(), "Rivera", "1234", "101");

        Thread.Sleep(1000);

        var fromDate = DateTime.Now;

        Thread.Sleep(1000);

        var newOrder = CreateOrderForMapper(Guid.NewGuid(), "18 de Julio", "2222", "202");

        var orders = new List<Order> { oldOrder, newOrder };

        _orderRepository
            .Setup(r => r.GetAllBy(
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
                1,
                20))
            .Returns((System.Linq.Expressions.Expression<Func<Order, bool>> predicate,
                    int page,
                    int pageSize) =>
                orders
                    .Where(predicate.Compile())
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList());

        var result = _orderService.GetFilteredOrders(new SearchFilteredOrdersRequestDto
        {
            From = fromDate,
            To = DateTime.Now.AddMinutes(1)
        });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(newOrder.OrderNumber, result[0].OrderNumber);
    }

    [TestMethod]
    public void GetFilteredOrders_WhenStatusIsProvided_ShouldFilterByStatus()
    {
        var pendingOrder = CreateOrderForMapper(Guid.NewGuid(), "Rivera", "1234", "101");

        var preparedOrder = CreateOrderForMapper(Guid.NewGuid(), "18 de Julio", "2222", "202");
        preparedOrder.Prepare();

        var orders = new List<Order> { pendingOrder, preparedOrder };

        _orderRepository
            .Setup(r => r.GetAllBy(
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
                1,
                20))
            .Returns((System.Linq.Expressions.Expression<Func<Order, bool>> predicate,
                    int page,
                    int pageSize) =>
                orders
                    .Where(predicate.Compile())
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList());

        var result = _orderService.GetFilteredOrders(new SearchFilteredOrdersRequestDto
        {
            From = DateTime.MinValue,
            To = DateTime.MaxValue,
            Status = "Prepared"
        });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(preparedOrder.OrderNumber, result[0].OrderNumber);
        Assert.AreEqual(OrderStatus.Prepared, result[0].Status);
    }

    [TestMethod]
    public void GetFilteredOrders_WhenAddressTextIsProvided_ShouldFilterByAddress()
    {
        var order1 = CreateOrderForMapper(Guid.NewGuid(), "Rivera", "1234", "101");
        var order2 = CreateOrderForMapper(Guid.NewGuid(), "18 de Julio", "2222", "202");

        var orders = new List<Order> { order1, order2 };

        _orderRepository
            .Setup(r => r.GetAllBy(
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
                1,
                20))
            .Returns((System.Linq.Expressions.Expression<Func<Order, bool>> predicate,
                    int page,
                    int pageSize) =>
                orders
                    .Where(predicate.Compile())
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList());

        var result = _orderService.GetFilteredOrders(new SearchFilteredOrdersRequestDto
        {
            From = DateTime.MinValue,
            To = DateTime.MaxValue,
            AddressText = "Rivera"
        });

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(order1.OrderNumber, result[0].OrderNumber);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void GetFilteredOrders_WhenStatusIsInvalid_ShouldThrowException()
    {
        _orderService.GetFilteredOrders(new SearchFilteredOrdersRequestDto
        {
            From = DateTime.MinValue,
            To = DateTime.MaxValue,
            Status = "CualquierCosa"
        });
    }

    [TestMethod]
    public void GetCustomerOrders_WhenPaginationIsProvided_UsesCorrectPageAndPageSize()
    {
        var dto = new SearchCustomerOrdersRequestDto
        {
            CustomerId = Guid.NewGuid(),
            Page = 2,
            PageSize = 5
        };

        _orderRepository
            .Setup(r => r.GetAllBy(
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
                2,
                5))
            .Returns(new List<Order>());

        _orderService.GetCustomerOrders(dto);

        _orderRepository.Verify(r => r.GetAllBy(
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
                2,
                5),
            Times.Once);
    }

    [TestMethod]
    public void GetFilteredOrders_WhenPaginationIsProvided_UsesCorrectPageAndPageSize()
    {
        var dto = new SearchFilteredOrdersRequestDto
        {
            From = DateTime.MinValue,
            To = DateTime.MaxValue,
            Page = 3,
            PageSize = 10
        };

        _orderRepository
            .Setup(r => r.GetAllBy(
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
                3,
                10))
            .Returns(new List<Order>());

        _orderService.GetFilteredOrders(dto);

        _orderRepository.Verify(r => r.GetAllBy(
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
                3,
                10),
            Times.Once);
    }

    #region ExecuteAction

    [TestMethod]
    public void ExecuteAction_WhenOrderDoesNotExist_ThrowsException()
    {
        var id = Guid.NewGuid();
        var token = Guid.NewGuid();

        var session = CreateAdminSession(token);

        _sessionRepository
            .Setup(r => r.GetByToken(token))
            .Returns(session);

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Order, bool>>>()))
            .Returns((Order?)null);

        Assert.ThrowsException<KeyNotFoundException>(
            () => _orderService.ExecuteAction(
                id,
                OrderAction.Prepare,
                token));
    }
    [TestMethod]
    public void ExecuteAction_Prepare_ChangesStatusToPrepared()
    {
        var order = CreateOrder();
        var token = Guid.NewGuid();

        var session = CreateAdminSession(token);

        _sessionRepository
            .Setup(r => r.GetByToken(token))
            .Returns(session);

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Order, bool>>>()))
            .Returns(order);

        _orderRepository
            .Setup(r => r.Update(It.IsAny<Order>()));

        var result = _orderService.ExecuteAction(
            order.OrderNumber,
            OrderAction.Prepare,
            token);

        Assert.AreEqual(OrderStatus.Prepared, result.Status);

        _orderRepository.Verify(
            r => r.Update(It.IsAny<Order>()),
            Times.Once);
    }


    [TestMethod]
    public void ExecuteAction_WhenTransitionIsInvalid_ThrowsException()
    {
        var order = CreateOrder();

        var token = Guid.NewGuid();

        var session = CreateDispatcherSession(token);

        _sessionRepository
            .Setup(r => r.GetByToken(token))
            .Returns(session);

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Order, bool>>>()))
            .Returns(order);

        Assert.ThrowsException<ConflictException>(
            () => _orderService.ExecuteAction(
                order.OrderNumber,
                OrderAction.MarkAsNotDelivered,
                token));
    }

    #endregion

    [TestMethod]
    public void ExecuteAction_Cancel_WithAdmin_DoesNotThrow()
    {
        var order = CreateOrder();
        var token = Guid.NewGuid();

        var session = CreateAdminSession(token);

        _sessionRepository
            .Setup(r => r.GetByToken(token))
            .Returns(session);

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Order, bool>>>()))
            .Returns(order);

        _orderRepository
            .Setup(r => r.Update(It.IsAny<Order>()));

        var result = _orderService.ExecuteAction(
            order.OrderNumber,
            OrderAction.Cancel,
            token);

        Assert.AreEqual(OrderStatus.Cancelled, result.Status);
    }

    [TestMethod]
    public void ExecuteAction_Cancel_WithDispatcher_ThrowsUnauthorizedAccessException()
    {
        var order = CreateOrder();
        var token = Guid.NewGuid();

        var session = CreateDispatcherSession(token);

        _sessionRepository
            .Setup(r => r.GetByToken(token))
            .Returns(session);

        Assert.ThrowsException<UnauthorizedAccessException>(
            () => _orderService.ExecuteAction(
                order.OrderNumber,
                OrderAction.Cancel,
                token));
    }

    [TestMethod]
    public void ExecuteAction_Prepare_WithDispatcher_IsAllowed()
    {
        var order = CreateOrder();
        var token = Guid.NewGuid();

        var session = CreateDispatcherSession(token);

        _sessionRepository
            .Setup(r => r.GetByToken(token))
            .Returns(session);

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Order, bool>>>()))
            .Returns(order);

        _orderRepository
            .Setup(r => r.Update(It.IsAny<Order>()));

        var result = _orderService.ExecuteAction(
            order.OrderNumber,
            OrderAction.Prepare,
            token);

        Assert.AreEqual(OrderStatus.Prepared, result.Status);
    }

    private Session CreateAdminSession(Guid token)
    {
        var admin = CreateAdminUser();

        var session = new Session(admin)
        {
            Token = token
        };

        return session;
    }

    private Session CreateDispatcherSession(Guid token)
    {
        var dispatcher = CreateDispatcherUser();

        var session = new Session(dispatcher)
        {
            Token = token
        };

        return session;
    }

    private User CreateAdminUser()
    {
        return new User(
            "Admin",
            "Tester",
            "admin@test.com",
            "099123456",
            "Password!Test1A",
            Role.Admin,
            true);
    }

    private User CreateDispatcherUser()
    {
        return new User(
            "Dispatcher",
            "Tester",
            "dispatcher@test.com",
            "099765432",
            "Password!Test2A",
            Role.Dispatcher,
            true);
    }

    [TestMethod]
    public void DelayOrder_ShouldChangeStatusToDelayed()
    {
        var order = CreateOrder();

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .Returns(order);

        SetupOrderUpdate();

        var result = _orderService.DelayOrder(order.OrderNumber);

        Assert.AreEqual(OrderStatus.Delayed, result.Status);
    }

    [TestMethod]
    [ExpectedException(typeof(KeyNotFoundException))]
    public void DelayOrder_WhenOrderDoesNotExist_ShouldThrowException()
    {
        var orderNumber = Guid.NewGuid();

        _orderRepository
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>()))
            .Returns((Order?)null);

        _orderService.DelayOrder(orderNumber);
    }
}