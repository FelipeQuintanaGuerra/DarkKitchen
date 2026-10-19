using Domain;
using Domain.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Repository;

namespace RepositoriesTests;

[TestClass]
public class OrderRepositoryTests : RepositoryTestBase
{
    private OrderRepository _orderRepository = null!;

    [TestInitialize]
    public void InitializeRepository()
    {
        _orderRepository = new OrderRepository(Context);
    }

    private static User CreateCustomer(string email)
    {
        return new User(
            "Juan",
            "Perez",
            email,
            "099123456",
            "ValidPassword123!",
            Role.Client);
    }

    private ShippingType CreateShippingType(string? name = null, decimal cost = 250)
    {
        var shippingType = new ShippingType(
            name ?? $"Tipo de envío {Guid.NewGuid()}",
            cost
        );

        Context.ShippingTypes.Add(shippingType);
        Context.SaveChanges();

        return shippingType;
    }

    private Order CreateOrder(Guid customerId)
    {
        ShippingType shippingType = CreateShippingType();

        return new Order(
            customerId,
            shippingType,
            new DeliveryAddress("Test", "123", "1"));
    }

    [TestMethod]
    public void GetBy_WhenOrderExists_ReturnsOrderWithLines()
    {
        var customer = new User("Juan", "Perez", "getby@test.com", "099123456", "ValidPassword123!", Role.Client);

        Context.Users.Add(customer);
        Context.SaveChanges();

        var category = new Category("Category");
        var commercialLine = new CommercialLine("Commercial Line");

        var product1 = new Product(
            "CODE01",
            "Product One",
            "Product description one",
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture("image1.jpg", 100) },
            100
        );

        var product2 = new Product(
            "CODE02",
            "Product Two",
            "Product description two",
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture("image2.jpg", 100) },
            200
        );

        Context.Categories.Add(category);
        Context.CommercialLines.Add(commercialLine);
        Context.Products.Add(product1);
        Context.Products.Add(product2);
        Context.SaveChanges();

        var order = new Order(customer.Id, CreateShippingType(), new DeliveryAddress("Test", "123", "1"));
        order.AddLine(new OrderLine(product1.Id, 2, 100, 0));
        order.AddLine(new OrderLine(product2.Id, 1, 200, 0));

        Context.Orders.Add(order);
        Context.SaveChanges();

        Context.ChangeTracker.Clear();

        var result = _orderRepository.GetBy(o => o.OrderNumber == order.OrderNumber);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Lines.Count);
    }

    [TestMethod]
    public void GetAllBy_WhenOrdersExist_ReturnsOrdersWithLines()
    {
        var customer = new User("Juan", "Perez", "getall@test.com", "099123456", "ValidPassword123!", Role.Client);

        Context.Users.Add(customer);
        Context.SaveChanges();

        var category = new Category("Category");
        var commercialLine = new CommercialLine("Commercial Line");

        var product = new Product(
            "CODE03",
            "Product Three",
            "Product description three",
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture("image3.jpg", 100) },
            100
        );

        Context.Categories.Add(category);
        Context.CommercialLines.Add(commercialLine);
        Context.Products.Add(product);
        Context.SaveChanges();

        var order = new Order(customer.Id, CreateShippingType(), new DeliveryAddress("Test", "123", "1"));
        order.AddLine(new OrderLine(product.Id, 2, 100, 0));

        Context.Orders.Add(order);
        Context.SaveChanges();

        Context.ChangeTracker.Clear();

        var result = _orderRepository.GetAllBy(o => o.CustomerId == customer.Id);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, result[0].Lines.Count);
    }

    [TestMethod]
    public void GetBy_WhenOrderIsRetrieved_CanPreparePendingOrder()
    {
        var customer = new User("Juan", "Perez", "state@test.com", "099123456", "ValidPassword123!", Role.Client);

        Context.Users.Add(customer);
        Context.SaveChanges();

        var order = new Order(customer.Id, CreateShippingType(), new DeliveryAddress("Test", "123", "1"));

        Context.Orders.Add(order);
        Context.SaveChanges();

        Context.ChangeTracker.Clear();

        var result = _orderRepository.GetBy(o => o.OrderNumber == order.OrderNumber);

        Assert.IsNotNull(result);

        result.Prepare();

        Assert.AreEqual(OrderStatus.Prepared, result.Status);
    }

    [TestMethod]
    public void GetAllBy_WhenOrdersAreRetrieved_CanPreparePendingOrders()
    {
        var customer = new User("Juan", "Perez", "states@test.com", "099123456", "ValidPassword123!", Role.Client);

        Context.Users.Add(customer);
        Context.SaveChanges();

        var order = new Order(customer.Id, CreateShippingType(), new DeliveryAddress("Test", "123", "1"));

        Context.Orders.Add(order);
        Context.SaveChanges();

        Context.ChangeTracker.Clear();

        var result = _orderRepository.GetAllBy(o => o.CustomerId == customer.Id);

        Assert.AreEqual(1, result.Count);

        result[0].Prepare();

        Assert.AreEqual(OrderStatus.Prepared, result[0].Status);
    }

    [TestMethod]
    public void GetBy_WhenOrderDoesNotExist_ReturnsNull()
    {
        var nonExistentOrderNumber = Guid.NewGuid();

        var result = _orderRepository.GetBy(o => o.OrderNumber == nonExistentOrderNumber);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetAllBy_WhenNoOrdersExist_ReturnsEmptyList()
    {
        var nonExistentCustomerId = Guid.NewGuid();

        var result = _orderRepository.GetAllBy(o => o.CustomerId == nonExistentCustomerId);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetAllBy_WhenPageSizeIsOne_ReturnsOnlyOneOrder()
    {
        var customer = CreateCustomer("pagination1@test.com");
        Context.Users.Add(customer);
        Context.SaveChanges();

        Context.Orders.Add(CreateOrder(customer.Id));
        Context.Orders.Add(CreateOrder(customer.Id));
        Context.Orders.Add(CreateOrder(customer.Id));
        Context.SaveChanges();

        Context.ChangeTracker.Clear();

        var result = _orderRepository.GetAllBy(
            o => o.CustomerId == customer.Id,
            page: 1,
            pageSize: 1);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void GetAllBy_WhenSecondPageIsRequested_SkipsFirstPage()
    {
        var customer = CreateCustomer("pagination2@test.com");
        Context.Users.Add(customer);
        Context.SaveChanges();

        Context.Orders.Add(CreateOrder(customer.Id));
        Context.Orders.Add(CreateOrder(customer.Id));
        Context.SaveChanges();

        Context.ChangeTracker.Clear();

        var firstPage = _orderRepository.GetAllBy(
            o => o.CustomerId == customer.Id,
            page: 1,
            pageSize: 1);

        var secondPage = _orderRepository.GetAllBy(
            o => o.CustomerId == customer.Id,
            page: 2,
            pageSize: 1);

        Assert.AreEqual(1, firstPage.Count);
        Assert.AreEqual(1, secondPage.Count);
        Assert.AreNotEqual(firstPage[0].OrderNumber, secondPage[0].OrderNumber);
    }

    [TestMethod]
    public void GetAllBy_WhenPageIsLessThanOne_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            _orderRepository.GetAllBy(
                o => true,
                page: 0,
                pageSize: 20));
    }

    [TestMethod]
    public void GetAllBy_WhenPageSizeIsLessThanOne_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            _orderRepository.GetAllBy(
                o => true,
                page: 1,
                pageSize: 0));
    }

    [TestMethod]
    public void GetAllUnpagedBy_WhenMultipleOrdersMatch_ReturnsAllMatchingOrders()
    {
        var customer = CreateCustomer("unpaged@test.com");
        Context.Users.Add(customer);
        Context.SaveChanges();

        Context.Orders.Add(CreateOrder(customer.Id));
        Context.Orders.Add(CreateOrder(customer.Id));
        Context.Orders.Add(CreateOrder(customer.Id));
        Context.SaveChanges();

        Context.ChangeTracker.Clear();

        var result = _orderRepository.GetAllUnpagedBy(o => o.CustomerId == customer.Id);

        Assert.AreEqual(3, result.Count);
    }

    [TestMethod]
    public void GetBy_WhenOrderExists_ReturnsOrderWithShippingType()
    {
        var customer = CreateCustomer("shipping@test.com");
        Context.Users.Add(customer);
        Context.SaveChanges();

        var shippingType = CreateShippingType("Envío test", 250);


        var order = new Order(
            customer.Id,
            shippingType,
            new DeliveryAddress("Test", "123", "1"));

        Context.Orders.Add(order);
        Context.SaveChanges();

        Context.ChangeTracker.Clear();

        var result = _orderRepository.GetBy(o => o.OrderNumber == order.OrderNumber);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.ShippingType);
        Assert.AreEqual(shippingType.Id, result.ShippingTypeId);
        Assert.AreEqual("Envío test", result.ShippingType.Name);
    }

    [TestMethod]
    public void GetAllBy_WhenOrdersExist_ReturnsOrdersWithShippingType()
    {
        var customer = CreateCustomer("shipping-list@test.com");
        Context.Users.Add(customer);
        Context.SaveChanges();

        var shippingType = CreateShippingType("Envío lista", 300);

        var order = new Order(
            customer.Id,
            shippingType,
            new DeliveryAddress("Test", "123", "1"));

        Context.Orders.Add(order);
        Context.SaveChanges();

        Context.ChangeTracker.Clear();

        var result = _orderRepository.GetAllBy(o => o.CustomerId == customer.Id);

        Assert.AreEqual(1, result.Count);
        Assert.IsNotNull(result[0].ShippingType);
        Assert.AreEqual(shippingType.Id, result[0].ShippingTypeId);
        Assert.AreEqual("Envío lista", result[0].ShippingType.Name);
    }

    [TestMethod]
    public void GetAllUnpagedBy_WhenOrdersExist_ReturnsOrdersWithShippingType()
    {
        var customer = CreateCustomer("unpaged-shipping@test.com");
        Context.Users.Add(customer);
        Context.SaveChanges();

        var shippingType = CreateShippingType("Envío unpaged", 350);

        Context.Orders.Add(new Order(
            customer.Id,
            shippingType,
            new DeliveryAddress("Test", "123", "1")));

        Context.Orders.Add(new Order(
            customer.Id,
            shippingType,
            new DeliveryAddress("Test", "123", "1")));

        Context.SaveChanges();

        Context.ChangeTracker.Clear();

        var result = _orderRepository.GetAllUnpagedBy(o => o.CustomerId == customer.Id);

        Assert.AreEqual(2, result.Count);

        Assert.IsTrue(result.All(o => o.ShippingType != null));

        Assert.IsTrue(result.All(o => o.ShippingTypeId == shippingType.Id));

        Assert.IsTrue(result.All(o => o.ShippingType.Name == "Envío unpaged"));
    }
}